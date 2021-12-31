using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStar_ACO_complete : MonoBehaviour
{

    private Graph graph;

    public List<fourmis> list_agent;
    public ColonyMulti colonyM;
    public LoadGraph co_graph;
    public List<Node> chemin_reel;

    public bool isGenerated = false;
    public Node began;
    public Node Endeuh;
    public float costeuh;

   

    // Start is called before the first frame update
    public IEnumerator Start()
    {

        yield return new WaitUntil(() => FindObjectOfType<LoadGraph>().isGenerated);
        co_graph = FindObjectOfType<LoadGraph>();
        graph = co_graph.graph;
        costeuh = (float)GetCostShortestPath(began, Endeuh);  
        isGenerated = true;
        yield return null;
    }

    public double GetCostShortestPath (Node begin, Node end)
    {
        List<Node> a = new List<Node>();

        double costrealPath = 0;

        a = this.GetShortestPathAstar(begin, end, graph);

        for (int i = 0; i < (a.Count() - 1); i++)
        {
            costrealPath += new Vector2(a[i].pos.Item1 - a[i + 1].pos.Item1, a[i].pos.Item2 - a[i + 1].pos.Item2).magnitude;
        }
        return costrealPath;
    }

    public List<Node> GetShortestPathAstar(Node begin, Node end, Graph graph)
    {

        Node beginReal = graph.nodes[begin.pos];
        //Node beginReal = begin;
        Node endReal = graph.nodes[end.pos];
        //Node endReal = end;
        foreach (Node node in graph.nodes.Values)
        {

            node.StraightLineDistanceToEnd = Vector3.Distance(node.realPos, end.realPos);

            node.Visited = false;
            node.MinCostToStart = null;
            node.NearestToStart = null;
        }

        AstarSearch(beginReal, endReal);
        List<Node> shortestPath = new List<Node>();
        //shortestPath.Clear();
        shortestPath.Add(endReal);
        BuildShortestPath(shortestPath, endReal);
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

