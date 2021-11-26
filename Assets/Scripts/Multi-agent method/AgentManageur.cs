using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class AgentManageur : MonoBehaviour
{
    // Correspond à la valeur seuil de l'oisiveté au delà de laquelle on remarque le noeud comme a visité d'urgence
    public float threshold = 60;

    // Correspond respectivement au délai de lancement (en seconde) et au taux de répétition (en seconde) de la méthode de check du graphe.
    public float delay = 0;
    public float repeatRate = 1;

    public int maxPriority = 8;

    public bool isGenerated = false;

    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;

    private Dictionary<Node, bool> managerTool;
    private Dictionary<Node, int> nodePriority;

    IEnumerator Start()
    {
        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onSettingNodeToTrue += OnSetNodeToTrue;
        EventManager.current.onSettingNodeToFalse += OnSetNodeToFalse;
        EventManager.current.onUpdateNewMaxIdleness += OnUpdateMaxIdleness;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnSettingNodePriorityToOne;

        // Récupération des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();
        loadGraph = FindObjectOfType<LoadGraph>();

        // Initialisation des variables

        managerTool = new Dictionary<Node, bool>();
        nodePriority = new Dictionary<Node, int>();


        // Attente de la génération du script
        yield return new WaitUntil(() => graphGenerator.isGenerated);
        //yield return new WaitUntil(() => loadGraph.isGenerated);

        // Récupération des variables issues d'autres scripts
        graph = graphGenerator.graph;
        //graph = loadGraph.graph;

        // Méthode d'initialisation
        LoadManagerTool();
        LoadNodePriority();
        LoadPaths();


        // Génération = OK
        print("| Manageur | Génération des variables terminée...");
        yield return new WaitUntil(() => isGenerated);
        // Lancement à interval régulier de la méthode de check 
        InvokeRepeating("CheckGraphLeveling", delay, repeatRate);

    }

    void LoadNodePriority()
    {
        foreach (Node node in graph.nodes.Values)
        {
            nodePriority.Add(node, 1);
        }
    }

    // Méthode permettant de set l'ensemble des nodes dans managerTool à false
    void LoadManagerTool()
    {
        //print("| Manageur | Initialisation 'managerTool'...");
        foreach (Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
        print("| Manageur | 'managerTool' initialisé.");
    }

    // Méthode permettant de charger préalablement tous les paths existants entre 2 nodes dans le graphe.
    void LoadPaths()
    {
        print("| Manageur | Itinialisation des données des shortestPaths...");
        AStar aStar = FindObjectOfType<AStar>();
        int count = 1;
        int nbNode = graph.nodes.Values.Count;
        int totalPath = nbNode * nbNode;
        Dictionary<(Node, Node), List<Node>> shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        foreach (Node start in graph.nodes.Values)
        {
            foreach (Node end in graph.nodes.Values)
            {
                //print("| Manageur | From (" + start.pos.Item1 + "," + start.pos.Item2 + ") to (" + end.pos.Item1 + "," + end.pos.Item2 + ") : path " + count + "/" + nbNode * nbNode);
                if (count % 100 == 0) print("| Manageur | Path " + count + "/" + totalPath);
                count++;
                shortestPathData.Add((start, end), aStar.GetShortestPathAstar(start, end, graph));

            }
        }
        print("| Manageur | Path " + count + "/" + totalPath);
        EventManager.current.SendShortestPathData(shortestPathData);
        print("| Manageur | ShortestPaths initialisés.");
        isGenerated = true;
    }

    void CheckGraphLeveling()
    {

        int priority = -1;
        foreach (Node node in graph.nodes.Values)
        {
            if (managerTool[node] == false || (managerTool[node] == true && nodePriority[node] < maxPriority))
            {
                float x = node.timeSinceLastVisit;
                // 0% <= x < 15%
                if (x < 0.15 * threshold)
                {
                    priority = 1;
                }
                // 15% <= x < 30%
                if (x >= 0.15 * threshold && x < 0.30 * threshold)
                {
                    priority = 2;
                }
                // 30% <= x < 50%
                if (x >= 0.30 * threshold && x < 0.50 * threshold)
                {
                    priority = 3;
                }
                // 50% <= x < 65%
                if (x >= 0.50 * threshold && x < 0.65 * threshold)
                {
                    priority = 4;
                }
                // 65% <= x < 80%
                if (x >= 0.65 * threshold && x < 0.80 * threshold)
                {
                    priority = 5;
                }
                // 80% <= x < 90%
                if (x >= 0.80 * threshold && x < 0.90 * threshold)
                {
                    priority = 6;
                }
                // 90% <= x
                if (x >= 0.90 * threshold)
                {
                    priority = 7;
                }

                if (priority == -1)
                {
                    throw new Exception("La priorité du node est toujours à -1, alors qu'elle devrait être égale à 1..maxPriority.");
                }

                // Updating new priority
                nodePriority[node] = priority;

                if (priority > 1)
                {
                    // Si le node est en cours de visite mais que sa priorité doit être revu à la hausse, on le fait quand même passé dans l'évent pour prévenir le gestionnaire.
                    if (managerTool[node] == true)
                    {
                        if (nodePriority[node] < priority)
                        {
                            //if (hasNeighbourWithBetterPriorityAlreadySpotted(node, priority) == false)
                            //{
                            EventManager.current.HasToBeVisited(node);
                            //}
                        }
                    }
                    else
                    {
                        //if (hasNeighbourWithBetterPriorityAlreadySpotted(node, priority) == false)
                        //{
                        OnSetNodeToTrue(node);
                        EventManager.current.HasToBeVisited(node);
                        //}
                    }

                }
            }

        }
        // Sending new priority to gestionnaire and agents
        EventManager.current.UpdateNodePriority(nodePriority);
    }

    // Méthode vérifiant si un des voisins du noeud 'node' n'est pas déjà prévu d'être visité, et si oui vérifiant qu'il est bien de priorité égale ou supérieure.
    // Si oui, cela signifie que le noeud 'node' va être visité également, et donc il n'est pas nécessaire de l'ajouter à la liste des neouds à visiter.
    bool hasNeighbourWithBetterPriorityAlreadySpotted(Node node, int priority)
    {
        // Pour chaque voisin du node, on vérifie qu'il n'existe pas un voisin déjà prévu d'être visité avec un niveau de priorité similaire ou supérieur au node à check.
        foreach (Edge edge in node.neighs)
        {
            if (priority <= nodePriority[edge.to])
            {
                return true;
            }
        }
        return false;
    }

    // Méthode permettant de set to true un node qui est en cours de visite, ainsi que tous ses voisins.
    public void OnSetNodeToTrue(Node node)
    {
        managerTool[node] = true;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = true;
        }
    }

    // Méthode permettant de set to false un node qui n'est plus en cours de visite; ainsi que tous ses voisins.
    public void OnSetNodeToFalse(Node node)
    {
        managerTool[node] = false;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = false;
        }
    }

    // Méthode lié à l'évent d'update de l'idleness max, detecté dans le script DataManager.
    void OnUpdateMaxIdleness(float value)
    {
        threshold = value;
    }

    // Méthode lié à l'évent de passage sur un node. Le node a été visité, donc il n'est plus prioritaire.
    void OnSettingNodePriorityToOne(Node node)
    {
        nodePriority[node] = 1;
    }

}
