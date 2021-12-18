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
    private GameObject parent;
    public string textFileName = "";
    private Dictionary<Node, NodeComponent> nodeComponentDict;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        //yield return new WaitUntil(() => FindObjectOfType<LoadMethod>().isReady);
        parent = GameObject.FindGameObjectWithTag("Sols");
        nodeComponentDict = new Dictionary<Node, NodeComponent>();
        string path = Directory.GetCurrentDirectory() + "/Assets/Data/";

        path = path + textFileName;

        graph = createGraph(path);
        spawnMap(graph, parent);
        setupCamera();

        isGenerated = true;
        yield return null;
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

    void setupCamera()
    {
        Camera camera = FindObjectOfType<Camera>();
        Vector3 posNW = getNorthWesternNodePos(graph);
        Vector3 posSE = getSouthEsternNodePos(graph);
        Vector3 cameraPosition = Vector3.Lerp(posNW, posSE, 0.5f);
        float heigh = Vector3.Distance(posNW, posSE);
        cameraPosition.y = (float) (heigh + (heigh * 0.05));
        camera.transform.position = cameraPosition;
    }

    Vector3 getNorthWesternNodePos(Graph graph)
    {
        Vector3 actualBestPos = graph.nodes.First().Value.realPos;
        float actualDist = Vector3.Distance(actualBestPos, Vector3.zero);
        float xPos = graph.nodes.First().Value.pos.Item1;
        float yPos = graph.nodes.First().Value.pos.Item2;

        foreach (Node node in graph.nodes.Values)
        {
            float xNodePos = node.pos.Item1;
            float yNodePos = node.pos.Item2;
            if (Vector3.Distance(node.realPos,Vector3.zero) > actualDist && xNodePos <= xPos && yNodePos >= yPos)
            {
                actualBestPos = node.realPos;
                xPos = xNodePos;
                yPos = yNodePos;
                actualDist = Vector3.Distance(Vector3.zero, node.realPos);
            }
        }
        return actualBestPos;
    }

    Vector3 getSouthEsternNodePos(Graph graph)
    {
        Vector3 actualBestPos = graph.nodes.First().Value.realPos;
        float actualDist = Vector3.Distance(actualBestPos, Vector3.zero);
        float xPos = graph.nodes.First().Value.pos.Item1;
        float yPos = graph.nodes.First().Value.pos.Item2;

        foreach (Node node in graph.nodes.Values)
        {
            float xNodePos = node.pos.Item1;
            float yNodePos = node.pos.Item2;
            if (Vector3.Distance(node.realPos, Vector3.zero) > actualDist && xNodePos >= xPos && yNodePos <= yPos)
            {
                actualBestPos = node.realPos;
                xPos = xNodePos;
                yPos = yNodePos;
                actualDist = Vector3.Distance(Vector3.zero, node.realPos);
            }
        }
        return actualBestPos;
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
        print("| LoadGraph | Spawning map...");
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
            spawnEdgeOnMap2(edge.from.realPos, edge.to.realPos);
        }
        print("| LoadGraph | Map ready.");
    }

    void spawnEdgeOnMap2 (Vector3 from, Vector3 to)
    {
        float edgeSize = Vector3.Distance(from, to);
        Vector3 middlePoint = Vector3.Lerp(from, to, 0.5f);
        Vector3 baseRepere = new Vector3(1,0,0);
        Quaternion edgeRotation = Quaternion.FromToRotation(baseRepere, (to - from));
     
        GameObject newEdge = Instantiate(prefabEdge, middlePoint, edgeRotation, parent.transform);
        //newEdge.GetComponent<Renderer>().material.SetColor("edgeColor", Color.green);
        // Changement de la taille de l'edge
        Vector3 newScale = new Vector3(edgeSize, newEdge.transform.localScale.y, newEdge.transform.localScale.z);
        newEdge.transform.localScale = newScale;
        
    }

    private Graph createGraph(string path)
    {
        print("| LoadGraph | Generating graph from file...");
        List<string> lines = File.ReadAllLines(path).ToList<string>();
        Graph graph = new Graph();
        graph.edges = new List<Edge>();

        createNodes(graph, lines[0]);
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
                //print("e" + edgeToAdd.from.pos + "->" + edgeToAdd.to.pos);
                from.neighs.Add(edgeToAdd);
                graph.edges.Add(edgeToAdd);
            }
        }
        print("| LoadGraph | Graph fully generated.");
        return graph;
    }



    private void createNodes(Graph graph, string line)
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
