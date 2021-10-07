using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;

public class GraphGenerator : MonoBehaviour
{
    public GameObject sol;
    public Graph graph;
    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    private void Generate()
    {
        graph = new Graph();
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
                throw;
            }
        }
        //Case where the map is already existing
        foreach (Transform item in transform)
        {
            if (item.GetComponent<NodeComponent>())
            {
                //If the object has a NodeComponent, we generate a node in the graph at its position
                (int, int) pos = ((int)item.position.x, (int)item.position.z);
                Node node = new Node(pos);
                graph.nodes.Add(pos, node);
                item.GetComponent<NodeComponent>().node = new Node(pos);
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

            if (topNeigh == null || leftNeigh == null) topLeftNeigh = null;
            if (topNeigh == null || rightNeigh == null) topRightNeigh = null;
            if (bottomNeigh == null || leftNeigh == null) bottomLeftNeigh = null;
            if (bottomNeigh == null || rightNeigh == null) bottomRightNeigh = null;

            List<Node> neighs = new List<Node>() { leftNeigh, rightNeigh, topNeigh, bottomNeigh, topLeftNeigh, topRightNeigh, bottomLeftNeigh, bottomRightNeigh };
            neighs.RemoveAll(item => item == null);
            foreach (Node to in neighs)
            {
                Edge edge = new Edge(node, to, new Vector2(node.pos.Item1 - to.pos.Item1, node.pos.Item2 - to.pos.Item2).magnitude);
                node.neighs.Add(edge);
                graph.edges.Add(edge);
            }
        }
        //Debug.Log("Saving...");
        //File.WriteAllText("test.txt", graph.SaveAsString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}

