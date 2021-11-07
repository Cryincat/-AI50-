using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

public class AgentMarket_MAM : MonoBehaviour
{
    public List<Node> hasToBeVisited;
    public bool isGenerated = false;
    private Agent_MAM[] listAgent;
    private AgentManager_MAM agentManager;
    public ConcurrentDictionary<Node, Agent_MAM> nodeAssignation;

    public float repeatRate = 5f;
    public float delay = 5f;

    // Start is called before the first frame update
    void Start()
    {
        nodeAssignation = new ConcurrentDictionary<Node, Agent_MAM>();
        hasToBeVisited = new List<Node>();
        agentManager = FindObjectOfType<AgentManager_MAM>();
        listAgent = FindObjectsOfType(typeof(Agent_MAM)) as Agent_MAM[];
        isGenerated = true;
        print("| MARKET | Agent market is setup.");
        InvokeRepeating("Warn", delay, repeatRate);
    }

    void Warn()
    {
        if (hasToBeVisited.Count == 0)
        {
            print("| MARKET | There isn't any node to visit.");
            return;
        }
        else
        {
            print("| Market | Warn.");
            foreach (Node node in hasToBeVisited)
            {
                WarnAboutNode(node);
            }
            print("| MARKET | List cleared.");
            hasToBeVisited.Clear();

            List<Node> keysList = copyKeys(nodeAssignation);
            foreach (Node node in nodeAssignation.Keys)
            {
                WarnAboutNode(node);
            }
        }
        return;
        
    }

    public void UpdateNodeAssignation(Node nodeToVerify)
    {
        foreach(Node node in nodeAssignation.Keys)
        {
            if (nodeToVerify == node)
            {
                Agent_MAM agentLinkedToNode = nodeAssignation[node];
                nodeAssignation.TryRemove(node, out agentLinkedToNode);
                print("Removed node cause of visited (updatenodeassignation)");
            }
        }
    }


    List<Node> copyKeys(ConcurrentDictionary<Node, Agent_MAM> nodeAssignation)
    {
        List<Node> temp = new List<Node>();
        foreach(Node node in nodeAssignation.Keys)
        {
            temp.Add(node);
        }
        return temp;
    }

    void WarnAboutNode(Node node)
    {
        Agent_MAM bestAgent = FindBestAgentByDist(node);
        print("Best agent for (" + node.pos.Item1 + "," + node.pos.Item2 + ") is " + bestAgent.ToString());
        if (bestAgent != null)
        {
            if (nodeAssignation.ContainsKey(node))
            {
                bool temp = nodeAssignation.TryUpdate(node, bestAgent, nodeAssignation[node]);
                print(" 1 : " + temp);
            }
            else
            {
                bool temp = nodeAssignation.TryAdd(node, bestAgent);
                print(" 2 : " + temp);
            }
            print("| MARKET | Assignate (" + node.pos.Item1 + ", " + node.pos.Item2 + ") to " +  bestAgent.ToString());
            return;
        }
        print("| MARKET | There is no agent available.");
        return;
    }

    // This method have to return the best agent for the given node (Test the distance from agent to node, and the best one is returned)
    Agent_MAM FindBestAgentByDist(Node node)
    {
        Vector3 posNode = new Vector3(node.pos.Item1,0,node.pos.Item2);
        float dist = Mathf.Infinity;
        int iteration = 0;
        int bestAgentIteration = -1;

        foreach (Agent_MAM agent in listAgent)
        {
            float agentDist = Vector3.Distance(agent.transform.position, posNode);
            if (dist > agentDist)
            {
                dist = agentDist;
                bestAgentIteration = iteration;
            }


            iteration++;
        }

        if (bestAgentIteration == -1)
        {
            return null;
        }
        return listAgent[bestAgentIteration];
    }

}
