using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_ACO : MonoBehaviour
{
    public GameObject parent_ACO;
    public GameObject ACO;
    public GameObject complete_graph;
    public GameObject colony;
    public GameObject AStar_AC;
    public GameObject Agents;
    public GameObject parent_agent;
    //public LoadGraph loadingGraph;
    // Start is called before the first frame update
    //public CompleteGraph completingGraph;

    public Graph graph_base;
    public Graph graph_complete;

    public List<fourmis> ListAgent;
    public AStar_ACO AStar_;
    public Agent_ACO Agen;
    public List<Node> realTimeNodes;
    IEnumerator Start()
    {

        //Creer un graph
        GameObject gameObject = Instantiate(ACO, Vector3.zero, Quaternion.identity); 

        //Rendre graph complet
        GameObject gameObject1 = Instantiate(complete_graph, Vector3.zero, Quaternion.identity);

        //lancer colony sur le graph
        GameObject gameObject2 = Instantiate(colony, Vector3.zero, Quaternion.identity);

        // A* sur le path
        GameObject gameObject3 = Instantiate(AStar_AC, Vector3.zero, Quaternion.identity);

        //control des agents
        yield return new WaitUntil(() => FindObjectOfType<AStar_ACO>().isGenerated);

        ListAgent = new List<fourmis>();
        AStar_ = FindObjectOfType<AStar_ACO>();
        ListAgent = AStar_.list_agent;

        for (int i = 0; i<ListAgent.Count; i++)
        //foreach (var agent in ListAgent)
        {
            
            realTimeNodes = new List<Node>();
            //realTimeNodes = agent.listCheminReel;
            
            foreach(var b in ListAgent[i].listCheminReel) { 
                realTimeNodes.Add(b);
                
            }

            GameObject Agent = Instantiate(Agents, Vector3.zero, Quaternion.identity);
            Agent.name = ("Agent_" + i);
            
            Node startPoint = realTimeNodes[0];
            Agent_ACO agent__ = Agent.GetComponent<Agent_ACO>();
            agent__.startPoint = startPoint;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
