using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class Agent_MAM : MonoBehaviour
{
    public float speed = 1;
    protected Vector3 oldPos;
    GraphGenerator graphGenerator;
    AgentMarket_MAM agentMarket;
    AgentManager_MAM agentManager;
    [NonSerialized] protected Node node;
    [NonSerialized] protected Node destination;
    [NonSerialized] public List<Node> destinationList;
    public bool isGenerated;
    public bool isTaken;
    private Graph graph;
    private Node oldOnTheWay = null;

    public IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        agentMarket = FindObjectOfType<AgentMarket_MAM>();
        agentManager = FindObjectOfType<AgentManager_MAM>();

        destination = null;
        destinationList = new List<Node>();

        yield return new WaitUntil(() => graphGenerator.isGenerated);
        yield return new WaitUntil(() => agentMarket.isGenerated);
        yield return new WaitUntil(() => agentManager.isGenerated);

        graph = graphGenerator.graph;
        node = graphGenerator.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        node.WarnAgentVisit();
        oldPos = transform.position;
        isGenerated = true;

        print("AGENT / Agent MAM is setup.");
    }
    private void Update()
    {
        if (node != null && isGenerated)
        {
            if (destinationList.Count() == 0)
            {
                isTaken = false;
                if (agentMarket.hasToBeVisited.Count == 0)
                {
                    print("AGENT / No destination. Giving new one");
                    FindDestination();
                }
                else
                {
                    Node temp = agentMarket.hasToBeVisited[0];
                    isTaken = true;
                    destinationList.Insert(0, temp);
                    agentMarket.hasToBeVisited.Remove(temp);
                    agentManager.SetNodeToTrue(temp);
                    print("MARKET / Warning agent about (" + node.pos.Item1 + "," + node.pos.Item2 + ").");
                }
                
            }
            destination = destinationList[0];
            GoToDestination();
        }
    }
    protected void FindDestination()
    {
        Node temp = null;
        //TEST
        if (destination == null)
        {
            var choice = 2;
            System.Random random = new System.Random();
            switch (choice)
            {
                case 0:
                    temp = node.neighs[random.Next(node.neighs.Count)].to;
                    destinationList.Add(temp);
                    agentManager.SetNodeToTrue(temp);
                    break;
                case 1:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Sum(y => y.to.timeSinceLastVisit))).First().to;
                    destinationList.Add(temp);
                    agentManager.SetNodeToTrue(temp);
                    break;
                case 2:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Max(y => y.to.timeSinceLastVisit))).First().to;
                    destinationList.Add(temp);
                    agentManager.SetNodeToTrue(temp);
                    break;
            }
        }
    }

    private void GoToDestination()
    {
        speed = Mathf.Abs(speed);
        if (transform.position != oldPos)
        {
            throw new Exception("Error : the transform position was modified by another script");
        }
        float movement = speed * Time.deltaTime;
        GoToDestination(movement);
        oldPos = transform.position;
        Node newOnTheWay = graph.nodes[((int)Math.Truncate(oldPos.x), (int)Math.Truncate(oldPos.z))];
        if (!newOnTheWay.Equals(oldOnTheWay))
        {
            newOnTheWay.WarnAgentVisit();
            agentMarket.CheckNeighbour(newOnTheWay);
        }
        oldOnTheWay = graph.nodes[((int)Math.Truncate(oldPos.x), (int)Math.Truncate(oldPos.z))];
    }

    private void GoToDestination(float movementLeft)
    {
        if (destination != null)
        {
            Vector3 moveToward = Vector3.MoveTowards(transform.position, destination.realPosFromagentHeights, movementLeft);
            movementLeft -= Vector3.Distance(transform.position, moveToward);
            transform.position = moveToward;
            if (Vector3.Distance(moveToward, destination.realPosFromagentHeights) < 0.01)
            {
                node.agentPresence = false;
                node = destination;
                node.WarnAgentVisit();
                agentManager.SetNodeToFalse(node);
                destination = null;
                destinationList.RemoveAt(0);
            }
        }
    }

}
