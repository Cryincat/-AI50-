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

    private Dictionary<Node, bool> managerTool;
    private Dictionary<Node, int> nodePriority;

    IEnumerator Start()
    {
        // Liaison entre les �venements et leurs m�thodes
        EventManager.current.onSettingNodeToTrue += OnSetNodeToTrue;
        EventManager.current.onSettingNodeToFalse += OnSetNodeToFalse;
        EventManager.current.onUpdateNewMaxIdleness += OnUpdateMaxIdleness;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnSettingNodePriorityToOne;

        // R�cup�ration des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();

        // Initialisation des variables

        managerTool = new Dictionary<Node, bool>();
        nodePriority = new Dictionary<Node, int>();


        // Attente de la g�n�ration du script
        yield return new WaitUntil(() => graphGenerator.isGenerated);

        // R�cup�ration des variables issues d'autres scripts
        graph = graphGenerator.graph;

        // M�thode d'initialisation
        LoadManagerTool();
        LoadNodePriority();
        LoadPaths();


        // G�n�ration = OK
        print("| Manageur | G�n�ration des variables termin�e...");
        isGenerated = true;

        // Lancement � interval r�gulier de la m�thode de check 
        InvokeRepeating("CheckGraphLeveling", delay, repeatRate);

    }

    void LoadNodePriority()
    {
        foreach (Node node in graph.nodes.Values)
        {
            nodePriority.Add(node, 1);
        }
    }

    // M�thode permettant de set l'ensemble des nodes dans managerTool � false
    void LoadManagerTool()
    {
        //print("| Manageur | Initialisation 'managerTool'...");
        foreach (Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
        print("| Manageur | 'managerTool' initialis�.");
    }

    // M�thode permettant de charger pr�alablement tous les paths existants entre 2 nodes dans le graphe.
    void LoadPaths()
    {
        print("| Manageur | Itinialisation des donn�es des shortestPaths...");
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
                shortestPathData.Add((start, end), aStar.GetShortestPathAstar(start, end));

            }
        }
        print("| Manageur | Path " + count + "/" + totalPath);
        EventManager.current.SendShortestPathData(shortestPathData);
        print("| Manageur | ShortestPaths initialis�s.");
    }

    void CheckGraphLeveling()
    {

        int priority = -1;
        foreach (Node node in graph.nodes.Values)
        {
            if (managerTool[node] == false || (managerTool[node] == true && nodePriority[node] < 5))
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

                // Updating new priority
                nodePriority[node] = priority;

                if (priority > 1)
                {
                    // Si le node est en cours de visite mais que sa priorit� doit �tre revu � la hausse, on le fait quand m�me pass� dans l'�vent pour pr�venir le gestionnaire.
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

    // M�thode v�rifiant si un des voisins du noeud 'node' n'est pas d�j� pr�vu d'�tre visit�, et si oui v�rifiant qu'il est bien de priorit� �gale ou sup�rieure.
    // Si oui, cela signifie que le noeud 'node' va �tre visit� �galement, et donc il n'est pas n�cessaire de l'ajouter � la liste des neouds � visiter.
    bool hasNeighbourWithBetterPriorityAlreadySpotted(Node node, int priority)
    {
        // Pour chaque voisin du node, on v�rifie qu'il n'existe pas un voisin d�j� pr�vu d'�tre visit� avec un niveau de priorit� similaire ou sup�rieur au node � check.
        foreach (Edge edge in node.neighs)
        {
            if (priority <= nodePriority[edge.to])
            {
                return true;
            }
        }
        return false;
    }

    // M�thode permettant de set to true un node qui est en cours de visite, ainsi que tous ses voisins.
    public void OnSetNodeToTrue(Node node)
    {
        managerTool[node] = true;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = true;
        }
    }

    // M�thode permettant de set to false un node qui n'est plus en cours de visite; ainsi que tous ses voisins.
    public void OnSetNodeToFalse(Node node)
    {
        managerTool[node] = false;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = false;
        }
    }

    // M�thode li� � l'�vent d'update de l'idleness max, detect� dans le script DataManager.
    void OnUpdateMaxIdleness(float value)
    {
        threshold = value;
    }

    // M�thode li� � l'�vent de passage sur un node. Le node a �t� visit�, donc il n'est plus prioritaire.
    void OnSettingNodePriorityToOne(Node node)
    {
        nodePriority[node] = 1;
    }

}
