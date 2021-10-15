using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Diagnostics;

public class HamiltonianPathGenerator : MonoBehaviour
{
    public bool isDiagVoisin;
    public Component cylindre;
    public int vitesse;
    public float step;

    private Graph graph;
    private Vector3 nextPosition, diff;


    private Node startNode;
    private List<Node> path;
    private Dictionary<Node, List<Node>> dicoVoisin;
    private List<Node> hamiltonianPath;
    private GraphGenerator graphGeneratorInstance;
    private bool ready = false;
    private int iteration = 1;
    private bool setupUpdate = true;

    Stopwatch sw;
    // Start is called before the first frame update
    // ceci est un changement de grande envergure
    IEnumerator Start()
    {
        sw = new Stopwatch();
        dicoVoisin = new Dictionary<Node, List<Node>>();
        hamiltonianPath = new List<Node>();
        graphGeneratorInstance = GameObject.Find("Sols").GetComponent<GraphGenerator>();
        diff = new Vector3();
        nextPosition = new Vector3();


        yield return new WaitUntil(() => graphGeneratorInstance.isGenerated);
        graph = graphGeneratorInstance.graph;
        startNode = graph.nodes[((int)cylindre.transform.position.x, (int)cylindre.transform.position.z)];
        setNeighboursToNode(graph);

        sw.Start();
        print("Recherche de path hamiltonien");
        hamiltonianPath = findHamiltonianPath(graph, startNode, new List<Node>(), startNode);
        foreach (Node node in hamiltonianPath)
        {

        }
        sw.Stop();
        printTime(sw);
        ready = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            if (setupUpdate)
            {
                nextPosition.Set(hamiltonianPath[iteration].pos.Item1, 0, hamiltonianPath[iteration].pos.Item2);
                diff = nextPosition - cylindre.transform.position;
                
                setupUpdate = false;
            }
            
            Vector3 diffTest = nextPosition - cylindre.transform.position;
            cylindre.transform.Translate(diff * Time.deltaTime * vitesse);
            if (diffTest.x <= step && diffTest.x >= -step && diffTest.z <= step && diffTest.z >= -step)
            {
                iteration++;
                if (iteration >= hamiltonianPath.Count())
                {
                    iteration = 0;
                }
                nextPosition.Set(hamiltonianPath[iteration].pos.Item1, 0, hamiltonianPath[iteration].pos.Item2);
                diff = nextPosition - cylindre.transform.position;
            }
        }
        
    }


    void printTime(Stopwatch sw)
    {
        TimeSpan ts = sw.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        print("Temps : " + elapsedTime);
    }

    // Fill the neighbours dictionary with all nodes and their neighbours
    void setNeighboursToNode(Graph graph)
    {
        foreach (Node node in graph.nodes.Values)
        {
            List<Node> voisins = new List<Node>();
            foreach (Node nodeVoisinPotentiel in graph.nodes.Values)
            {
                if (isVoisin(node, nodeVoisinPotentiel,isDiagVoisin))
                {
                    voisins.Add(nodeVoisinPotentiel);
                }
            }
            dicoVoisin.Add(node, voisins);
        }
    }

    // Check if two nodes are neighbours
    bool isVoisin(Node node, Node nodeVoisin, bool diag)
    {
        if ((node.pos.Item1 == nodeVoisin.pos.Item1 - 1 && node.pos.Item2 == nodeVoisin.pos.Item2 )|| (node.pos.Item1 == nodeVoisin.pos.Item1 + 1 && node.pos.Item2 == nodeVoisin.pos.Item2) || (node.pos.Item2 == nodeVoisin.pos.Item2 - 1 && node.pos.Item1 == nodeVoisin.pos.Item1) || (node.pos.Item2 == nodeVoisin.pos.Item2 + 1 && node.pos.Item1 == nodeVoisin.pos.Item1))
        {
            return true;
        }
        if (diag)
        {
            if ((node.pos.Item1 == nodeVoisin.pos.Item1 - 1 && node.pos.Item2 == nodeVoisin.pos.Item2 -1) || (node.pos.Item1 == nodeVoisin.pos.Item1 - 1 && node.pos.Item2 == nodeVoisin.pos.Item2 + 1) || (node.pos.Item1 == nodeVoisin.pos.Item1 + 1 && node.pos.Item2 == nodeVoisin.pos.Item2 - 1) || (node.pos.Item1 == nodeVoisin.pos.Item1 + 1 && node.pos.Item2 == nodeVoisin.pos.Item2 + 1))
            {
                return true;
            }
        }
        return false;
    }

    string listNodeToString(List<Node> list)
    {
        string temp = "";
        foreach (Node node in list)
        {
            temp = temp + " x = " + node.pos.Item1 + " y = " + node.pos.Item2 + " ; ";
        }
        return temp;

    }

    // Check if there is an hamiltonian path in the given graph, if yes return the path, if no return an empty list or null (A COMPLETER).
    List<Node> findHamiltonianPath(Graph graph, Node startNode, List<Node> path, Node nodeActuel)
    {
        path.Add(nodeActuel);
        // print("path =" + listNodeToString(path));
        if (path.Count() == graph.nodes.Count() && dicoVoisin[nodeActuel].Contains(startNode))
        {
            return path;
        }
        foreach (Node voisin in dicoVoisin[nodeActuel])
        {
            if (!path.Contains(voisin))
            {
                path = findHamiltonianPath(graph, startNode, path, voisin);
                if (path.Count() == graph.nodes.Count() && dicoVoisin[path[path.Count()-1]].Contains(startNode))
                {
                    return path;
                }
            }
            if (path.Contains(voisin) && voisin == startNode && path.Count() == graph.nodes.Count())
            {
                return path;
            }
        }
        path.Remove(nodeActuel);
        return path;
    }

}
/*
 * IDEE POUR AMELIORER :
 *  - Algo qui cherche les points importants pour que l'ensemble d'une salle soit visité, et l'agent suit ce path la
 *  - Lancer une recherche de path hamiltonien pour chaque salle dans laquelle un agent entre
 * 
 */