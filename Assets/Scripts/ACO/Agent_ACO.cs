using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class Agent_ACO : MonoBehaviour
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
    public Node destination;

    [NonSerialized] private List<Node> priorityQueue;
    public List<Node> pathToNode;

    private Dictionary<Node, int> nodeAssignated;
    private Dictionary<(Node, Node), List<Node>> shortestPathData;

    private Graph graph;

    //private bool isRandomDestination = false;
    private bool clear = false;

    private GameObject capsule;
    private Renderer capsuleRenderer;

    private Color patrollingColor1;
    private Color patrollingColor2;
    private Color patrollingColor3;

    public Manager_ACO manag;

    public bool startIsDOne = false;

    

    public void Start()
    {
        //yield return new WaitUntil(() => FindObjectOfType<AStar_ACO>().isGenerated);

        transform.position = startPoint.realPos;
        node = startPoint;

        

        // Récupération des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();
        capsuleRenderer = GetComponentInChildren<Renderer>();
        loadGraph = FindObjectOfType<LoadGraph>();
        

        //priorityQueue = new List<Node>();
        pathToNode = new List<Node>();

        patrollingColor1 = new Color(0.5f, 0.9f, 0.2f); // Green
        patrollingColor2 = new Color(1f, 0.2f, 0.5f); // Red
        patrollingColor3 = new Color(0.2f, 0.5f, 1f); // Blue

        oldPos = transform.position;

        manag = FindObjectOfType<Manager_ACO>();

        print("NEW AGENT");
        foreach (var s in manag.realTimeNodes)
        {
            pathToNode.Add(s);
            print(s.pos);
        }

        destination = null;
        startIsDOne = true;
        isGenerated = true;
    }

    private void Update()
    {
        if(startIsDOne && node != null)
        {
            if (destination == null)
            {
                destination = pathToNode[0]; // donne ton prochain node
                
            }
            GoToDestination();
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
            Vector3 moveToward = Vector3.MoveTowards(transform.position, destination.realPosFromAgentHeights, mouvement);
            mouvement -= Vector3.Distance(transform.position, moveToward);
            transform.position = moveToward;
            if (Vector3.Distance(moveToward, destination.realPosFromAgentHeights) < 0.01)
            {
                
                node.agentPresence = false;
                node = destination;
                node.WarnAgentVisit();
                pathToNode.Add(pathToNode[0]);
                pathToNode.RemoveAt(0);
                destination = null;
               
            }
        }
    }

    public void SetList(List<Node> n)
    {
        pathToNode.Clear();
        foreach (var y in n) pathToNode.Add(y);
    }
    // Méthode lié à l'évenement d'update de nodeAssignation. Clear la liste existante et récupère les nodes présent dans la nouvelle qui lui sont assignés..
  
    // Méthode renvoyant le node sur lequel se trouve l'agent.
    public Node GetNode()
    {
        return node;
    }


}
