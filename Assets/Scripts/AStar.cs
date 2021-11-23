using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStar : MonoBehaviour
{
    public Graph graph;
    public GraphGenerator graphGenerator;
    
    // Start is called before the first frame update
    public IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        while (!graphGenerator.isGenerated)
            yield return null;
        graph = graphGenerator.graph;

        // Path test
        /* 
        Node node1 = new Node((41, 18));
        Node node2 = new Node((38, 18));
        List<Node> a = this.GetShortestPathAstar(graph.nodes[(46,21)], graph.nodes[(38, 19)]);
        Debug.Log("Taille liste : " + a.Count);
        foreach(var node in a)
        {
            Debug.Log("Node : " + node.pos);
        }
        */

    }

    public List<Node> GetShortestPathAstar(Node begin, Node end)
    {
        foreach (var node in graph.nodes.Values)
        {
            node.StraightLineDistanceToEnd = Vector3.Distance(node.realPos, end.realPos);//node.StraightLineDistanceTo(End);
            node.Visited = false;
            node.MinCostToStart = null;
            node.NearestToStart = null;
        }
        AstarSearch(begin, end);
        List<Node> shortestPath = new List<Node>();
        shortestPath.Add(end);
        BuildShortestPath(shortestPath, end);
        shortestPath.Reverse();
        return shortestPath;
    }
    private void BuildShortestPath(List<Node> list, Node node)
    {
        if (node.NearestToStart == null)
            return;
        list.Add(node.NearestToStart);
        BuildShortestPath(list, node.NearestToStart);
    }

    private void AstarSearch(Node begin, Node end)
    {
        begin.MinCostToStart = 0;
        var prioQueue = new List<Node>();
        prioQueue.Add(begin);

        do
        {
            prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToEnd).ToList();
            var node = prioQueue.First();
            prioQueue.Remove(node);

            foreach (var cnn in node.neighs.OrderBy(x => x.cost))
            {
                var childNode = cnn.to;
                if (childNode.Visited)
                    continue;
                if (childNode.MinCostToStart == null || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    childNode.NearestToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.Visited = true;
            if (node == end)
                return;
        } while (prioQueue.Any());
    }
}
