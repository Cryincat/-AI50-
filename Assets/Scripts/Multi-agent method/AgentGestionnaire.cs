using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections;

public class AgentGestionnaire : MonoBehaviour
{
    // Correspond respectivement au délai de lancement (en seconde) et au taux de répétition (en seconde) de la méthode de check du graphe.
    public float delay = 0;
    public float repeatRate = 3;
    public bool isGenerated = false;

    private List<Node> hasToBeVisited;
    private static AgentPatrouilleur[] agentList;

    private Dictionary<Node, AgentPatrouilleur> nodeAssignation;
    private Dictionary<Node, int> nodePriority;
    private Dictionary<(Node, Node), List<Node>> managerData;

    private Graph graph;
    private GraphGenerator graphGenerator;

    //private bool hasFinish = false;

    IEnumerator Start()
    {
        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onHasToBeVisited += OnHasToBeVisited;
        EventManager.current.onUpdatingShortestPathData += OnUpdatingShortestPathData;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnRemoveNodeFromNodeAssignation;

        // Récupération des instance d'autres scripts
        agentList = FindObjectsOfType(typeof(AgentPatrouilleur)) as AgentPatrouilleur[];
        graphGenerator = FindObjectOfType<GraphGenerator>();
        yield return new WaitUntil(() => graphGenerator.isGenerated);
        graph = graphGenerator.graph;

        // Initialisation des variables
        hasToBeVisited = new List<Node>();
        nodeAssignation = new Dictionary<Node, AgentPatrouilleur>();
        managerData = new Dictionary<(Node, Node), List<Node>>();
        nodePriority = new Dictionary<Node, int>();

        // On envoie la première itération des variables du script à travers certains évenements
        EventManager.current.UpdateNodeAssignation(nodeAssignation,nodePriority);

        // Génération = OK
        print("| Gestionnaire | Génération des variables terminée...");
        isGenerated = true;

        // Lancement à interval régulier de la méthode de warn 
        InvokeRepeating("Warn", delay, repeatRate);
    }



    void Warn()
    {
        foreach(Node node in hasToBeVisited)
        {
            WarnAboutNode(node);
        }
        hasToBeVisited.Clear();
        updateNodeAssignation();
    }

    void updateNodeAssignation()
    {
        if (nodeAssignation.Count == 0)
        {
            return;   
        }
        List<Node> tempKeys = getKeys(nodeAssignation);
        foreach (Node node in tempKeys)
        {
            WarnAboutNode(node);
        }
        EventManager.current.UpdateNodeAssignation(nodeAssignation,nodePriority);
        EventManager.current.UpdatingKeyList(nodePriority);
    }

    List<Node> getKeys(Dictionary<Node,AgentPatrouilleur> temp)
    {
        List<Node> keys = new List<Node>();

        foreach(Node node in temp.Keys)
        {
            keys.Add(node);
        }
        return keys;
    }

    void WarnAboutNode(Node node)
    {
        AgentPatrouilleur bestAgent = FindBestAgentByPath(node);
        if (bestAgent != null)
        {
            if (nodeAssignation.ContainsKey(node))
            {
                nodeAssignation[node] = bestAgent;
            }
            else
            {
                nodeAssignation.Add(node, bestAgent);
            }
        }
    }

    AgentPatrouilleur FindBestAgentByPath(Node node)
    {
        
        float dist = Mathf.Infinity;
        int iteration = 0;
        int bestAgentIteration = -1;

        foreach (AgentPatrouilleur agent in agentList)
        {
            Node agentNode = agent.GetNode();
            float nbNodeInPath = Mathf.Infinity ;

            if (managerData.ContainsKey((agentNode, node)))
            {
                nbNodeInPath = managerData[(agentNode, node)].Count;
            }
            else
            {
                List<Node> path = PathFinding(agentNode, node);
                EventManager.current.AddingShortestPath(path, agentNode, node);
                /*while (hasFinish == false)
                {
                    //print("| Gestionnaire | En attente d'une mis à jour du managerData...");
                }
                hasFinish = false;*/
                nbNodeInPath = managerData[(agentNode, node)].Count;
            }
            if (nbNodeInPath == -1)
            {
                throw new Exception("Une erreur est survenue dans la récupération du nombre de node dans le path pour le choix du meilleure agent.");
            }
            
            if (dist > nbNodeInPath)
            {
                dist = nbNodeInPath;
                bestAgentIteration = iteration;
               
            }
            iteration++;
        }
        if (bestAgentIteration == -1)
        {
            return null;
        }
       
        return agentList[bestAgentIteration];
    }

    void OnRemoveNodeFromNodeAssignation(Node node)
    {
        nodeAssignation.Remove(node);
        nodePriority.Remove(node);
    }

    void OnHasToBeVisited(Node node, int priority)
    {
        hasToBeVisited.Add(node);
        if (!nodePriority.ContainsKey(node))
        {
            nodePriority.Add(node, priority);
        }
        else
        {
            nodePriority[node] = priority;
        }
        EventManager.current.UpdateNodeAssignation(nodeAssignation, nodePriority);
    }

    void OnUpdatingShortestPathData(Dictionary<(Node, Node), List<Node>> shortestPathData)
    {
        managerData = shortestPathData;
        //hasFinish = true;
    }

    // Méthode permettant de récupérer le plus court chemin entre deux nodes (BFS like)
    List<Node> PathFinding(Node startNode, Node endNode)
    {
        List<Node> queue = new List<Node>();
        Dictionary<Node, bool> explored = new Dictionary<Node, bool>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

        if (startNode == endNode)
        {
            return BuildPath(startNode, endNode, previous);
        }
        foreach (Node node in graph.nodes.Values)
        {
            explored.Add(node, false);
        }

        explored[startNode] = true;
        queue.Add(startNode);
        while (queue.Count != 0)
        {
            Node u = queue[0];
            queue.RemoveAt(0);

            foreach (Edge edge in u.neighs)
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

    // Méthode reconstrusant le plus cours chemin déterminé par 'PathFinding'.
    List<Node> BuildPath(Node startNode, Node endNode, Dictionary<Node, Node> previous)
    {
        List<Node> path = new List<Node>();
        if (startNode == endNode)
        {
            path.Add(startNode);
            return path;
        }
        path.Add(endNode);
        while (!path.Contains(startNode))
        {
            path.Insert(0, previous[endNode]);
            endNode = previous[endNode];
        }
        return path;
    }
}
