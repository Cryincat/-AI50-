using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{

    Graph graph;
    bool recup = false;
    private Node startNode;
    private List<Node> path;
    private Dictionary<Node, List<Node>> dicoVoisin = new Dictionary<Node, List<Node>>();
    public Component cylindre;
    private List<Node> hamiltonianPath = new List<Node>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (recup == false)
        {
            graph = GameObject.Find("Sols").GetComponent<GraphGenerator>().graph;
            recup = true;
            /*foreach(Node node in graph.nodes.Values)
            {
                print(node.pos);
            }*/
            startNode = graph.nodes[(0, 0)];
            Dictionary<Node, bool> dicoVisited = new Dictionary<Node, bool>();
            setNeighboursToNode(graph);
            hamiltonianPath = findHamiltonianPath(graph, startNode, new List<Node>(), startNode, dicoVisited);

           


        }
        System.Threading.Thread.Sleep(120);
        Vector3 newPosition = new Vector3(hamiltonianPath[0].pos.Item1, 2, hamiltonianPath[0].pos.Item2);
        print(newPosition);
        cylindre.transform.position = newPosition;
        hamiltonianPath.Add(hamiltonianPath[0]);
        hamiltonianPath.RemoveAt(0);
    }


    // Clear dictionary of visited nodes, and set only path as visited
    void setVisitedWithPath(Dictionary<Node, bool> dicoVisited, List<Node> path)
    {

        dicoVisited.Clear();
        if (path.Count() == 0)
        {
            return;
        }
        foreach (Node node in path)
        {
            dicoVisited.Add(node, true);
        }
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
    List<Node> findHamiltonianPath(Graph graph, Node startNode, List<Node> path, Node nodeActuel, Dictionary<Node, bool> dicoVisited)
    {
        path.Add(nodeActuel);
        print("1) path actuel : " + listNodeToString(path));
        
        print("2) Ajout du node de position : " + nodeActuel.pos);
        if (path.Count() == graph.nodes.Count() && dicoVoisin[nodeActuel].Contains(startNode))
        {
            print("return 1");
            return path;
        }
        print("3) Les voisins du node actuel de pos : " + nodeActuel.pos + " sont :" + listNodeToString(dicoVoisin[nodeActuel]));
        foreach (Node voisin in dicoVoisin[nodeActuel])
        {
            if (!path.Contains(voisin))
            {
                print("4) if sur le voisin de pos : " + voisin.pos);
                path = findHamiltonianPath(graph, startNode, path, voisin, dicoVisited);
                if (path.Count() == graph.nodes.Count() && dicoVoisin[path[path.Count()-1]].Contains(startNode))
                {
                    print("return 2");
                    return path;
                }
            }
            if (path.Contains(voisin) && voisin == startNode && path.Count() == graph.nodes.Count())
            {
                print("return 3");
                return path;
            }
        }
        print("retrait du node de position : " + nodeActuel.pos);
        path.Remove(nodeActuel);
        print("return 4");
        return path;
    }

}
