using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MDP
{
    public static State Transition(State s, Action a)
    {
        var newState = new State(s.graph, s.positionOfAgent);
        var offset = (0, 0);
        if (new[] { Action.up, Action.upLeft, Action.upRight }.Contains(a))
        {
            offset.Item2 = 1;
        }
        if (new[] { Action.down, Action.downLeft, Action.downRight }.Contains(a))
        {
            offset.Item2 = -1;
        }
        if (new[] { Action.left, Action.upLeft, Action.downLeft }.Contains(a))
        {
            offset.Item1 = -1;
        }
        if (new[] { Action.right, Action.upRight, Action.downRight }.Contains(a))
        {
            offset.Item1 = 1;
        }
        foreach (var n in newState.graph.nodes.Values)
        {
            n.timeSinceLastVisit += 1;
        }
        var ancientNode = newState.graph.nodes[newState.positionOfAgent];
        ancientNode.agentPresence = false;
        var newPosAgent = (newState.positionOfAgent.Item1 + offset.Item1, newState.positionOfAgent.Item1 + offset.Item2);
        var newNode = ancientNode;
        if (newState.graph.nodes.ContainsKey(newPosAgent))
            newNode = newState.graph.nodes[newPosAgent];
        newNode.agentPresence = true;
        newNode.timeSinceLastVisit = 0;
        newState.positionOfAgent = newPosAgent;
        return newState;
    }
    public class State
    {
        public Graph graph;
        public (int, int) positionOfAgent;

        public State(Graph graph, (int, int) positionOfAgent)
        {
            this.graph = new Graph(graph);
            this.positionOfAgent = positionOfAgent;
        }
    }

    public enum Action
    {
        up,
        down,
        left,
        right,
        upLeft,
        upRight,
        downLeft,
        downRight
    }
}
