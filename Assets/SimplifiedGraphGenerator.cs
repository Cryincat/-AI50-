using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class SimplifiedGraphGenerator : MonoBehaviour
{
    GraphGenerator graphGenerator;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        while (!graphGenerator.isGenerated)
        {
            //yield return null;
            yield return new WaitForSeconds(1);
        }
        StartCoroutine(Simplify());
    }

    private IEnumerator Simplify()
    {
        System.Random random = new System.Random();
        Population population = new Population(100, graphGenerator.graph, random);
        print(population.FitnessMax());

        for(int i = 0; i< 100; i++)
        {
            Profiler.BeginSample("Selection");
            population.Selection();
            Profiler.EndSample();

            Profiler.BeginSample("Crossover");
            population.Crossover();
            Profiler.EndSample();

            Profiler.BeginSample("Mutation");
            population.Mutation(1/graphGenerator.graph.nodes.Count, random);
            Profiler.EndSample();

            print(population.FitnessMax());
            var max = population.members.First(m => m.fitness == population.FitnessMax()).genes;
            foreach (var g in max.Where(x=>x.Value==true))
            {
                print(g.Key + " -> " + true);
            }
            print("------");
            GC.Collect();
            yield return null;
        }
        yield return null;
        Debug.Break();
    }
    public class DNA
    {
        Graph baseGraph;
        int size;
        public float fitness;
        public Dictionary<(int, int), bool> genes;

        public DNA(Graph baseGraph, System.Random random, Dictionary<(int, int), bool> genes = null)
        {
            this.baseGraph = baseGraph;
            this.size = baseGraph.nodes.Count;
            if (genes != null)
                this.genes = genes;
            else
            {
                this.genes = new Dictionary<(int, int), bool>();
                for (int i = 0; i < size; i++)
                {
                    //this.genes.Add(baseGraph.nodes.Keys.ToList()[i], random.NextDouble() < 0.5);//random.choice(string.printable))
                    this.genes.Add(baseGraph.nodes.Keys.ToList()[i], false);
                }
            }
            CalculateFitness();
        }
        public void CalculateFitness()
        {
            Profiler.BeginSample("Init vision");
            var vision = new Dictionary<(int, int), bool>();
            foreach (var kvp in genes)
            {
                vision.Add(kvp.Key, false);
            }
            Profiler.EndSample();


            foreach (var kvp in genes.Where(x=>x.Value))
            {
                if (kvp.Value)
                {
                    var pos = kvp.Key;
                    Profiler.BeginSample("neighs");
                    var neighs = new (int, int)[]{
                        (pos.Item1 - 1, pos.Item2 - 1), (pos.Item1 - 1, pos.Item2), (pos.Item1 - 1, pos.Item2 + 1),
                        (pos.Item1, pos.Item2 - 1), (pos.Item1, pos.Item2), (pos.Item1, pos.Item2+1),
                        (pos.Item1 +1, pos.Item2 - 1), (pos.Item1 + 1, pos.Item2), (pos.Item1 + 1, pos.Item2 + 1)};
                    Profiler.EndSample();

                    Profiler.BeginSample("for each neigh, vision = true");
                    foreach (var n in neighs)
                    {
                        if (vision.ContainsKey(n) && !vision[n])
                        {
                            vision[n] = true;
                        }
                    }
                    Profiler.EndSample();
                }
            }

            Profiler.BeginSample("Calculation vision score");
            float visionScore = 0;
            foreach (var kvp in vision)
            {
                if (kvp.Value) visionScore++;
            }
            visionScore = (visionScore-0.5f*vision.Values.Count(x=>x==true)) / vision.Count/* * genes.Values.Count(x=>x)*/;
            fitness = visionScore;
            Profiler.EndSample();

        }
    }
    public class Population
    {
        public List<DNA> members;
        List<DNA> matingPool;
        System.Random random;
        Graph baseGraph;

        public Population(int size, Graph baseGraph, System.Random random)
        {
            this.baseGraph = baseGraph;
            members = new List<DNA>(size);
            this.random = random;
            for (int i = 0; i < size; i++)
            {
                members.Add(new DNA(baseGraph, random));
            }
        }
        public void Selection()
        {
            matingPool = new List<DNA>();
            for (int i = 0; i < members.Count; i++)
            {
                matingPool.Add(SelectionGetIndividual());
            }
        }
        public DNA SelectionGetIndividual()
        {
            for (int i = 0; i < members.Count; i++)
            {
                members[i].fitness = Mathf.Pow(members[i].fitness,10000);
            }
            var sum = members.Sum(x => x.fitness);
            for (int i = 0; i < members.Count; i++)
            {
                if (sum != 0) members[i].fitness /= sum;
            }
            List<float> accumulatedprobabilities = new List<float>();
            sum = 0;
            for (int i = 0; i < members.Count; i++)
            {
                sum += members[i].fitness;
                accumulatedprobabilities.Add(sum);
            }
            float r = (float)random.NextDouble();
            for (int i = 0; i < members.Count; i++)
            {
                if (accumulatedprobabilities[i] >= r) return members[i];
            }
            //Si la somme des fitness est 0 (improbable)
            return members[random.Next(members.Count)];
        }
        public void Crossover()
        {
            for (int i = 0; i < members.Count; i++)
            {
                var parent1 = members[random.Next(members.Count)];
                var parent2 = members[random.Next(members.Count)];
                var r = random.Next(parent1.genes.Count);
                var newChildGenes = new Dictionary<(int, int), bool>();
                for (int j = 0; j < r; j++) {
                    var kvp = parent1.genes.ElementAt(j);
                    newChildGenes.Add(kvp.Key, kvp.Value);
                }
                for (int j = r; j < parent1.genes.Count; j++)
                {
                    var kvp = parent2.genes.ElementAt(j);
                    newChildGenes.Add(kvp.Key, kvp.Value);
                }
                members[i] = new DNA(baseGraph, random, newChildGenes);
            }
        }
        public void Mutation(float mutationRate, System.Random random)
        {
            foreach (var m in members)
            {
                for(int i = 0; i<m.genes.Count; i++)
                {
                        if (random.NextDouble() > mutationRate)
                            m.genes[m.genes.ElementAt(i).Key] = !m.genes[m.genes.ElementAt(i).Key];
                    Profiler.BeginSample("CalculateFitness");
                    m.CalculateFitness();
                    Profiler.EndSample();
                }
                
            }
        }
        public float FitnessSum()
        {
            return members.Sum(x => x.fitness);
        }
        public float FitnessMax()
        {
            return members.Max(x => x.fitness);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
