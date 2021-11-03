using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class AgentManager_MAM : MonoBehaviour
{
    private GraphGenerator graphGenerator;
    private AgentMarket_MAM agentMarket;
    private Graph graph;

    public Dictionary<Node, bool> managerTool;
    public float threshold = 15;
    public bool isGenerated = false;
    public float delay = 10f;
    public float repeatRate = 10f;

    // Start is called before the first frame update
    IEnumerator Start()
    {

        managerTool = new Dictionary<Node, bool>();
        graphGenerator = FindObjectOfType<GraphGenerator>();
        agentMarket = FindObjectOfType<AgentMarket_MAM>();
        

        yield return new WaitUntil(() => graphGenerator.isGenerated);
        graph = graphGenerator.graph;

        yield return new WaitUntil(() => agentMarket.isGenerated);
        LoadManagerTool();
        isGenerated = true;
        print("MANAGER / AgentManager is setup.");

        InvokeRepeating("CheckGraph", delay, repeatRate);
    }

    // Set every node to false (not going to be visited)
    void LoadManagerTool()
    {
        foreach(Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
    }

    void CheckGraph()
    {
        print("| MANAGER | Checking for nodes...");
        foreach (Node node in graph.nodes.Values)
        {
            if (node.timeSinceLastVisit > threshold && !agentMarket.nodeAssignation.Keys.Contains(node) && managerTool[node] == false)
            {
                SetNodeToTrue(node);
                if (!hasNeighbourInList(node))
                {
                    print("| MANAGER | Adding node to market list.");
                    agentMarket.hasToBeVisited.Add(node);
                }
            }
        }
    }

    bool hasNeighbourInList(Node node)
    {
        foreach(Edge edge in node.neighs)
        {
            if (agentMarket.nodeAssignation.Keys.Contains(edge.to))
            {
                return true;
            }
        }
        return false;
    }

    public void SetNodeToTrue(Node node)
    {
        managerTool[node] = true;
        foreach(Edge edge in node.neighs)
        {
            managerTool[edge.to] = true;
        }
    }

    public void SetNodeToFalse(Node node)
    {
        managerTool[node] = false;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = false;
        }
    }
}
