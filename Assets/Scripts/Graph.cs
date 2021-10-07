using System;
using System.Collections;
using System.Collections.Generic;
public class Graph : Dictionary<(int, int), Node>
{
    public Dictionary<(int, int), Node> nodes;
    public List<Edge> edges;

    public Graph()
    {
        nodes = new Dictionary<(int, int), Node>();
        edges = new List<Edge>();
    }

    public string SaveAsString()
    {
        string save = "";
        foreach (var n in nodes.Values)
        {
            save += n.pos + "\n";
        }
        return save;
    }

    internal void LoadFromString(string v)
    {
        throw new NotImplementedException();
    }
}

public class Node
{
    public (int, int) pos;
    public List<Edge> neighs;

    public Node((int, int) pos)
    {
        this.pos = pos;
        neighs = new List<Edge>();
    }
}
public class Edge
{
    public Node from;
    public Node to;
    public float cost;

    public Edge(Node from, Node to, float cost)
    {
        this.from = from;
        this.to = to;
        this.cost = cost;
    }
}