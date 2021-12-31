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

        yield return new WaitUntil(() => FindObjectOfType<Manager_ACO>().colonyGenerated);
        co_graph = FindObjectOfType<LoadGraph>();
        graph = co_graph.graph;
        list_agent = new List<fourmis>(); 

        colonyM = FindObjectOfType<ColonyMulti>(); // cas où il y a plusieurs colony a gérer

        foreach (var fourm in colonyM.listFourmisSAve)
        {
            list_agent.Add(fourm);          
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
     
        
        isGenerated = true;
        yield return null;
    }

    public List<Node> GetShortestPathAstar(Node begin, Node end, Graph graph)
    {       
        Node beginReal = graph.nodes[begin.pos];
        Node endReal = graph.nodes[end.pos];
        foreach (Node node in graph.nodes.Values)
        {
           
            node.StraightLineDistanceToEnd = Vector3.Distance(node.realPos, end.realPos);
      
            node.Visited = false;
            node.MinCostToStart = null;
            node.NearestToStart = null;
        }  
        AstarSearch(beginReal, endReal);
        List<Node> shortestPath = new List<Node>();
        shortestPath.Add(endReal);
        BuildShortestPath(shortestPath, endReal);
        shortestPath.Reverse();

        return shortestPath;
        
    }
    private void BuildShortestPath(List<Node> list, Node node)
    {
        if (node.NearestToStart == null)
            return;
        list.Add(node.NearestToStart);
        BuildShortestPath(list, node.NearestToStart);
    }

    private void AstarSearch(Node begin, Node end)
    {
        begin.MinCostToStart = 0;
        var prioQueue = new List<Node>();
        prioQueue.Add(begin);
        do
        {
            prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToEnd).ToList();
            var node = prioQueue.First();
            prioQueue.Remove(node);

            foreach (var cnn in node.neighs.OrderBy(x => x.cost))
            {
                var childNode = cnn.to;
                if (childNode.Visited)
                    continue;
                if (childNode.MinCostToStart == null || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    childNode.NearestToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.Visited = true;
            if (node == end)
                return;
        } while (prioQueue.Any());
    }
}

