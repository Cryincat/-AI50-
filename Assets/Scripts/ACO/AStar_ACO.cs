using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStar_ACO : MonoBehaviour
{
    
    private Graph graph;

    public List<fourmis> list_agent;
    public ColonyMulti colonyM;
    public LoadGraph co_graph;
    public List<Node> chemin_reel;

    public bool isGenerated = false;

    // Start is called before the first frame update
    public IEnumerator Start()
    {

        //yield return new WaitUntil(() => FindObjectOfType<ColonyMulti>().isGenerated);
        yield return new WaitUntil(() => FindObjectOfType<Manager_ACO>().colonyGenerated);
        co_graph = FindObjectOfType<LoadGraph>();
        graph = co_graph.graph;
       //foreach (var l in graph.nodes) print("OYOYOYOYOYO" + l.Value.neighs.Count()); // AU DEBUT LES NODES N ONT QUE 2 VOISINS
        list_agent = new List<fourmis>(); 

        colonyM = FindObjectOfType<ColonyMulti>(); // cas où il y a plusieurs colony a gérer

        foreach (var fourm in colonyM.listFourmisSAve)
        {
            //print("/// AGENT ///");
            list_agent.Add(fourm);
            //foreach (var f in fourm.listChemin) print("BBB : " + f.neighs.Count()); // ME RETOUR DE COLONY MULTI A 3 VOISINS
            //foreach (var j in fourm.listChemin) print(j.pos);
        }// Il y a bien les 50 nodes du graph repartis
        
        
     
        foreach (var agent in list_agent) // pour chaque agent
        {
             
            for (int i =0; i< agent.listChemin.Count;i++) // pour chaque node
            {
               
                if (i < (agent.listChemin.Count - 1)){
                   
                    List<Node> a = new List<Node>();
                   
                    a = this.GetShortestPathAstar(agent.listChemin[i], agent.listChemin[(i+1)],graph); // calcul du chemin reel
                   
                    foreach (var tab in a) agent.listCheminReel.Add(tab);
                    
                   
                }
                else
                {
                    
                    List<Node> a = new List<Node>();
                    
                    a = this.GetShortestPathAstar(agent.listChemin[i], agent.listChemin[0], graph);
                    
                    foreach (var tab in a) agent.listCheminReel.Add(tab); 
                    
                }
            }
            
        }
        print("// fin a star //");
        
        isGenerated = true;
        yield return null;
    }

    public List<Node> GetShortestPathAstar(Node begin, Node end, Graph graph)
    {
        //print("BEGIN : " + begin.neighs.Count() + "END :" + end.neighs.Count());// LE BEGIN ET END ONT 3 VOISINS
        //foreach (var f in graph.nodes) print("HAAAAAAA" + f.Value.neighs.Count()); // LE GRAPH LES NODES ONT QUE 2 VOISINS
        
        Node beginReal = graph.nodes[begin.pos];
        Node endReal = graph.nodes[end.pos];
        //print("begin de base : " + end.pos + "voisins :" + end.neighs.Count());
        //print("begin REAL : " + endReal.pos + "voisins REAL:" + endReal.neighs.Count());
        foreach (Node node in graph.nodes.Values)
        {
           
            node.StraightLineDistanceToEnd = Vector3.Distance(node.realPos, end.realPos);
      
            node.Visited = false;
            node.MinCostToStart = null;
            node.NearestToStart = null;
        }
        //print("count graph value" + graph.nodes.Values.Count);
        
        AstarSearch(beginReal, endReal);
        List<Node> shortestPath = new List<Node>();
        //shortestPath.Clear();
        shortestPath.Add(endReal);
        BuildShortestPath(shortestPath, endReal);
        shortestPath.Reverse();

        //print("---");
        //print("node a :" + begin.pos+ "node b :" +end.pos);
        //foreach (var t in shortestPath) print(t.pos);
        //print("---");
        return shortestPath;
        
    }
    private void BuildShortestPath(List<Node> list, Node node)
    {
        if (node.NearestToStart == null)
            return;
        list.Add(node.NearestToStart);
        //print("nod : "+ node.pos + "node nearest :" + node.NearestToStart.pos);
        //print("NODE NEAREST TO START" + node.NearestToStart.MinCostToStart);
        BuildShortestPath(list, node.NearestToStart);
    }

    private void AstarSearch(Node begin, Node end)
    {
        begin.MinCostToStart = 0;
        var prioQueue = new List<Node>();
        prioQueue.Add(begin);
        //print("BEGIN :" + begin.neighs.Count());
        do
        {
            prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToEnd).ToList();
            var node = prioQueue.First();
            //print("noide :" + node.pos);
            prioQueue.Remove(node);

            //print("ET LA LE DRAME :" + node.neighs.Count());
            foreach (var cnn in node.neighs.OrderBy(x => x.cost))
            {
                var childNode = cnn.to;
                if (childNode.Visited)
                    continue;
                if (childNode.MinCostToStart == null || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    //print("min cost tostart" + childNode.MinCostToStart);
                    //print("NODDDESSSSS : " + node.pos);
                    childNode.NearestToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.Visited = true;
            if (node == end)
                return;
        } while (prioQueue.Any());
        //print("///" + graph.nodes.Where(x => x.Value.Visited && x.Value.MinCostToStart == 0).Count());
    }
}

