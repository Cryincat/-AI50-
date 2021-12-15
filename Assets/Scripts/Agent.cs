﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class Agent : MonoBehaviour
{
    public bool thinking;
    public float speed = 1;
    protected Vector3 oldPos;
    LoadGraph loadGraph;
    [NonSerialized] protected Node node;
    [NonSerialized] protected Node destination;
    bool isGenerated;

    public virtual IEnumerator Start()
    {
        if(node != null) transform.position = node.realPosFromAgentHeights;
        loadGraph = FindObjectOfType<LoadGraph>();
        destination = null;
        while (!loadGraph.isGenerated)
            yield return null;
        node = loadGraph.graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        transform.position = node.realPosFromAgentHeights;
        node.WarnAgentVisit();
        oldPos = transform.position;
        isGenerated = true;

        foreach (MDP.Action action in Enum.GetValues(typeof(MDP.Action)))
        {
            var t = MDP.Transition(new MDP.State(loadGraph.graph, node.pos), action);
        }
    }
    virtual protected void Update()
    {
        if (destination != null && destination.realPos != null)
        {
            Debug.Log(destination.pos);
        }
        if (destination != null)
        {
            GoToDestination();
        }
        else if (node != null && isGenerated && !thinking)
        {
            thinking = true;
            StartCoroutine(FindDestination());
        }

    }
    virtual protected IEnumerator FindDestination()
    {
        //TEST
        if (destination == null)
        {
            var choice = 2;
            System.Random random = new System.Random();
            switch (choice)
            {
                case 0:
                    destination = node.neighs[random.Next(node.neighs.Count)].to;
                    break;
                case 1:
                    destination = (node.neighs.OrderByDescending(x => x.to.neighs.Sum(y => y.to.timeSinceLastVisit))).First().to;
                    break;
                case 2:
                    destination = (node.neighs.OrderByDescending(x => x.to.neighs.Max(y => y.to.timeSinceLastVisit))).First().to;
                    break;
            }
        }
        yield return null;
    }
    private void GoToDestination()
    {
        speed = Mathf.Abs(speed);
        if (transform.position != oldPos)
        {
            throw new Exception("Error : the transform position was modified by another script");
        }
        float movement = speed * Time.deltaTime;
        GoToDestination(movement);
        oldPos = transform.position;
    }
    private void GoToDestination(float movementLeft)
    {
        if (destination != null)
        {
            Vector3 moveToward = Vector3.MoveTowards(transform.position, destination.realPosFromAgentHeights, movementLeft);
            movementLeft -= Vector3.Distance(transform.position, moveToward);
            transform.position = moveToward;
            if (Vector3.Distance(moveToward, destination.realPosFromAgentHeights) < 0.01)
            {
                node.agentPresence = false;
                node = destination;
                node.WarnAgentVisit();
                destination = null;
                Debug.Log("Dest" + destination == null);
                //thinking = false;
                //StartCoroutine(FindDestination());
                //GoToDestination(movementLeft);
            }
        }
    }
}