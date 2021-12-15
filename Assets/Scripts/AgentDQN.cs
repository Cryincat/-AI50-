﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentDQN : Agent
{
    public string agentId;
    GraphGenerator graphGenerator;
    UdpSocket udpSocket;
    public (int, int)? action;
    System.Diagnostics.Stopwatch stopwatch;
    MeshRenderer meshRenderer;

    public override IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        while (!graphGenerator.isGenerated) yield return null;

        StartCoroutine(base.Start());
        udpSocket = FindObjectOfType<UdpSocket>();

        int i = 0;
        while (true)
        {
            string name = "Agent" + i;
            if (transform.parent.Find(name) == null)
            {
                transform.name = name;
                agentId = name;
                break;
            }
            i++;
        }
        Debug.Log("Started");
        yield break;
    }
    protected override void Update()
    {
        base.Update();
        if (!thinking)
            meshRenderer.material.color = Color.white;
    }

    // Start is called before the first frame update
    protected override IEnumerator FindDestination()
    {
        if (destination == null)
        {
            Debug.Log("FindDestination : thinking is " + thinking + " in pos "+ node.pos);

            string message = transform.name + "\n" + node.pos + "\n" + graphGenerator.graph.SaveAsStringWithTimes();
            udpSocket.SendData(message);
            meshRenderer.material.color = Color.green;
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (action == null)
            {
                yield return new WaitForSeconds(0.001f);
            }

            var pos = (node.pos.Item1 + action.Value.Item1, node.pos.Item2 + action.Value.Item2);
            Debug.Log("Destination is " + pos);
            if (graphGenerator.graph.nodes.ContainsKey(pos))
            {
                destination = graphGenerator.graph.nodes[pos];
            }
            //if (_catch) yield return new WaitForSeconds(1f / base.speed);
            //else 
            thinking = false;
            action = null;
            Debug.Log("D : "+ thinking);
            yield break;
        }
    }

    internal void GiveAction(string v)
    {
        var (first, second, _) = v.Split(',');
        first = first.Remove(0, 1);
        second = second.Remove(0, 1);
        second = second.Remove(second.Length - 1, 1);

        action = (int.Parse(first), int.Parse(second));
        Debug.Log("Action is " + action);

        print("Time thinking is : " + stopwatch.ElapsedMilliseconds + " ms");
        stopwatch.Restart();
    }
}