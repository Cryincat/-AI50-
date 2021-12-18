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

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<LoadGraph>().isGenerated);
        Generate();
        isGenerated = true;
        yield return null;
    }

    public void Generate()
    {
        graph = new Graph();

        sol = FindObjectOfType<LoadGraph>().gameObject;

        nodeComponentDict = new Dictionary<Node, NodeComponent>();
        //agents = FindObjectsOfType<Agent>().ToList();
        int numNode = 0;

        //Case where the map is already existing
        foreach (Transform item in sol.transform)
        {
            //print("there is a a transform");
            var nc = item.GetComponent<NodeComponent>();
            if (nc)
            {
                //print("there is a a nc");
                //If the object has a NodeComponent, we generate a node in the graph at its position
                (int, int) pos = ((int)item.position.x, (int)item.position.z);
                Node node = new Node(pos);
                graph.nodes.Add(pos, node);
                //nc.node = node;
                nodeComponentDict.Add(node, nc);
            }
        }

        Debug.Log("Nombre de nodes : " + graph.nodes.Count);
        //print("nb node :" + graph.nodes.Count);

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
                Edge edge = new Edge(node, to, new Vector2(node.pos.Item1 - to.pos.Item1, node.pos.Item2 - to.pos.Item2).magnitude);
                node.neighs.Add(edge);
                graph.edges.Add(edge);
            }
            //Debug.Log("Voisins de Node" + numNode + " (posX: " + node.pos.Item1 + ", posY:" + node.pos.Item2 + "): " + neighs.Count);
            numNode++;
            /*foreach (var agent in agents)
            {
                agent.node = graph.nodes.Values.OrderBy(x => Vector3.Distance(agent.transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
            }*/
        }
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
