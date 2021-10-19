using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class Agent : MonoBehaviour
{
    private Vector3 oldPos;
    GraphGenerator graphGenerator;
    public Node node;
    public IEnumerator Start()
    {
        oldPos = transform.position;
        graphGenerator = FindObjectOfType<GraphGenerator>();
        while (!graphGenerator.isGenerated)
        {
            yield return null;
            try
            {
                node = graphGenerator.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    private void Update()
    {
        try
        {
            if (oldPos != transform.position)
            {
                oldPos = transform.position;
                node = graphGenerator.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
                print(node._name + "changing node");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}