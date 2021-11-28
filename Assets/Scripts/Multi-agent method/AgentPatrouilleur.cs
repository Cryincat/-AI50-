using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class AgentPatrouilleur : MonoBehaviour
{
    public bool isGenerated = false;
    public float speed = 1f;
    public bool isGraphLoading = false;
    public Node startPoint;


    private Vector3 oldPos;

    private LoadGraph loadGraph;
    private GraphGenerator graphGenerator;
    private AgentManageur agentManageur;
    private AgentGestionnaire agentGestionnaire;

    [NonSerialized] private Node node;
    [NonSerialized] private Node destination;

    [NonSerialized] private List<Node> priorityQueue;
    private List<Node> pathToNode;

    private Dictionary<Node,int> nodeAssignated;
    private Dictionary<(Node, Node), List<Node>> shortestPathData;

    private Graph graph;

    private bool isRandomDestination = false;
    private bool clear = false;

    private GameObject capsule;
    private Renderer capsuleRenderer;

    private Color patrollingColor1;
    private Color patrollingColor2;
    private Color patrollingColor3;

    public IEnumerator Start()
    {
        

        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onNodeTaggedVisited += OnNodeVisited;
        EventManager.current.onUpdateNodeAssignation += OnUpdateNodeAssignation;
        EventManager.current.onSendShortestPathData += OnSendShortestPathData;

        //Set position of agent
        transform.position = startPoint.realPos;

        // Récupération des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();
        agentManageur = FindObjectOfType<AgentManageur>();
        agentGestionnaire = FindObjectOfType<AgentGestionnaire>();
        capsuleRenderer = GetComponentInChildren<Renderer>();
        loadGraph = FindObjectOfType<LoadGraph>();

        // Setup des variables
        destination = null;

        priorityQueue = new List<Node>();
        pathToNode = new List<Node>();

        nodeAssignated = new Dictionary<Node, int>();
        shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        patrollingColor1 = new Color(0.5f, 0.9f, 0.2f); // Green
        patrollingColor2 = new Color(1f, 0.2f, 0.5f); // Red
        patrollingColor3 = new Color(0.2f, 0.5f, 1f); // Blue

        // Attente que le graph s'initialise, puis l'agent manageur et enfin l'agent gestionnaire.
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
        
        yield return new WaitUntil(() => agentManageur.isGenerated);
        yield return new WaitUntil(() => agentGestionnaire.isGenerated);

        
        

        



       
        node = graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        node.WarnAgentVisit();

        oldPos = transform.position;
        isGenerated = true;
        FindObjectOfType<TimeManager>().delta = 1;
    }

    private void Update()
    {
        if (node != null && isGenerated)
        {
            if (clear == true)
            {
                priorityQueue.Clear();
                pathToNode.Clear();
                clear = false;
            }
            if (priorityQueue.Count == 0)
            {
                Node temp = CheckForNode();
                if (temp != null)
                {
                    priorityQueue.Add(temp);
                    // REMOVE LE NODE AVANT DE L'AVOIR REACH OU APRES? ICI AVANT
                    //EventManager.current.RemoveNodeFromNodeAssignation(temp);
                }
            }

            // Cas où aucun noeud particulier n'est à visiter
            if (priorityQueue.Count == 0)
            {
                isRandomDestination = true;
                capsuleRenderer.material.SetColor("_Color", patrollingColor1);
                FindDestination();
            }
            // Cas où il reste un noeud particulier à visiter
            else
            {
                // Cas où le chemin vers ce noeud n'est pas encore attribué
                if (pathToNode.Count == 0)
                {
                    capsuleRenderer.material.SetColor("_Color", patrollingColor2);
                    if (shortestPathData.ContainsKey((node, priorityQueue[0])))
                    {
                        foreach (Node node in shortestPathData[(node, priorityQueue[0])])
                        {
                            pathToNode.Add(node);
                        }
                            
                    }
                    else
                    {
                        throw new Exception("The path from node (" + node.pos.Item1 + "," + node.pos.Item2 + ") to node (" + priorityQueue[0].pos.Item1 + "," + priorityQueue[0].pos.Item2 + ") doesn't exist.");
                    }
                    destination = pathToNode[0];
                }
                // Cas où il y a un chemin à suivre
                else
                {
                    capsuleRenderer.material.SetColor("_Color", patrollingColor3);
                    destination = pathToNode[0];
                    EventManager.current.SettingNodeToTrue(destination);
                }
            }
            if (destination != null)
            {
                GoToDestination();
            }
        }
    }

    // Méthode permettant de faire avancer l'agent vers sa prochaine destination.
    private void GoToDestination()
    {
        speed = Mathf.Abs(speed);
        if (transform.position != oldPos)
        {
            throw new Exception("Erreur : la position a été modifié par un autre script.");
        }
        float mouvement = speed * Time.deltaTime;
        GoToDestination(mouvement);
        oldPos = transform.position;
    }

    // Méthode permettant de faire déplacer l'agent vers sa prochaine destination.
    private void GoToDestination(float mouvement)
    {
        if (destination != null)
        {
            Vector3 moveToward = Vector3.MoveTowards(transform.position, destination.realPosFromagentHeights, mouvement);
            mouvement -= Vector3.Distance(transform.position, moveToward);
            transform.position = moveToward;
            if (Vector3.Distance(moveToward, destination.realPosFromagentHeights) < 0.01)
            {
                EventManager.current.RemoveNodeFromNodeAssignation(node);
                EventManager.current.NodeTaggedVisited(node);
                node.agentPresence = false;
                node = destination;
                node.WarnAgentVisit();
                EventManager.current.SettingNodeToFalse(node);
                destination = null;
                if (isRandomDestination == true)
                {
                    isRandomDestination = false;
                }
                else
                {
                    pathToNode.RemoveAt(0);
                    if (pathToNode.Count == 0)
                    {
                        nodeAssignated.Remove(node);
                        priorityQueue.Remove(node);
                    }
                }
            }
        }
    }

    // Méthode permettant de récupérer la prochaine destination logique la plus proche de l'agent.
    private void FindDestination()
    {
        Node temp = null;
        if (destination == null)
        {
            var choice = 0;
            System.Random random = new System.Random();

            switch (choice)
            {
                case 0:
                    temp = node.neighs[random.Next(node.neighs.Count)].to;
                    destination = temp;
                    EventManager.current.SettingNodeToTrue(temp);
                    break;
                case 1:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Sum(y => y.to.timeSinceLastVisit))).First().to;
                    destination = temp;
                    EventManager.current.SettingNodeToTrue(temp);
                    break;
                case 2:
                    temp = (node.neighs.OrderByDescending(x => x.to.neighs.Max(y => y.to.timeSinceLastVisit))).First().to;
                    destination = temp;
                    EventManager.current.SettingNodeToTrue(temp);
                    break;
            }
        }
    }

    // Méthode permettant de récupérer le meilleur node à visiter au plus vite par l'agent. Crée une liste de tous les nodes de priorité
    // la plus haute qui lui sont assignés, et choisi le plus proche en fonction du path dans le graphe.
    Node CheckForNode()
    {
        if (nodeAssignated.Count == 0)
        {
            return null;
        }
        int priority = -1;
        List<Node> listForCheck = new List<Node>();

        // On récupère la liste des nodes de plus haute priorité
        foreach(Node node in nodeAssignated.Keys)
        {
            if (nodeAssignated[node] > priority)
            {
                priority = nodeAssignated[node];
                listForCheck.Clear();
                listForCheck.Add(node);
            }
            else if (priority == nodeAssignated[node])
            {
                listForCheck.Add(node);
            }
        }
        
        Node bestNode = null;
        float dist = Mathf.Infinity;

        foreach (Node node in listForCheck)
        {
            float nbNodeInPath = 999;
            if (shortestPathData.ContainsKey((GetNode(), node)))
            {
                nbNodeInPath = shortestPathData[(GetNode(), node)].Count;
            }
            else
            {
                throw new Exception("The path from node (" + GetNode().pos.Item1 + "," + GetNode().pos.Item2 + ") to node (" + node.pos.Item1 + "," + node.pos.Item2 + ") doesn't exist.");
            }

            if (dist > nbNodeInPath)
            {
                
                bestNode = node;
                dist = nbNodeInPath;
            }
            if (nbNodeInPath <= 0)
            {
                throw new Exception("Probleme dans le CheckForNodes d'un patrouilleur.");
            }
        }
        return bestNode;
    }

    // Méthode lié à l'évenement d'update de nodeAssignation. Clear la liste existante et récupère les nodes présent dans la nouvelle qui lui sont assignés..
    void OnUpdateNodeAssignation(Dictionary<Node, AgentPatrouilleur> _nodeAssignation, Dictionary<Node, int> nodePriority)
    {
        nodeAssignated.Clear();
        foreach (Node node in _nodeAssignation.Keys)
        {
            if (_nodeAssignation[node].ToString() == this.ToString())
            {
                nodeAssignated.Add(node,nodePriority[node]);
            }
        }
    }

    // Méthode renvoyant le node sur lequel se trouve l'agent.
    public Node GetNode()
    {
        return node;
    }

    //Méthode lié à l'évenent de visite d'un node. Si par hasard un node devant être visité par cet agent a été visité par un autre, on le remove des nodes qui lui sont assignés.
    private void OnNodeVisited(Node node)
    {
        if (priorityQueue.Count != 0)
        {
            if (node == priorityQueue[0])
            {
                clear = true;
            }
        }
        if (nodeAssignated.Keys.Contains(node))
        {
            nodeAssignated.Remove(node);
        }
    }

    // Méthode de récupération des paths liant chaque node à chaque node dans le graphe.
    void OnSendShortestPathData(Dictionary<(Node, Node), List<Node>> _shortestPathData)
    {
        shortestPathData = _shortestPathData;

    }
}
