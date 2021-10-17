using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QLearningPatrolling : MonoBehaviour
{

    private GraphGenerator graphGeneratorInstance;
    private DataQLearning dataQLearningInstance;
    private Graph graph;
    private int nbState;
    private Dictionary<(Node,int) , float> Q;
    private float time;
    private List<int> listAction;
    private Node firstState;
    private bool ready = false;
    private System.Random random;
    private Vector3 newPosition;
    private float[] factor;
    private int saveIterator = 0;
    private Vector3 diff;
    public int nbAction;
    public Component agent;
    public float gamma = 0.9f;
    public float epsilonRange = 0.3f;
    public float vitesse = 4.0f;
    private bool isSaving = false;
    private float rewardValue = 1;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        random = new System.Random();
        newPosition = new Vector3();
        diff = new Vector3();
        graphGeneratorInstance = GameObject.Find("Sols").GetComponent<GraphGenerator>();
        dataQLearningInstance = GameObject.Find("Sols").GetComponent<DataQLearning>();
        yield return new WaitUntil(() => graphGeneratorInstance.isGenerated);
        yield return new WaitUntil(() => dataQLearningInstance.isGenerated);
        print("Ca y est on peut commencer à apprendre !");
        Q = dataQLearningInstance.Q;
        listAction = new List<int> { 0,1,2,3 };
        graph = graphGeneratorInstance.graph;
        //nbState = graph.nodes.Count;
        //setQToZero();
        //setTimeSinceLastVisit(graph);
        firstState = graph.nodes[((int)agent.transform.position.x, (int)agent.transform.position.z)];
        firstState.timeSinceLastVisit = 0;
        newPosition = agent.transform.position;
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (ready == true)
        {
            // Permet d'avoir un désordre dans les actions (0,1,2,3 => 1,2,3,0 => 2,3,0,1 => 3,0,1,2 => ...)
            listAction.Add(listAction[0]);
            listAction.RemoveAt(0);          
            
            // Déplacement vers la nouvelle position
            if (isPositionReach(agent.transform.position, newPosition))
            {
                // Run d'une itération du Q learning
                firstState = runQLearning(firstState, gamma, epsilonRange, listAction, graph);
                saveIterator++;
                // Calcul de la nouvelle position vers laquelle se diriger pour l'agent
                newPosition.Set(firstState.pos.Item1, agent.transform.position.y, firstState.pos.Item2);
                diff = newPosition - agent.transform.position;
            }

            // Mouvement de l'agent
            
            Vector3 diffTest = newPosition - agent.transform.position;
            agent.transform.Translate(diff * Time.deltaTime * vitesse);

            // Toutes les 200 itérations, on save Q dans via le script DataQLearning dans le fichier text dédié à celà.
            if (saveIterator > 200)
            {
                print("Save Q");
                saveIterator = 0;

                // On met en pause le script en question le temps que la sauvegarde se fasse
                //Time.timeScale = 0;
                dataQLearningInstance.isSaved = false;
                dataQLearningInstance.save();
                StartCoroutine(pauseUntilSaveIsDone());

            }
        }
    }

    IEnumerator pauseUntilSaveIsDone()
    {
        yield return new WaitUntil(() => dataQLearningInstance.isSaved);
    }

    bool isPositionReach(Vector3 agentPosition, Vector3 positionToReach)
    {
        Vector3 diff = agentPosition - positionToReach;
        if (diff.x <= 0.2 && diff.x >= -0.2 && diff.z <= 0.2 && diff.z >= -0.2)
        {
            return true;
        }
        return false;
    }

    // Method that initiate each couple (node,action) to 0
    void setQToZero()
    {
        foreach(Node node in graph.nodes.Values)
        {
            Q.Add((node,0), 0f);
            Q.Add((node, 1), 0f);
            Q.Add((node, 2), 0f);
            Q.Add((node, 3), 0f);
        }

    }
    
    // The reward function : check if the nextState has the best value in "timeSinceLastVisit". If yes, reward = rewardValue, if no, reward  = -10
    float getReward(Node state, int bestAction, Node nextState, Graph graph)
    {
        List<Node> neighbours = new List<Node>();
        for (int i = 0; i <= 4; i++)
        {
            Node temp = getNextState(state, i, graph);
            if (!temp.Equals(state))
            {
                neighbours.Add(temp);
            }
        }

        float lastVisitedValueWithBestAction = nextState.timeSinceLastVisit;
        foreach(Node neighbour in neighbours)
        {
            if (lastVisitedValueWithBestAction < neighbour.timeSinceLastVisit)
            {
                return -1;
            }
        }
        return rewardValue;

    }

    // Method that return the next node after doing this action in actual node
    // Actions are corresponding to : 0 = UP, 1 = DOWN, 2 = LEFT, 3 = RIGHT
    Node getNextState(Node state, int action, Graph graph)
    {
        int x = state.pos.Item1;
        int y = state.pos.Item2;

        if (action == 0)
        {
            x = x + 1;
        }
        else if (action == 1)
        {
            x = x - 1;
        }
        else if (action == 2)
        {
            y = y - 1;
        }
        else if (action == 3)
        {
            y = y + 1;
        }

        if (graph.nodes.ContainsKey((x, y)))
        {
            return graph.nodes[(x, y)];
        }
        return state;
 
    }

    Node runQLearning(Node state,float gamma, float epsilonRange, List<int> listAction, Graph graph)
    {
        int bestAction = getBestAction(state, epsilonRange);
        Node nextState = getNextState(state, bestAction, graph);
        float reward = getReward(state, bestAction, nextState, graph);
        Q[(state, bestAction)] = (1 - gamma) * Q[(state, bestAction)] + gamma * (reward + gamma * getMaxNextState(state,listAction));
        state.timeSinceLastVisit = 0;
        return nextState;
    }

    // Return the best action due to Q values
    int getBestAction(Node state, float epsilonRange)
    {
        float bestActionValue = -99999;
        int bestAction = -3;

        foreach (int action in listAction)
        {
            if (random.NextDouble() < epsilonRange)
            {
                return action;
            }
            if (bestActionValue < Q[(state, action)])
            {
                bestAction = action;
            }
        }
        return bestAction;
    }

    // Return the max value in Q for a given state and all his possible action
    float getMaxNextState(Node state, List<int> listAction)
    {
        List<float> listValue = new List<float>();

        foreach (int i in listAction)
        {
            listValue.Add(Q[(state, i)]);
        }

        return listValue.Max();
    }
}
