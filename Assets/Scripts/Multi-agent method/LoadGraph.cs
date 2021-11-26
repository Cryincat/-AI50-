using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using System.Linq;


public class LoadGraph : MonoBehaviour
{

    public GameObject prefabSol;
    public GameObject prefabEdge;
    public bool isGenerated = false;
    public Graph graph;
    public GameObject parent;
    private Dictionary<Node, NodeComponent> nodeComponentDict;


    // Start is called before the first frame update
    void Start()
    {
        nodeComponentDict = new Dictionary<Node, NodeComponent>();
        string path = Directory.GetCurrentDirectory() + "/Assets/Data/";
        string textFileName = "graph_1.txt";
        path = path + textFileName;

        graph = createGraph(path);
        spawnMap(graph, parent);

        print(graph);

        print("| LoadGraph | Génération des variables terminée.");
        isGenerated = true;
    }

    void Update()
    {
        if (isGenerated)
        {
            foreach (var node in graph.nodes.Values)
            {
                node.timeSinceLastVisit += Time.deltaTime;
            }
            /*foreach (var agent in agents)
            {
                //agent.node.timeSinceLastVisit = 0f;
                foreach (var e in agent.node.neighs)
                {
                    e.to.timeSinceLastVisit = 0f;
                }
            }*/

            foreach (var nc in nodeComponentDict.Values)
            {
                float value = Math.Max(0, 1 - .01f * nc.node.timeSinceLastVisit);
                Color color = new Color(1, value, value, 1);
                List<Color> colors = new List<Color>();

                foreach (var v in nc.meshFilter.mesh.vertices)
                {
                    colors.Add(color);
                }
                nc.meshFilter.mesh.colors = colors.ToArray();
            }
        }
    }


    public void print (Graph graph)
    {
        foreach(Node node in graph.nodes.Values)
        {
            print("Node : (" + node.pos.Item1 + "," + node.pos.Item2 + ")");
        }
        foreach(Edge edge in graph.edges)
        {
            print("Edge : from (" + edge.from.pos.Item1 + "," + edge.from.pos.Item2 + ") to (" + edge.to.pos.Item1 + "," + edge.to.pos.Item2 + ") : cost = " + edge.cost);
        }
    }

    void spawnMap(Graph graph, GameObject parent)
    {
        print("| LoadGraph | Generating world...");
        foreach (Node node in graph.nodes.Values)
        {
            
            GameObject gameObject = Instantiate(prefabSol, node.realPos, Quaternion.identity, parent.transform);
            NodeComponent nc = gameObject.GetComponent<NodeComponent>();
            nc.node = node;
            nodeComponentDict.Add(node, nc);
        }
        // Spawn l'edge visuellement
        foreach (Edge edge in graph.edges)
        {
            spawnEdgeOnMap(edge.from.realPos, edge.to.realPos);
        }
        print("| LoadGraph | Wolrd ready.");
    }

    void spawnEdgeOnMap(Vector3 from, Vector3 to)
    {
        float pas = 0.02f;
        float dist = Vector3.Distance(from, to);
        float decount = dist;
        while(decount > 0)
        {
            Vector3 position = Vector3.MoveTowards(from, to, decount);
            GameObject gameObject = Instantiate(prefabEdge, position, Quaternion.identity, parent.transform);
            gameObject.GetComponent<Renderer>().material.color = Color.white;
            decount -= pas;
        }
    }

    private Graph createGraph(string path)
    {
        print("| LoadGraph | Creating graph from file...");
        List<string> lines = File.ReadAllLines(path).ToList<string>();
        Graph graph = new Graph();
        graph.edges = new List<Edge>();

        createNode(graph, lines[0]);
        lines.RemoveAt(0);
        foreach(string line in lines)
        {
            string[] lineSplitted = line.Split(';');
            string[] nodeInStringSplitted = lineSplitted[0].Split(',');
            string[] edgesInStringSplitted = lineSplitted[1].Split(':');

            // récupération du couple x,y du node
            int x = int.Parse(nodeInStringSplitted[0]);
            int y = int.Parse(nodeInStringSplitted[1]);
            Node from = graph.nodes[(x, y)];

            // génération et attachements des edges
            foreach(string edgeInString in edgesInStringSplitted)
            {
                string[] edgeInStringSplitted = edgeInString.Split(',');
                x = int.Parse(edgeInStringSplitted[0]);
                y = int.Parse(edgeInStringSplitted[1]);
                Node to = graph.nodes[(x, y)];
                float cost = Vector3.Distance(from.realPos, to.realPos);
                Edge edgeToAdd = new Edge(from, to, cost);
                from.neighs.Add(edgeToAdd);
                graph.edges.Add(edgeToAdd);
            }
        }
        return graph;
    }



    private void createNode(Graph graph, string line)
    {
        Dictionary<(int, int), Node> nodes = new Dictionary<(int, int), Node>();
        graph.nodes = nodes;
        string[] stringNode = line.Split(';');
        foreach (string element in stringNode)
        {
            string[] couple = element.Split(',');
            int x = int.Parse(couple[0]);
            int y = int.Parse(couple[1]);
            Node node = new Node((x, y));
            graph.nodes.Add((x, y), node);
        }
    }

}
