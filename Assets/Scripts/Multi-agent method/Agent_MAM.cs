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
    [NonSerialized] public List<Node> priorityQueue;
    private List<Node> pathToNode;
    public bool isGenerated;
    private Graph graph;
    private Node oldOnTheWay = null;
    private bool isAStarPath = false;
    private bool isRandomDestination = false;

    public IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        agentMarket = FindObjectOfType<AgentMarket_MAM>();
        agentManager = FindObjectOfType<AgentManager_MAM>();

        destination = null;
        priorityQueue = new List<Node>();
        pathToNode = new List<Node>();

        yield return new WaitUntil(() => graphGenerator.isGenerated);
        yield return new WaitUntil(() => agentMarket.isGenerated);
        yield return new WaitUntil(() => agentManager.isGenerated);

        graph = graphGenerator.graph;
        node = graphGenerator.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        node.WarnAgentVisit();
        oldPos = transform.position;
        isGenerated = true;

        print("| AGENT | Agent MAM is setup.");
    }

    private void Update()
    {
        
        if (node != null && isGenerated)
        {

            //priorityQueue.Sort();
            // Cas où aucun noeud particulier n'est à visiter
            if (priorityQueue.Count == 0)
            {
                isRandomDestination = true;
                //print("| Agent | Aucune destination prévue. Je prend un de mes voisins.");
                FindDestination();
            }
            // Cas où il reste un noeud particulier à visiter
            else
            {
                // Cas où le chemin vers ce noeud n'est pas encore attribué
                if (pathToNode.Count == 0)
                {
                    
                    print("| Agent | Je cherche un nouveau chemin pour aller visiter au plus vide le node de priorité.");
                    Node actualNode = graph.nodes[((int)transform.position.x, (int)transform.position.z)];
                    pathToNode = PathFinding2(actualNode, priorityQueue[0]);
                    destination = pathToNode[0];
                }
                // Cas où il y a un chemin en train d'être suivi
                else
                {
                    
                    print("| Agent | Je suis le chemin.");
                    destination = pathToNode[0];
                    agentManager.SetNodeToTrue(destination);
                }

            }
            if (destination != null)
            {
                GoToDestination();
            }
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
            choice = (int)random.Next(2);
            switch (choice)
            {
                case 0:
                    temp = node.neighs[random.Next(node.neighs.Count)].to;
                    destination = temp;
                    agentManager.SetNodeToTrue(temp);
                    break;
                case 1:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Sum(y => y.to.timeSinceLastVisit))).First().to;
                    destination = temp;
                    agentManager.SetNodeToTrue(temp);
                    break;
                case 2:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Max(y => y.to.timeSinceLastVisit))).First().to;
                    destination = temp;
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
                agentMarket.CheckNeighbour(destination);
                if (isRandomDestination)
                {
                    print("1");
                    node.agentPresence = false;
                    node = destination;
                    node.WarnAgentVisit();
                    agentManager.SetNodeToFalse(node);
                    destination = null;
                    isRandomDestination = false;
                }
                else
                {
                    print("2");
                    node.agentPresence = false;
                    node = destination;
                    node.WarnAgentVisit();
                    agentManager.SetNodeToFalse(node);
                    UpdatePriorityQueue(node);
                    destination = null;
                    pathToNode.RemoveAt(0);
                    if (pathToNode.Count == 0)
                    {
                        priorityQueue.Remove(node);
                        print("Removed node because of visited.");
                    }
                }
                    
            }
        }
    }

    void UpdatePriorityQueue(Node nodeVisited)
    {
        foreach(Edge edge in nodeVisited.neighs)
        {
            if (priorityQueue.Contains(edge.to))
            {
                priorityQueue.Remove(edge.to);
                print("Removed from priorityQueue : JUST VISITED ON PATH.");
            }
        }
    }

    List<Node> PathFinding2(Node startNode, Node endNode)
    {
        List<Node> queue = new List<Node>();
        Dictionary<Node, bool> explored = new Dictionary<Node, bool>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

        foreach (Node node in graph.nodes.Values)
        {
            explored.Add(node, false);
        }

        explored[startNode] = true;
        queue.Add(startNode);
        while(queue.Count != 0)
        {
            Node u = queue[0];
            queue.RemoveAt(0);

            foreach(Edge edge in u.neighs)
            {
                if (explored[edge.to] == false)
                {
                    explored[edge.to] = true;
                    previous[edge.to] = u;
                    queue.Add(edge.to);
                }
            }
            if (queue.Contains(endNode))
            {
                break;
            }
        }
        return BuildPath(startNode, endNode, previous);

    }

    List<Node> BuildPath(Node startNode, Node endNode, Dictionary<Node, Node> previous)
    {
        //print("Start node : " + startNode.pos.Item1 + "," + startNode.pos.Item2);
        //print("End node : " + endNode.pos.Item1 + "," + endNode.pos.Item2);
        List<Node> path = new List<Node>();
        path.Add(endNode);
        while (!path.Contains(startNode))
        {
            //print("adding node (" + previous[endNode].pos.Item1 + "," + previous[endNode].pos.Item2 + ") to path.");
            path.Insert(0, previous[endNode]);
            endNode = previous[endNode];
        }
        return path;
    }
}