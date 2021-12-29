using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GreedyAlgorithmSimplifiedGraphGenerator : MonoBehaviour
{
    GraphGenerator graphGenerator;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        while (!graphGenerator.isGenerated)
        {
            //yield return null;
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(Simplify());
    }
    private IEnumerator Simplify()
    {
        float visionScore = 0;
        var keptNodes = new List<(int, int)>();
        var vision = new Dictionary<(int, int), bool>();
        foreach (var n in graphGenerator.graph.nodes)
        {
            vision.Add(n.Key, false);
        }

        (int, int) node;
        while (vision.Count(v => v.Value) < graphGenerator.graph.nodes.Count)
        {
            node = FindOptimumNode(keptNodes, ref vision);
        }
        print(graphGenerator.graph.nodes.Count + "->"+keptNodes.Count);
        yield return null;
    }

    private (int, int) FindOptimumNode(List<(int, int)> keptNodes, ref Dictionary<(int, int), bool> vision)
    {
        Dictionary<(int, int), Dictionary<(int, int), bool>> nodeToAssociatedVision = new Dictionary<(int, int), Dictionary<(int, int), bool>>();
        foreach (var item in graphGenerator.graph.nodes.Where(i => !keptNodes.Any(n => n == i.Key)))
        {
            var associatedVision = new Dictionary<(int, int), bool>();
            foreach (var v in vision) associatedVision.Add(v.Key, v.Value);
            foreach (var n in item.Value.neighs)
            {
                associatedVision[n.to.pos] = true;
            }
            associatedVision[item.Key] = true;
            nodeToAssociatedVision.Add(item.Key, associatedVision);
        }
        (int, int) node = nodeToAssociatedVision.OrderByDescending(ntva => ntva.Value.Count(x => x.Value)).First().Key;
        vision = nodeToAssociatedVision[node];
        keptNodes.Add(node);
        return node;
    }
}
