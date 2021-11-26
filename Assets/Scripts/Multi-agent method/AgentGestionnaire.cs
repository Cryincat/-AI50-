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
    public float delay = 0;
    public float repeatRate = 3;
    public bool isGenerated = false;

    private List<Node> hasToBeVisited;
    private static AgentPatrouilleur[] agentList;

    private Dictionary<Node, AgentPatrouilleur> nodeAssignation;
    private Dictionary<Node, int> nodePriority;
    private Dictionary<(Node, Node), List<Node>> shortestPathData;

    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;

    //private bool hasFinish = false;

    IEnumerator Start()
    {
        // Liaison entre les �venements et leurs m�thodes
        EventManager.current.onHasToBeVisited += OnHasToBeVisited;
        EventManager.current.onSendShortestPathData += OnSendShortestPathData;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnRemoveNodeFromNodeAssignation;
        EventManager.current.onUpdateNodePriority += OnUpdateNodePriority;

        // R�cup�ration des instance d'autres scripts
        agentList = FindObjectsOfType(typeof(AgentPatrouilleur)) as AgentPatrouilleur[];
        graphGenerator = FindObjectOfType<GraphGenerator>();
        loadGraph = FindObjectOfType<LoadGraph>();

        // Attente de l'initialisation compl�te du graphGenerator afin de r�cup�rer le graph.
        yield return new WaitUntil(() => graphGenerator.isGenerated);
        //yield return new WaitUntil(() => loadGraph.isGenerated);
        graph = graphGenerator.graph;
        //graph = loadGraph.graph;

        // Initialisation des variables
        hasToBeVisited = new List<Node>();
        nodeAssignation = new Dictionary<Node, AgentPatrouilleur>();
        nodePriority = new Dictionary<Node, int>();

        // Attente de l'initialisation compl�te de l'agent manageur.
        yield return new WaitUntil(() => FindObjectOfType<AgentManageur>().isGenerated);

        // On envoie les informations aux agents.
        EventManager.current.UpdateNodeAssignation(nodeAssignation, nodePriority);

        // G�n�ration = OK
        print("| Gestionnaire | G�n�ration des variables termin�e...");
        isGenerated = true;

        // Lancement � interval r�gulier de la m�thode de warn .
        InvokeRepeating("Warn", delay, repeatRate);
    }

    // M�thode permettant de v�rifier tous les nodes envoy�s par l'agent manageur. S'ils ne sont pas encore assign�, on lance la m�thode d'assignation.
    void Warn()
    {
        foreach (Node node in hasToBeVisited)
        {
            if (!nodeAssignation.Keys.Contains(node)) WarnAboutNode(node);
        }
        hasToBeVisited.Clear();
        updateNodeAssignation();
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
        EventManager.current.UpdateNodeAssignation(nodeAssignation,nodePriority);
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
            float nbNodeInPath = Mathf.Infinity ;

            if (shortestPathData.ContainsKey((agentNode, node)))
            {
                nbNodeInPath = shortestPathData[(agentNode, node)].Count;
            }
            else
            {
                throw new Exception("The path from node (" + agentNode.pos.Item1 + "," + agentNode.pos.Item2 + ") to node (" + node.pos.Item1 + "," + node.pos.Item2 + ") doesn't exist.");
            }
            if (nbNodeInPath == -1)
            {
                throw new Exception("Une erreur est survenue dans la r�cup�ration du nombre de node dans le path pour le choix du meilleure agent.");
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

    // M�thode li� � l'�venement de passage d'un agent sur un node. Lorsqu'un node est visit�, on le remove de nodeAssignation. L'agent manageur s'occupe de remettre sa priorit� � 1.
    void OnRemoveNodeFromNodeAssignation(Node node)
    {
        nodeAssignation.Remove(node);
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
        foreach ((Node, Node) couple in data.Keys)
        {
            shortestPathData.Add((couple.Item1,couple.Item2), data[couple]);
        }
    }

    // M�thode permettant de r�cup�rer le dictionnaire sur les priorit�s de passage pour chaque node.
    void OnUpdateNodePriority(Dictionary<Node,int> _nodePriority)
    {
        nodePriority = _nodePriority;
    }
}
