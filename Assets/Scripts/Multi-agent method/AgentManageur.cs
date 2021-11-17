using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class AgentManageur : MonoBehaviour
{
    // Correspond � la valeur seuil de l'oisivet� au del� de laquelle on remarque le noeud comme a visit� d'urgence
    public float threshold = 60;

    // Correspond respectivement au d�lai de lancement (en seconde) et au taux de r�p�tition (en seconde) de la m�thode de check du graphe.
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
        // Liaison entre les �venements et leurs m�thodes
        EventManager.current.onUpdatingKeyList += OnUpdatingKeyList;
        EventManager.current.onAddingShortestPath += OnAddShortestPath;
        EventManager.current.onSettingNodeToTrue += OnSetNodeToTrue;
        EventManager.current.onSettingNodeToFalse += OnSetNodeToFalse;
        EventManager.current.onUpdateNewMaxIdleness += OnUpdateMaxIdleness;

        // R�cup�ration des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();

        // Initialisation des variables
        shortestPathData = new Dictionary<(Node, Node), List<Node>>();
        managerTool = new Dictionary<Node, bool>();
        nodePriority = new Dictionary<Node, int>();


        // Attente de la g�n�ration du script
        yield return new WaitUntil(() => graphGenerator.isGenerated);

        // R�cup�ration des variables issues d'autres scripts
        graph = graphGenerator.graph;

        // M�thode d'initialisation
        LoadManagerTool();

        // G�n�ration = OK
        print("| Manageur | G�n�ration des variables termin�e...");
        isGenerated = true;

        // Lancement � interval r�gulier de la m�thode de check 
        InvokeRepeating("CheckGraphLeveling", delay, repeatRate);

    }

    // M�thode permettant de set l'ensemble des nodes dans managerTool � false
    void LoadManagerTool()
    {
        //print("| Manageur | Initialisation 'managerTool'...");
        foreach (Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
        //print("| Manageur | 'managerTool' initialis�.");
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
                    throw new Exception("La priorit� du node est toujours � -1, alors qu'elle devrait �tre �gale � 1..5.");
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

    // M�thode v�rifiant si un des voisins du noeud 'node' n'est pas d�j� pr�vu d'�tre visit�, et si oui v�rifiant qu'il est bien de priorit� �gale ou sup�rieure.
    // Si oui, cela signifie que le noeud 'node' va �tre visit� �galement, et donc il n'est pas n�cessaire de l'ajouter � la liste des neouds � visiter.
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





    // M�thode ajoutant un nouveau shortest path dans les donn�es des shortestPaths
    void AddShortestPath(List<Node> shortestPath, Node start, Node end)
    {
        shortestPathData.Add((start, end), shortestPath);
        EventManager.current.UpdatingShortestPathData(shortestPathData);
    }



    // M�thode r�pondant � l'�venement d'update de la liste des nodes pr�vues d'�tre visit� par l'agent gestionnaire.
    void OnUpdatingKeyList(Dictionary<Node,int> temp)
    {
        nodePriority = temp;
    }

    // M�thode r�pondant � l'�venement d'ajout d'un plus cours chemin entre 2 nodes.
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
