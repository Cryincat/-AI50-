using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonyMulti : MonoBehaviour
{
    
    public List<Node> graphNormal;
    //public Dictionary<(int, int), Node> nodes;
    public List<Edge> edgeNormal;
    
    public float coutCheminSave;
    public List<fourmis> listFourmisSAve;

    public int nbAgents;
    public Node posColony;
    public float alpha;
    public float beta;
    public float tauxEvap;

    public CompleteGraph object_graph;
    public Graph graph_complet;

    public bool isGenerated = false;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<CompleteGraph>().isGenerated);
        coutCheminSave = Mathf.Infinity;
        listFourmisSAve = new List<fourmis>();
        edgeNormal = new List<Edge>();
        graphNormal = new List<Node>();

        object_graph = FindObjectOfType<CompleteGraph>();
        graph_complet = object_graph.graph;

        foreach (var ed in graph_complet.edges) { 
            edgeNormal.Add(ed);
            
        } // copie de tout les edges
        foreach (var no in graph_complet.nodes) { graphNormal.Add(no.Value); } // copie de tout les nodes

        launch();

        
        foreach (var g in listFourmisSAve)
        {
            print("CHEMIN VIRTUEL");
            foreach (var f in g.listChemin) print(f.pos);
        }

        isGenerated = true;
        yield return null;

    }

    public void launch()
    {
        int tantQue = 1;
        int cout = 0;
        float coutChemin = 0;
        List<fourmis> listFourmis = new List<fourmis>();
        float coutCheminSave2 = 0;

        System.Random aleatSpawn = new System.Random();
        

        posColony = graphNormal[aleatSpawn.Next(graphNormal.Count)]; // Spawn de la colony au hasard

        List<Edge> listEdges = new List<Edge>();
        foreach (var op in edgeNormal) listEdges.Add(op);

        List<Node> listNodes = new List<Node>();
        

        foreach (var x in listEdges) x.pheromone = 0.01; // Reset pheromones

        while (tantQue == 1) // Tant que ça converge pas
        {
            int estDejaFait = 0;

            listFourmis.Clear();
            foreach (var a in graphNormal) listNodes.Add(a);

            if (cout > 100) tantQue = 0; // SORTIE BOUCLE INFINIE
            if (coutCheminSave == coutCheminSave2 && coutCheminSave2 != 0)
            {
                cout++;
            }
            else
            {
                cout = 0;
            }

            coutCheminSave2 = coutCheminSave;
            coutChemin = 0;

            for (int i = 0; i < nbAgents; i++) // Création fourmis
            {
                fourmis fourm = new fourmis(posColony);
                listFourmis.Add(fourm);
            }

            foreach (var t in listEdges) t.pheromone = (1 - tauxEvap) * t.pheromone; // EVAPORATION

            while (listNodes.Count != 0)
            {

                foreach (var f in listFourmis)
                {
                    if (f.pos == posColony && estDejaFait == 0)
                    {
                        listNodes.Remove(f.pos);
                        f.listChemin.Add(f.pos);
                        estDejaFait = 1;
                    }
                    if (!(listNodes.Count == 0))
                    {
                        f.oldpos = f.pos;
                        
                        f.pos = choixOpti(listNodes, listEdges, f.pos);
                        if(f.pos != posColony) listNodes.Remove(f.pos);
                        f.listChemin.Add(f.pos);
                        var edge = listEdges.Find(x => (x.from == f.oldpos && x.to == f.pos) || (x.from == f.pos && x.to == f.oldpos));
                        int majedge = listEdges.FindIndex(x => (x.from == f.oldpos && x.to == f.pos) || (x.from == f.pos && x.to == f.oldpos));
                        f.listIndexEdgeF.Add(majedge);
                        f.longueur += edge.cost;
                    }
                }
            }

            foreach (var c in listFourmis)
            {
                if (c.longueur != 0) coutChemin += c.longueur;

            }
                    
            if (coutChemin < coutCheminSave)
            {
                coutCheminSave = coutChemin;
                listFourmisSAve.Clear();
                foreach (var y in listFourmis) listFourmisSAve.Add(y);

            }

            foreach (var k in listFourmis)
            {
                foreach (var z in k.listIndexEdgeF) listEdges[z].pheromone += (1 / k.longueur); // MAJ PHEROMONE DES EDGES

            }          
        }
    }

    public Node choixOpti(List<Node> choice, List<Edge> listEdge, Node pos)
    {
        System.Random aleat = new System.Random();

        List<float> probaEdges = new List<float>();
        List<Edge> EdgesSelect = new List<Edge>();

        
        double totalCost = 0;

        foreach (var s in choice)
        {
            if (!(s == pos))
            {
                Edge edgeHinsho = listEdge.Find(x => (x.from == pos && x.to == s) || (x.from == s && x.to == pos));
                totalCost += Mathf.Pow((float)edgeHinsho.pheromone, alpha) * Mathf.Pow((float)(1 / edgeHinsho.cost), beta);
            }


        }

        foreach (var teo in choice) // Calcul de toutes les probas
        {
            if (!(teo == pos))
            {
                Edge edgeHinsh = listEdge.Find(x => (x.from == pos && x.to == teo) || (x.from == teo && x.to == pos));
                EdgesSelect.Add(edgeHinsh);
                float probs = 0;
                probs = (float)((((Mathf.Pow((float)edgeHinsh.pheromone,alpha) * Mathf.Pow((float)(1 / edgeHinsh.cost), beta))) / totalCost));

                probaEdges.Add(probs);
            }

        }

        List<float> accumulatedprobabilities = new List<float>();
        float sum = 0;
        for (int i = 0; i < probaEdges.Count; i++) // Calcul de la somme des probas
        {
            sum += probaEdges[i];
            accumulatedprobabilities.Add(sum);
        }
        //print("-------");
        int randFF = aleat.Next(100);
        int databased = (int)System.DateTime.Now.Ticks;
        System.Random randTest = new System.Random(databased + randFF);

        double rand = randTest.NextDouble(); // Pseudo aléatoire convenable
        double randPIF = aleat.NextDouble();

        if (randPIF < 0.9)
        {
            for (int i = 0; i < EdgesSelect.Count; i++)
            {
                if (accumulatedprobabilities[i] >= rand)
                {
                    if (EdgesSelect[i].to != pos) return EdgesSelect[i].to;
                    return EdgesSelect[i].from;
                }
            }
            return choice[aleat.Next(choice.Count)];
        }
        else
        {
            return choice[aleat.Next(choice.Count)];
        }
    }
}

public class fourmis
{
    public List<Node> listChemin;
    public List<int> listIndexEdgeF;
    public float longueur;
    public Node pos;
    public Node oldpos;
    public List<Node> listCheminReel;

    public fourmis(Node pose)
    {
        this.longueur = 0;
        this.pos = pose;
        this.oldpos = this.pos;
        this.listChemin = new List<Node>();
        this.listIndexEdgeF = new List<int>();
        this.listCheminReel = new List<Node>();
    }

}

