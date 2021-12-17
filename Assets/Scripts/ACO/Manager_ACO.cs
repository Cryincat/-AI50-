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

    public Graph graph_base;
    public Graph graph_complete;

    public List<fourmis> ListAgent;
    public AStar_ACO AStar_;
    public Agent_ACO Agen;
    public List<Node> realTimeNodes;

    public bool colonyGenerated;

    public int nbColony;
    public float costChemin;
    IEnumerator Start()
    {
        nbColony = 5;
        costChemin = 0;
        colonyGenerated = false;

        //Creer un graph
        GameObject gameObject = Instantiate(ACO, Vector3.zero, Quaternion.identity); 

        //Rendre graph complet
        GameObject gameObject1 = Instantiate(complete_graph, Vector3.zero, Quaternion.identity);

        //lancer colony sur le graph
        for (int i = 0; i < nbColony; i++)
        {

            GameObject origin = Instantiate(colony, Vector3.zero, Quaternion.identity);
            origin.name = ("Colony_" + i);
            ColonyMulti colonys = origin.GetComponent<ColonyMulti>();
            yield return new WaitUntil(() => FindObjectOfType<ColonyMulti>().isGenerated);

            if (colonys.coutCheminSave < costChemin || costChemin == 0)
            {
                costChemin = colonys.coutCheminSave;
             
                for (int u = (i-1); u >= 0; u--)
                {
                    GameObject destroyGameObject = GameObject.Find("Colony_" + u);
                    Destroy(destroyGameObject);
                }
            }

            else
            {
                GameObject test = GameObject.Find("Colony_" + i);
                Destroy(test);
            }
        }


        colonyGenerated = true;

        // A* sur le path
        GameObject gameObject3 = Instantiate(AStar_AC, Vector3.zero, Quaternion.identity);

        //control des agents
        yield return new WaitUntil(() => FindObjectOfType<AStar_ACO>().isGenerated);

        ListAgent = new List<fourmis>();
        AStar_ = FindObjectOfType<AStar_ACO>();
        ListAgent = AStar_.list_agent;
       

        for (int i = 0; i<ListAgent.Count; i++)
        {
            realTimeNodes = new List<Node>();
            
            foreach(var b in ListAgent[i].listCheminReel) { 
                realTimeNodes.Add(b);
                print(b.pos);
            }

            GameObject Agent = Instantiate(Agents, Vector3.zero, Quaternion.identity);
            Agent.name = ("Agent_" + i);
            
            Node startPoint = realTimeNodes[0];
            Agent_ACO agent__ = Agent.GetComponent<Agent_ACO>();
            agent__.startPoint = startPoint;
            yield return new WaitUntil(() => FindObjectOfType<Agent_ACO>().isGenerated);
        }
    }

}
