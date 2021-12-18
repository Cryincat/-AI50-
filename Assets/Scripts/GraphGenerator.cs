using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using System.Linq;

public class GraphGenerator : MonoBehaviour
{
    private Dictionary<Node, NodeComponent> nodeComponentDict;
    public List<Agent> agents;
    public GameObject sol;
    public Graph graph;
    public bool isGenerated;

    // Start is called before the first frame update
    void Start()
    {
        Generate();
        save();
    }

    void save()
    {
        string path = Directory.GetCurrentDirectory() + "/Assets/Data/graphNew.txt";
        List<string> data = new List<string>();
        data.Add("");
        foreach(Node node in graph.nodes.Values)
        {
            if (node != graph.nodes.Values.Last())
                data[0] += node.pos.Item1 + "," + node.pos.Item2 + ";";
            else
                data[0] += node.pos.Item1 + "," + node.pos.Item2;

        }
        foreach(Node node in graph.nodes.Values)
        {
            string dataToAdd = "";
            dataToAdd = node.pos.Item1 + "," + node.pos.Item2 + ";";
            foreach(Edge edge in node.neighs)
            {
                if (edge != node.neighs.Last())
                    dataToAdd += edge.to.pos.Item1 + "," + edge.to.pos.Item2 + ":";
                else
                    dataToAdd += edge.to.pos.Item1 + "," + edge.to.pos.Item2;

            }
            data.Add(dataToAdd);
        }
        File.WriteAllLines(path, data);
    }

    private void Generate()
    {
        graph = new Graph();
        nodeComponentDict = new Dictionary<Node, NodeComponent>();
        agents = FindObjectsOfType<Agent>().ToList();
        if (transform.childCount == 0)
        {
            try
            {
                string save = File.ReadAllText(Launcher.fileToLoad);
                Debug.Log(save);
                foreach (string line in new LineReader(() => new StringReader(save)))
                {
                    string elem = line;
                    elem = elem.Remove(0, 1);
                    elem = elem.Remove(elem.Length - 1, 1);
                    var coords = elem.Split(',');
                    var obj = Instantiate(sol, transform);
                    obj.transform.position = new Vector3(int.Parse(coords[0]), 0, int.Parse(coords[1]));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //Case where the map is already existing
        foreach (Transform item in transform)
        {
            var nc = item.GetComponent<NodeComponent>();
            if (nc)
            {
                //If the object has a NodeComponent, we generate a node in the graph at its position
                (int, int) pos = ((int)item.position.x, (int)item.position.z);
                Node node = new Node(pos);
                graph.nodes.Add(pos, node);
                nc.node = node;
                nodeComponentDict.Add(node, nc);
            }
        }

        foreach (Node node in graph.nodes.Values)
        {
            //for each node, we generate edges according to neighbors
            graph.nodes.TryGetValue((node.pos.Item1 - 1, node.pos.Item2), out Node leftNeigh);
            graph.nodes.TryGetValue((node.pos.Item1 + 1, node.pos.Item2), out Node rightNeigh);
            graph.nodes.TryGetValue((node.pos.Item1, node.pos.Item2 + 1), out Node topNeigh);
            graph.nodes.TryGetValue((node.pos.Item1, node.pos.Item2 - 1), out Node bottomNeigh);

            graph.nodes.TryGetValue((node.pos.Item1 - 1, node.pos.Item2 + 1), out Node topLeftNeigh);
            graph.nodes.TryGetValue((node.pos.Item1 + 1, node.pos.Item2 + 1), out Node topRightNeigh);
            graph.nodes.TryGetValue((node.pos.Item1 - 1, node.pos.Item2 - 1), out Node bottomLeftNeigh);
            graph.nodes.TryGetValue((node.pos.Item1 + 1, node.pos.Item2 - 1), out Node bottomRightNeigh);

            //if (topNeigh == null || leftNeigh == null) topLeftNeigh = null;
            //if (topNeigh == null || rightNeigh == null) topRightNeigh = null;
            //if (bottomNeigh == null || leftNeigh == null) bottomLeftNeigh = null;
            //if (bottomNeigh == null || rightNeigh == null) bottomRightNeigh = null;

            List<Node> neighs = new List<Node>() { leftNeigh, rightNeigh, topNeigh, bottomNeigh, topLeftNeigh, topRightNeigh, bottomLeftNeigh, bottomRightNeigh };
            neighs.RemoveAll(item => item == null);

            foreach (Node to in neighs)
            {
                Edge edge = new Edge(node, to, new Vector2(node.pos.Item1 - to.pos.Item1, node.pos.Item2 - to.pos.Item2).magnitude);
                node.neighs.Add(edge);
                graph.edges.Add(edge);
                print("n" + node.pos + "->" + to.pos);
            }
            /*foreach (var agent in agents)
            {
                agent.node = graph.nodes.Values.OrderBy(x => Vector3.Distance(agent.transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
            }*/
        }
        //Debug.Log("Saving...");
        //File.WriteAllText("test.txt", graph.SaveAsString());
        isGenerated = true;
    }

    // Update is called once per frame
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
}

