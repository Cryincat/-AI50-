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

    private Vector3 oldPos;

    private GraphGenerator graphGenerator;
    private AgentManageur agentManageur;
    private AgentGestionnaire agentGestionnaire;

    [NonSerialized] private Node node;
    [NonSerialized] private Node destination;

    [NonSerialized] private List<Node> priorityQueue;
    private List<Node> pathToNode;

    private Dictionary<Node,int> nodeAssignated;
    private Dictionary<(Node, Node), List<Node>> managerData;

    private Graph graph;

    private bool isRandomDestination = false;
    private bool clear = false;
    //private bool hasFinish = false;
    //private bool hasFinish1 = false;

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
        EventManager.current.onUpdatingShortestPathData += OnUpdatingShortestPathData;

        // Récupération des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();
        agentManageur = FindObjectOfType<AgentManageur>();
        agentGestionnaire = FindObjectOfType<AgentGestionnaire>();

        // Setup des variables
        destination = null;
        priorityQueue = new List<Node>();
        pathToNode = new List<Node>();
        capsuleRenderer = GetComponentInChildren<Renderer>();
        nodeAssignated = new Dictionary<Node, int>();

        managerData = new Dictionary<(Node, Node), List<Node>>();

        patrollingColor1 = new Color(0.3f, 0.9f, 0f); // Green
        patrollingColor2 = new Color(1f, 0f, 0.3f); // Red
        patrollingColor3 = new Color(0f, 0.3f, 1f); // Blue

        yield return new WaitUntil(() => graphGenerator.isGenerated);
        yield return new WaitUntil(() => agentGestionnaire.isGenerated);
        yield return new WaitUntil(() => agentManageur.isGenerated);

        graph = graphGenerator.graph;
        node = graphGenerator.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        node.WarnAgentVisit();

        oldPos = transform.position;
        isGenerated = true;
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
                //capsuleRenderer.material.SetColor("_Color", Color.green);
                FindDestination();
            }
            // Cas où il reste un noeud particulier à visiter
            else
            {
                // Cas où le chemin vers ce noeud n'est pas encore attribué
                if (pathToNode.Count == 0)
                {
                    
                    capsuleRenderer.material.SetColor("_Color", patrollingColor2);
                    //capsuleRenderer.material.SetColor("_Color", Color.red);
                    //print("| " + this.ToString() + " | je cherche un nouveau chemin pour me rendre au prochain node prioritaire.");
                    if (managerData.ContainsKey((node, priorityQueue[0])))
                    {
                        int count = 0;
                        foreach (Node node in managerData[(node, priorityQueue[0])])
                        {
                            count++;
                            pathToNode.Add(node);
                        }
                            
                    }
                    else
                    {
                        throw new Exception("Pourquoi on se retrouve à le générer alors qu'on la généré juste avant normalement fdp...");
                        List<Node> path = PathFinding(node, priorityQueue[0]);
                        EventManager.current.AddingShortestPath(path, node, priorityQueue[0]);
                        /*
                        while (hasFinish1 == false)
                        {
                            //print("| Gestionnaire | En attente d'une mis à jour du managerData...");
                            //System.Threading.Thread.Sleep(5);
                        }
                        hasFinish1 = false;*/
                        foreach (Node node in managerData[(node, priorityQueue[0])]) pathToNode.Add(node);
                    }
                    destination = pathToNode[0];
                }
                // Cas où il y a un chemin à suivre
                else
                {
                    capsuleRenderer.material.SetColor("_Color", patrollingColor3);
                    //capsuleRenderer.material.SetColor("_Color", Color.blue);
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
                        //print("| " + this.ToString() + " | Visite du node de priorité accomplie.");
                    }
                }
            }
        }
    }

    private void FindDestination()
    {
        Node temp = null;
        if (destination == null)
        {
            var choice = 2;
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
            if (managerData.ContainsKey((GetNode(), node)))
            {
                nbNodeInPath = managerData[(GetNode(), node)].Count;
            }
            else
            {
                List<Node> path = PathFinding(GetNode(), node);
                EventManager.current.AddingShortestPath(path, GetNode(), node);
                /*
                while (hasFinish == false)
                {
                    //print("| Gestionnaire | En attente d'une mis à jour du managerData...");
                    //System.Threading.Thread.Sleep(5);
                }
                hasFinish = false;*/
                nbNodeInPath = path.Count;
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

    public Node GetNode()
    {
        return node;
    }

    private void OnNodeVisited(Node node)
    {
        if (node == priorityQueue[0] )
        {
            clear = true;
        }
        if (nodeAssignated.Keys.Contains(node))
        {
            nodeAssignated.Remove(node);
        }
    }

    void OnUpdatingShortestPathData(Dictionary<(Node, Node), List<Node>> shortestPathData)
    {
        managerData = shortestPathData;
        //hasFinish = true;
        //hasFinish1 = true;
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
