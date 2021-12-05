using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public Dictionary<(int, int), Node> nodes;
    public List<Edge> edges;

    public Graph()
    {
        nodes = new Dictionary<(int, int), Node>();
        edges = new List<Edge>();
    }
    public Graph(Graph g)
    {
        nodes = new Dictionary<(int, int), Node>();
        edges = new List<Edge>();
        foreach (var n in g.nodes.Values)
        {
            nodes.Add(n.pos, new Node(n));
        }
        foreach(var e in g.edges)
        {
            edges.Add(new Edge(nodes[e.from.pos], nodes[e.to.pos], e.cost));
        }
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
    public string SaveAsStringWithTimes()
    {
        string save = "";
        foreach (var n in nodes.Values)
        {
            save += (n.pos,Mathf.FloorToInt(n.timeSinceLastVisit)) + "\n";
        }
        return save;
    }

    internal void LoadFromString(string v)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class Node
{
    public (int, int) pos;
    public List<Edge> neighs;
    public Vector3 realPos;
    public Vector3 realPosFromagentHeights;
    [SerializeField] public float timeSinceLastVisit;
    [SerializeField] public float StraightLineDistanceToEnd;
    [SerializeField] public bool Visited;
    [SerializeField] public Node NearestToStart;
    [SerializeField] public float? MinCostToStart;
    public bool agentPresence;

    public Node((int, int) pos)
    {
        this.pos = pos;
        timeSinceLastVisit = 0f;
        neighs = new List<Edge>();
        realPos = new Vector3(pos.Item1, 0, pos.Item2);
        realPosFromagentHeights = realPos + Vector3.up;
    }
    public Node(Node n) : this(n.pos)
    {
        //COPY CONSTRUCTOR
        agentPresence = n.agentPresence;
    }

    internal void WarnAgentVisit()
    {
        agentPresence = true;
        timeSinceLastVisit = 0;
        foreach(var e in neighs)
        {
            e.to.timeSinceLastVisit = 0;
        }
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