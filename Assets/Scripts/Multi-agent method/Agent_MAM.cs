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
    private bool isRandomDestination = false;

    private GameObject capsule;
    

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
            var capsuleRenderer = GetComponentInChildren<Renderer>();
            
            if (priorityQueue.Count == 0)
            {
                Node temp = CheckForNode();
                if (temp != null)
                {
                    priorityQueue.Add(temp);
                    Agent_MAM toRemove = agentMarket.nodeAssignation[temp];
                    agentMarket.nodeAssignation.TryRemove(temp, out toRemove);
                }
            }
            // Cas o� aucun noeud particulier n'est � visiter
            if (priorityQueue.Count == 0)
            {
                
                isRandomDestination = true;
                capsuleRenderer.material.SetColor("_Color", Color.green);
                //print("| Agent | Aucune destination pr�vue. Je prend un de mes voisins.");
                FindDestination();
            }
            // Cas o� il reste un noeud particulier � visiter
            else
            {
                // Cas o� le chemin vers ce noeud n'est pas encore attribu�
                if (pathToNode.Count == 0)
                {
                    capsuleRenderer.material.SetColor("_Color", Color.red);
                    print("| Agent | Je cherche un nouveau chemin pour aller visiter au plus vide le node de priorit�.");
                    pathToNode = PathFinding2(node, priorityQueue[0]);
                    destination = pathToNode[0];
                }
                // Cas o� il y a un chemin en train d'�tre suivi
                else
                {
                    //var coef = Vector3.Distance(new Vector3(priorityQueue[0].pos.Item1,0,priorityQueue[0].pos.Item2),new Vector3(transform.position.x,0,transform.position.z)) / 255;
                    capsuleRenderer.material.SetColor("_Color", Color.blue);
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

    Node CheckForNode()
    {
        List<Node> temp = new List<Node>();
        foreach(Node node in agentMarket.nodeAssignation.Keys)
        {
            print("AgentMArket : " + agentMarket.nodeAssignation[node].ToString() + ", this : " + this.ToString());
            if (agentMarket.nodeAssignation[node].ToString() == this.ToString())
            {
                temp.Add(node);
            }
        }

        if (temp.Count == 0)
        {
            return null;
        }

        Node bestNode = null;
        float dist = Mathf.Infinity;
        foreach(Node node in temp)
        {
            float distComp = Vector3.Distance(new Vector3(node.pos.Item1, 0, node.pos.Item2), transform.position);
            if (dist > distComp)
            {
                bestNode = node;
                dist = distComp;
            }
        }        
        return bestNode;
    }


    protected void FindDestination()
    {
        Node temp = null;
        //TEST
        if (destination == null)
        {
            var choice = 2;
            System.Random random = new System.Random();
            //choice = (int)random.Next(3)-1;
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
                if (isRandomDestination)
                {
                    agentMarket.UpdateNodeAssignation(node);
                    node.agentPresence = false;
                    node = destination;
                    node.WarnAgentVisit();
                    agentManager.SetNodeToFalse(node);
                    destination = null;
                    isRandomDestination = false;
                }
                else
                {
                    agentMarket.UpdateNodeAssignation(node);
                    node.agentPresence = false;
                    node = destination;
                    node.WarnAgentVisit();
                    agentManager.SetNodeToFalse(node);
                    //UpdatePriorityQueue(node);
                    destination = null;
                    pathToNode.RemoveAt(0);
                    if (pathToNode.Count == 0)
                    {
                        priorityQueue.Remove(node);
                        print("| Agent | Removed node because of visited.");
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
                print("| Agent | Removed from priorityQueue : JUST VISITED ON PATH.");
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