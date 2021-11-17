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

    public bool isGenerated = false;

    private Graph graph;
    private GraphGenerator graphGenerator;

    private Dictionary<(Node, Node), List<Node>> shortestPathData;
    private Dictionary<Node, bool> managerTool;

    private Dictionary<Node,int> nodePriority;

    IEnumerator Start()
    {
        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onUpdatingKeyList += OnUpdatingKeyList;
        EventManager.current.onAddingShortestPath += OnAddShortestPath;
        EventManager.current.onSettingNodeToTrue += OnSetNodeToTrue;
        EventManager.current.onSettingNodeToFalse += OnSetNodeToFalse;
        EventManager.current.onUpdateNewMaxIdleness += OnUpdateMaxIdleness;

        // Récupération des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();

        // Initialisation des variables
        shortestPathData = new Dictionary<(Node, Node), List<Node>>();
        managerTool = new Dictionary<Node, bool>();
        nodePriority = new Dictionary<Node, int>();


        // Attente de la génération du script
        yield return new WaitUntil(() => graphGenerator.isGenerated);

        // Récupération des variables issues d'autres scripts
        graph = graphGenerator.graph;

        // Méthode d'initialisation
        LoadManagerTool();

        // Génération = OK
        print("| Manageur | Génération des variables terminée...");
        isGenerated = true;

        // Lancement à interval régulier de la méthode de check 
        InvokeRepeating("CheckGraphLeveling", delay, repeatRate);

    }

    // Méthode permettant de set l'ensemble des nodes dans managerTool à false
    void LoadManagerTool()
    {
        //print("| Manageur | Initialisation 'managerTool'...");
        foreach (Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
        //print("| Manageur | 'managerTool' initialisé.");
    }

    void CheckGraphLeveling()
    {
        int priority = -1;
        foreach (Node node in graph.nodes.Values)
        {
            if (managerTool[node] == false)
            {
                
                float x = node.timeSinceLastVisit;
                // 0% <= x < 20%
                if (x < 0.2 * threshold)
                {
                    priority = 1;
                }
                // 20% <= x < 50%
                if (x >= 0.2 * threshold && x < 0.5 * threshold)
                {
                    priority = 2;
                }
                // 50% <= x < 80%
                if (x >= 0.5 * threshold && x < 0.8 * threshold)
                {
                    priority = 3;
                }
                // 80% <= x < 100%
                if (x >= 0.8 * threshold && x < threshold)
                {
                    priority = 4;
                }
                // 100% <= x
                if (x >= threshold) 
                {
                    priority = 5;
                }
                if (priority == -1)
                {
                    throw new Exception("La priorité du node est toujours à -1, alors qu'elle devrait être égale à 1..5.");
                }
                if (priority > 1)
                {
                    
                    if (hasNeighbourWithBetterPriorityAlreadySpotted(node, priority) == false)
                    {
                        OnSetNodeToTrue(node);
                        EventManager.current.HasToBeVisited(node, priority);
                    }

                }
            }

        }
    }

    // Méthode vérifiant si un des voisins du noeud 'node' n'est pas déjà prévu d'être visité, et si oui vérifiant qu'il est bien de priorité égale ou supérieure.
    // Si oui, cela signifie que le noeud 'node' va être visité également, et donc il n'est pas nécessaire de l'ajouter à la liste des neouds à visiter.
    bool hasNeighbourWithBetterPriorityAlreadySpotted(Node node, int priority)
    {
        foreach (Edge edge in node.neighs)
        {
            if (nodePriority.Keys.Contains(edge.to))
            {
                if (priority <= nodePriority[edge.to])
                {
                    return true;
                }
            }
        }
        return false;
    }





    // Méthode ajoutant un nouveau shortest path dans les données des shortestPaths
    void AddShortestPath(List<Node> shortestPath, Node start, Node end)
    {
        shortestPathData.Add((start, end), shortestPath);
        EventManager.current.UpdatingShortestPathData(shortestPathData);
    }



    // Méthode répondant à l'évenement d'update de la liste des nodes prévues d'être visité par l'agent gestionnaire.
    void OnUpdatingKeyList(Dictionary<Node,int> temp)
    {
        nodePriority = temp;
    }

    // Méthode répondant à l'évenement d'ajout d'un plus cours chemin entre 2 nodes.
    void OnAddShortestPath(List<Node> shortestPath, Node start, Node end)
    {
        AddShortestPath(shortestPath, start, end);
    }

    public void OnSetNodeToTrue(Node node)
    {
        managerTool[node] = true;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = true;
        }
    }

    public void OnSetNodeToFalse(Node node)
    {
        managerTool[node] = false;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = false;
        }
    }

    void OnUpdateMaxIdleness(float value)
    {
        threshold = value;
    }

}
