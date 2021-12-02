using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections;

public class AgentGestionnaire : MonoBehaviour
{
    // Correspond respectivement au d�lai de lancement (en seconde) et au taux de r�p�tition (en seconde) de la m�thode de check du graphe.
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
        // Liaison entre les �venements et leurs m�thodes
        EventManager.current.onHasToBeVisited += OnHasToBeVisited;
        EventManager.current.onSendShortestPathData += OnSendShortestPathData;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnRemoveNodeFromNodeAssignation;
        EventManager.current.onUpdateNodePriority += OnUpdateNodePriority;

        agentManageur = FindObjectOfType<AgentManageur>();
        yield return new WaitUntil(() => agentManageur.areAgentGenerated);

        // R�cup�ration des instance d'autres scripts
        agentList = FindObjectsOfType(typeof(AgentPatrouilleur)) as AgentPatrouilleur[];
        graphGenerator = FindObjectOfType<GraphGenerator>();
        loadGraph = FindObjectOfType<LoadGraph>();

        shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        // Attente de l'initialisation compl�te du graphGenerator afin de r�cup�rer le graph.
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

        // Attente de l'initialisation compl�te de l'agent manageur.
        yield return new WaitUntil(() => agentManageur.isGenerated);

        // On envoie les informations aux agents.
        EventManager.current.UpdateNodeAssignation(nodeAssignation, nodePriority);

        // G�n�ration = OK
        isGenerated = true;

        // Lancement � interval r�gulier de la m�thode de warn .
        InvokeRepeating("Warn", delay, repeatRate);
    }

    // M�thode permettant de v�rifier tous les nodes envoy�s par l'agent manageur. S'ils ne sont pas encore assign�, on lance la m�thode d'assignation.
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

    // M�thode permettant d'update les assignations de node en fonction des positions en temps r�el des diff�rents agents pr�sent sur le graphe.
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

    // M�thode assignant le node en param�tre au meilleur agent en fonction de leur distance en terme de path dans le graphe.
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

    // M�thode renvoyant le meilleur agent par rapport au node pass� en param�tre. V�rifie les distances de chaque agent par rapport au node et renvoie le plus proche.
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

    // M�thode li� � l'�venement de passage d'un agent sur un node. Lorsqu'un node est visit�, on le remove de nodeAssignation. L'agent manageur s'occupe de remettre sa priorit� � 1.
    void OnRemoveNodeFromNodeAssignation(Node node)
    {
        nodeAssignation.Remove(node);
        EventManager.current.SettingNodeToFalse(node);
    }

    // M�thode li� � l'�venement indiquant qu'un node doit �tre visit�, et donc assign� � un agent.
    void OnHasToBeVisited(Node node)
    {
        hasToBeVisited.Add(node);
    }

    // M�thode se lan�ant une fois au d�but, r�cup�rant la liste de tous les paths du graphe.
    void OnSendShortestPathData(Dictionary<(Node, Node), List<Node>> data)
    {
        shortestPathData = new Dictionary<(Node, Node), List<Node>>();
        shortestPathData = data;
        shortestPathUpdated = true;
    }

    // M�thode permettant de r�cup�rer le dictionnaire sur les priorit�s de passage pour chaque node.
    void OnUpdateNodePriority(Dictionary<Node, int> _nodePriority)
    {
        nodePriority = _nodePriority;
    }
}
