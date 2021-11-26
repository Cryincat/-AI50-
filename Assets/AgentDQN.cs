using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentDQN : Agent
{
    ServerClient serverClient;
    bool finding_destination;
    public override IEnumerator Start()
    {
        base.Start();
        serverClient = FindObjectOfType<ServerClient>();
        yield return null;
    }
    // Start is called before the first frame update
    protected override IEnumerator FindDestination()
    {
        if(base.destination == null && finding_destination == false)
        {
            bool _catch = false;
            (int, int) action = serverClient.AskAction(base.node.pos);
            try
            {
                destination = FindObjectOfType<GraphGenerator>().graph.nodes[(base.node.pos.Item1 + action.Item1, base.node.pos.Item2 + action.Item2)];
            } catch
            {
                _catch = true;
            }
            if (_catch) yield return new WaitForSeconds(base.speed);
            else yield return null;
        }
    }
}
