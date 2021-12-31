using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using System.Linq;

public class CompleteGraph : MonoBehaviour
{
    private Dictionary<Node, NodeComponent> nodeComponentDict;
    //public List<Agent> agents;
    public GameObject sol;
    public Graph graph;
    //public bool isGenerated;
    public bool isGenerated = false;
    public bool generateGood;
    public LoadGraph co_graph;
    private Graph graph2;
    public int nbEdgeModif;
    public GameObject Astar_complete;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<LoadGraph>().isGenerated);
        co_graph = FindObjectOfType<LoadGraph>();
        graph2 = co_graph.graph;
        nbEdgeModif = 0;
        //generateGood = false;


        //Generate();

        graph = new Graph();

        sol = FindObjectOfType<LoadGraph>().gameObject;

        nodeComponentDict = new Dictionary<Node, NodeComponent>();
        int numNode = 0;

        //Case where the map is already existing
        foreach (Transform item in sol.transform)
        {
            var nc = item.GetComponent<NodeComponent>();
            if (nc)
            {
                //If the object has a NodeComponent, we generate a node in the graph at its position
                (int, int) pos = ((int)item.position.x, (int)item.position.z);
                Node node = new Node(pos);
                graph.nodes.Add(pos, node);
                //nc.node = node;
                nodeComponentDict.Add(node, nc);
            }
        }

        Debug.Log("Nombre de nodes : " + graph.nodes.Count);

        foreach (Node node in graph.nodes.Values)
        {
            List<Node> neighs = new List<Node>();

            foreach (Node neigh in graph.nodes.Values)
            {
                graph.nodes.TryGetValue((neigh.pos.Item1, neigh.pos.Item2), out Node Neighb);
                //test if the neighbour is the current node
                if (Neighb.pos.Item1 == node.pos.Item1 && Neighb.pos.Item2 == node.pos.Item2)
                {
                }
                else
                {
                    neighs.Add(Neighb);
                }
            }

            neighs.RemoveAll(item => item == null);

            foreach (Node to in neighs)
            {
                GameObject gameObject1 = Instantiate(Astar_complete, Vector3.zero, Quaternion.identity);
                AStar_ACO_complete Astareuh = gameObject1.GetComponent<AStar_ACO_complete>();
                Astareuh.name = ("Astar_complete");
                Astareuh.began = node;
                Astareuh.Endeuh = to;
                yield return new WaitUntil(() => FindObjectOfType<AStar_ACO_complete>().isGenerated);

                Edge edge = new Edge(node, to, Astareuh.costeuh);// Il faut surtout que cost != 0
                node.neighs.Add(edge);
                graph.edges.Add(edge);
                nbEdgeModif++;
                Destroy(gameObject1);

            }
            numNode++;
        }

        //while (generateGood == false) { }
        isGenerated = true;
        yield return null;
    }

    public IEnumerator Generate()
    {
        yield return null;
    }
    
    // Update is called once per frame
    void Update()
    {
    }
}
