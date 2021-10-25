using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMarket_MAM : MonoBehaviour
{
    public List<Node> hasToBeVisited;
    public bool isGenerated = false;

    private AgentManager_MAM agentManager;

    // Start is called before the first frame update
    void Start()
    {
        hasToBeVisited = new List<Node>();
        agentManager = FindObjectOfType<AgentManager_MAM>();
        isGenerated = true;
        print("MARKET / Agent market is setup.");
    }

    // Update is called once per frame
    void Update()
    {
        if (hasToBeVisited.Count != 0)
        {
            WarnAgent(hasToBeVisited[0]);
        }
    }

    void WarnAgent(Node node)
    {
        Agent_MAM bestAgent = findBestAgent(node);
        if (bestAgent.isTaken == false)
        {
            bestAgent.isTaken = true;
            bestAgent.destinationList.Insert(0, node);
            hasToBeVisited.Remove(node);
            agentManager.SetNodeToTrue(node);
            print("MARKET / Warning agent about (" + node.pos.Item1 + "," + node.pos.Item2 + ").");
        }
    }

    // This method have to return the best agent for the given node (Test the distance from agent to node, and the best one is returned)
    Agent_MAM findBestAgent(Node node)
    {
        return FindObjectOfType<Agent_MAM>(); ;
    }

    public void CheckNeighbour(Node node)
    {
        // Inutile car testé au tour d'avant
        /*
        // Check if node is in list
        if (hasToBeVisited.Contains(node))
        {
            hasToBeVisited.Remove(node);
            agentManager.managerTool[node] = false;
        }*/
        foreach(Edge edge in node.neighs)
        {
            if (hasToBeVisited.Contains(edge.to))
            {
                hasToBeVisited.Remove(edge.to);
                agentManager.managerTool[edge.to] = false;
            }
        }
    }
}
