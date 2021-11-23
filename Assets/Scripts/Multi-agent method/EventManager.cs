using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class EventManager : MonoBehaviour
{

    public static EventManager current;

    private void Awake()
    {
        current = this;
    }

    // évenement correspondant à un node tag comme devant être visité au plus vite.
    public event Action<Node> onHasToBeVisited;
    public void HasToBeVisited(Node node)
    {
        if (onHasToBeVisited != null)
        {
            //print("| Event manager | hasToBeVisited");
            onHasToBeVisited(node);
        }
    }

    // évenement correspondant à l'update de la liste des Keys correspondant à au dictionnaire nodeAssignation assignant chaque node à visiter à un agent précis
    public event Action<Dictionary<Node,int>> onUpdateNodePriority;
    public void UpdateNodePriority(Dictionary<Node, int> nodePriority)
    {
        if (onUpdateNodePriority != null)
        {
            //print("| Event manager | onUpdatingKeyList");
            onUpdateNodePriority(nodePriority);
        }
    }

    // évenement correspondant à l'envoie du dictionnaire contenant les shortests path du graph sur lequel on se trouve.
    public event Action<Dictionary<(Node, Node), List<Node>>> onSendShortestPathData;
    public void SendShortestPathData(Dictionary<(Node, Node), List<Node>> shortestPathData)
    {
        if (onSendShortestPathData != null)
        {
            onSendShortestPathData(shortestPathData);
        }
    }

    // évenement correspondant à l'ajout d'un shortest path au dictionnaire reliant un couple de node à son shortest path
    public event Action<List<Node>,Node, Node> onAddingShortestPath;
    public void AddingShortestPath(List<Node> shortestPath, Node start, Node end)
    {
        if (onAddingShortestPath != null)
        {
            //print("| Event manager | onAddingShortestPath");
            onAddingShortestPath(shortestPath, start, end);
        }
    }

    // évenement correspondant à l'update du dictionnaire shortestPath contenu dans l'agent manageur
    public event Action<Dictionary<(Node, Node), List<Node>>> onUpdatingShortestPathData;
    public void UpdatingShortestPathData(Dictionary<(Node, Node), List<Node>> shortestPathData)
    {
        if (onUpdatingShortestPathData != null)
        {
            //print("| Event manager | onUpdatingShortestPathData");
            onUpdatingShortestPathData(shortestPathData);
        }
    }

    // évenement correspondant à l'update de nodeAssignation présent dans l'agent gestionnaire
    public event Action<Dictionary<Node, AgentPatrouilleur>, Dictionary<Node, int>> onUpdateNodeAssignation;
    public void UpdateNodeAssignation(Dictionary<Node, AgentPatrouilleur> nodeAssignation, Dictionary<Node, int> nodePriority)
    {
        if (onUpdateNodeAssignation != null)
        {
            //print("| Event manager | onUpdateNodeAssignation");
            onUpdateNodeAssignation(nodeAssignation,nodePriority);
        }
    }

    // évenement correspondant à la suppression d'un node de nodeAssignation dans l'agent gestionnaire
    public event Action<Node> onRemoveNodeFromNodeAssignation;
    public void RemoveNodeFromNodeAssignation(Node nodeToRemove)
    {
        if (onRemoveNodeFromNodeAssignation != null)
        {
            //print("| Event manager | onRemoveNodeFromNodeAssignation");
            onRemoveNodeFromNodeAssignation(nodeToRemove);
        }
    }

    // évenement correspondant à la mis à jour à TRUE d'un node dans le managerTool de l'agent manageur
    public event Action<Node> onSettingNodeToTrue;
    public void SettingNodeToTrue(Node node)
    {
        if (onSettingNodeToTrue != null)
        {
            //print("| Event manager | onSettingNodeToTrue");
            onSettingNodeToTrue(node);
        }
    }

    // évenement correspondant à la mis à jour à FALSE d'un node dans le managerTool de l'agent manageur
    public event Action<Node> onSettingNodeToFalse;
    public void SettingNodeToFalse(Node node)
    {
        if (onSettingNodeToFalse != null)
        {
            //print("| Event manager | onSettingNodeToFalse");
            onSettingNodeToFalse(node);
        }
    }

    // évenement correspondant à la visite par un autre agent patrouilleur d'un node prévu d'être visité ou en train d'être visité par un patrouilleur.
    public event Action<Node> onNodeTaggedVisited;
    public void NodeTaggedVisited(Node node)
    {
        if (onNodeTaggedVisited != null)
        {
            onNodeTaggedVisited(node);
        }
    }

    public event Action<float> onUpdateNewMaxIdleness;
    public void UpdateNewMaxIdleness(float value)
    {
        if (onUpdateNewMaxIdleness != null)
        {
            onUpdateNewMaxIdleness(value);
        }
    }

}

