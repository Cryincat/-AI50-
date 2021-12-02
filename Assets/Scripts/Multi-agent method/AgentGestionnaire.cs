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
    public float repeatRate = 1;
    public float delay = 3;
    public bool isGenerated = false;
    public bool isGraphLoading = false;

    private List<Node> hasToBeVisited;
    private static AgentPatrouilleur[] agentList;
    private AgentManageur agentManageur;

    private Dictionary<Node, AgentPatrouilleur> nodeAssignation;
    private Dictionary<Node, int> nodePriority;
    private Dictionary<(Node, Node), List<Node>> shortestPathData;

    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;

    private bool shortestPathUpdated = false;

    //private bool hasFinish = false;

    IEnumerator Start()
    {
        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onHasToBeVisited += OnHasToBeVisited;
        EventManager.current.onSendShortestPathData += OnSendShortestPathData;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnRemoveNodeFromNodeAssignation;
        EventManager.current.onUpdateNodePriority += OnUpdateNodePriority;

        agentManageur = FindObjectOfType<AgentManageur>();
        yield return new WaitUntil(() => agentManageur.areAgentGenerated);

        // Récupération des instance d'autres scripts
        agentList = FindObjectsOfType(typeof(AgentPatrouilleur)) as AgentPatrouilleur[];
        graphGenerator = FindObjectOfType<GraphGenerator>();
        loadGraph = FindObjectOfType<LoadGraph>();

        shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        // Attente de l'initialisation complète du graphGenerator afin de récupérer le graph.
        if (isGraphLoading)
        {
            yield return new WaitUntil(() => loadGraph.isGenerated);
            graph = loadGraph.graph;
        }
        else
        {
            yield return new WaitUntil(() => graphGenerator.isGenerated);
            graph = graphGenerator.graph;
        }



        // Initialisation des variables
        hasToBeVisited = new List<Node>();
        nodeAssignation = new Dictionary<Node, AgentPatrouilleur>();
        nodePriority = new Dictionary<Node, int>();

        // Attente de l'initialisation complète de l'agent manageur.
        yield return new WaitUntil(() => agentManageur.isGenerated);

        // On envoie les informations aux agents.
        EventManager.current.UpdateNodeAssignation(nodeAssignation, nodePriority);

        // Génération = OK
        isGenerated = true;

        // Lancement à interval régulier de la méthode de warn .
        InvokeRepeating("Warn", delay, repeatRate);
    }

    // Méthode permettant de vérifier tous les nodes envoyés par l'agent manageur. S'ils ne sont pas encore assigné, on lance la méthode d'assignation.
    void Warn()
    {
        if (shortestPathUpdated)
        {
            foreach (Node node in hasToBeVisited)
            {
                if (!nodeAssignation.Keys.Contains(node)) WarnAboutNode(node);
            }
            hasToBeVisited.Clear();
            updateNodeAssignation();
        }
    }

    // Méthode permettant d'update les assignations de node en fonction des positions en temps réel des différents agents présent sur le graphe.
    void updateNodeAssignation()
    {
        if (nodeAssignation.Count == 0)
        {
            return;
        }
        List<Node> tempKeys = nodeAssignation.Keys.ToList<Node>();
        foreach (Node node in tempKeys)
        {
            WarnAboutNode(node);
        }
        EventManager.current.UpdateNodeAssignation(nodeAssignation, nodePriority);
    }

    // Méthode assignant le node en paramètre au meilleur agent en fonction de leur distance en terme de path dans le graphe.
    // Si le meilleur agent existe, l'assigne au node dans nodeAssignation.
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

    // Méthode renvoyant le meilleur agent par rapport au node passé en paramètre. Vérifie les distances de chaque agent par rapport au node et renvoie le plus proche.
    AgentPatrouilleur FindBestAgentByPath(Node node)
    {
        float dist = Mathf.Infinity;
        int iteration = 0;
        int bestAgentIteration = -1;

        foreach (AgentPatrouilleur agent in agentList)
        {
            Node agentNode = agent.GetNode();
            float totalDistanceOfPath = Mathf.Infinity;
            if (shortestPathData.ContainsKey((agentNode, node)))
            {
                totalDistanceOfPath = getSumDistanceOfPath(shortestPathData[(agentNode, node)]);
            }
            else
            {
                throw new Exception("The path from node (" + agentNode.pos.Item1 + "," + agentNode.pos.Item2 + ") to node (" + node.pos.Item1 + "," + node.pos.Item2 + ") doesn't exist.");
            }
            if (dist > totalDistanceOfPath)
            {
                dist = totalDistanceOfPath;
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

    float getSumDistanceOfPath(List<Node> nodes)
    {
        float sum = 0;

        if (nodes.Count == 0 || nodes.Count == 1)
        {
            return 0;
        }
        if (nodes.Count == 2)
        {
            return Vector3.Distance(nodes[0].realPos, nodes[1].realPos);
        }

        for (int i = 0; i <= nodes.Count - 1; i++)
        {
            if (nodes.Count - 1 > i + 1)
                sum += Vector3.Distance(nodes[i].realPos, nodes[i + 1].realPos);
        }

        return sum;
    }

    // Méthode lié à l'évenement de passage d'un agent sur un node. Lorsqu'un node est visité, on le remove de nodeAssignation. L'agent manageur s'occupe de remettre sa priorité à 1.
    void OnRemoveNodeFromNodeAssignation(Node node)
    {
        nodeAssignation.Remove(node);
        EventManager.current.SettingNodeToFalse(node);
    }

    // Méthode lié à l'évenement indiquant qu'un node doit être visité, et donc assigné à un agent.
    void OnHasToBeVisited(Node node)
    {
        hasToBeVisited.Add(node);
    }

    // Méthode se lançant une fois au début, récupérant la liste de tous les paths du graphe.
    void OnSendShortestPathData(Dictionary<(Node, Node), List<Node>> data)
    {
        shortestPathData = new Dictionary<(Node, Node), List<Node>>();
        shortestPathData = data;
        shortestPathUpdated = true;
    }

    // Méthode permettant de récupérer le dictionnaire sur les priorités de passage pour chaque node.
    void OnUpdateNodePriority(Dictionary<Node, int> _nodePriority)
    {
        nodePriority = _nodePriority;
    }
}
