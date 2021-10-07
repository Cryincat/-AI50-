using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HamiltonianPathGenerator : MonoBehaviour
{

    private Graph graph;
    private Node startNode;
    private List<Node> path;
    private Dictionary<Node, List<Node>> dicoVoisin;
    public Component cylindre;
    private List<Node> hamiltonianPath;
    private GraphGenerator graphGeneratorInstance;
    private bool ready = false;
    // Start is called before the first frame update
    // ceci est un changement de grande envergure
    IEnumerator Start()
    {
        dicoVoisin = new Dictionary<Node, List<Node>>();
        hamiltonianPath = new List<Node>();
        graphGeneratorInstance = GameObject.Find("Sols").GetComponent<GraphGenerator>();

        yield return new WaitUntil(() => graphGeneratorInstance.isGenerated);
        graph = graphGeneratorInstance.graph;
        startNode = graph.nodes[(0, 0)];
        setNeighboursToNode(graph);
        hamiltonianPath = findHamiltonianPath(graph, startNode, new List<Node>(), startNode);
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            Vector3 newPosition = new Vector3(hamiltonianPath[0].pos.Item1, 2, hamiltonianPath[0].pos.Item2);
            cylindre.transform.position = newPosition;
            hamiltonianPath.Add(hamiltonianPath[0]);
            hamiltonianPath.RemoveAt(0);
        }
        
        System.Threading.Thread.Sleep(120);
    }



    // Fill the neighbours dictionary with all nodes and their neighbours
    void setNeighboursToNode(Graph graph)
    {
        foreach (Node node in graph.nodes.Values)
        {
            List<Node> voisins = new List<Node>();
            foreach (Node nodeVoisinPotentiel in graph.nodes.Values)
            {
                if (isVoisin(node, nodeVoisinPotentiel))
                {
                    voisins.Add(nodeVoisinPotentiel);
                }
            }
            dicoVoisin.Add(node, voisins);
        }
    }

    // Check if two nodes are neighbours
    bool isVoisin(Node node, Node nodeVoisin)
    {
        if ((node.pos.Item1 == nodeVoisin.pos.Item1 - 1 && node.pos.Item2 == nodeVoisin.pos.Item2 )|| (node.pos.Item1 == nodeVoisin.pos.Item1 + 1 && node.pos.Item2 == nodeVoisin.pos.Item2) || (node.pos.Item2 == nodeVoisin.pos.Item2 - 1 && node.pos.Item1 == nodeVoisin.pos.Item1) || (node.pos.Item2 == nodeVoisin.pos.Item2 + 1 && node.pos.Item1 == nodeVoisin.pos.Item1))
        {
            return true;
        }
        return false;
    }

    string listNodeToString(List<Node> list)
    {
        string temp = "";
        foreach (Node node in list)
        {
            temp = temp + " x = " + node.pos.Item1 + " y = " + node.pos.Item2 + ". \n";
        }
        return temp;

    }

    // Check if there is an hamiltonian path in the given graph, if yes return the path, if no return an empty list or null (A COMPLETER).
    List<Node> findHamiltonianPath(Graph graph, Node startNode, List<Node> path, Node nodeActuel)
    {
        path.Add(nodeActuel);

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
