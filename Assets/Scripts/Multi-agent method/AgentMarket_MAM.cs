using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMarket_MAM : MonoBehaviour
{
    public List<Node> hasToBeVisited;
    public bool isGenerated = false;
    private Agent_MAM[] listAgent;
    private AgentManager_MAM agentManager;

    // Start is called before the first frame update
    void Start()
    {
        hasToBeVisited = new List<Node>();
        agentManager = FindObjectOfType<AgentManager_MAM>();
        listAgent = FindObjectsOfType(typeof(Agent_MAM)) as Agent_MAM[];
        foreach(Agent_MAM agent in listAgent)
        {
            print(agent.ToString());
        }
        isGenerated = true;
        print("| MARKET | Agent market is setup.");
    }

    // Update is called once per frame
    void Update()
    {
      for(int i = 0; i < hasToBeVisited.Count; i++)
        {
            WarnAgent(hasToBeVisited[0]);
            agentManager.SetNodeToTrue(hasToBeVisited[0]);
            hasToBeVisited.RemoveAt(0);

        }
    }

    void WarnAgent(Node node)
    {
        Agent_MAM bestAgent = FindBestAgentByDist(node);
        if (bestAgent != null)
        {
            bestAgent.priorityQueue.Add(node);
            print("| MARKET | Warning agent about (" + node.pos.Item1 + "," + node.pos.Item2 + ").");
        }
    }

    // This method have to return the best agent for the given node (Test the distance from agent to node, and the best one is returned)
    Agent_MAM FindBestAgentByDist(Node node)
    {
        // get all agents of the scene
        //
        Vector3 posNode = new Vector3(node.pos.Item1,0,node.pos.Item2);
        float dist = Mathf.Infinity;
        int iteration = 0;
        int bestAgentIteration = -1;

        foreach (Agent_MAM agent in listAgent)
        {
            print(agent.ToString() + " : " + Vector3.Distance(agent.transform.position, posNode));
            if (dist > Vector3.Distance(agent.transform.position, posNode))
            {
                dist = Vector3.Distance(agent.transform.position, posNode);
                bestAgentIteration = iteration;
            }


            iteration++;
        }

        print("Agent choosen : " + listAgent[bestAgentIteration]);
        if (listAgent[bestAgentIteration] != null)
        {
            return listAgent[bestAgentIteration];
        }
        return null;
    }

    public void CheckNeighbour(Node node)
    {
        // Inutile car testé au tour d'avant
        /*
        // Check if node is in list
        if (hasToBeVisited.Contains(node))
        {
            hasToBeVisited.Remove(node);
            agentManager.managerTool[node] = false;
        }*/
        foreach(Edge edge in node.neighs)
        {
            if (hasToBeVisited.Contains(edge.to))
            {
                hasToBeVisited.Remove(edge.to);
                agentManager.managerTool[edge.to] = false;
            }
        }
    }
}
