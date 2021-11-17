using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private Graph graph;
    private GraphGenerator graphGenerator;

    public float maxIdlenessVisited = -1;
    public float maxIdlenessTheoric = -1;
    public float mediumIdleness = -1;

    private float nbNodes;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        EventManager.current.onNewNodeVisited += onNewNodeVisited;
        graphGenerator = FindObjectOfType<GraphGenerator>();
        yield return new WaitUntil(() => graphGenerator.isGenerated);
        graph = graphGenerator.graph;
        InvokeRepeating("CheckMaxIdleness", 1, 1);
        nbNodes = graph.nodes.Count;
    }

    void onNewNodeVisited(float value)
    {
        if (value > maxIdlenessVisited)
        {
            maxIdlenessVisited = value;
        }
        EventManager.current.UpdateNewMaxIdleness(maxIdlenessVisited);
    }

    void CheckMaxIdleness()
    {
        float idlenessSum = 0;
        foreach(Node node in graph.nodes.Values)
        {
            idlenessSum += node.timeSinceLastVisit;
            if (node.timeSinceLastVisit > maxIdlenessTheoric)
            {
                
                maxIdlenessTheoric = node.timeSinceLastVisit;
            }
        }
        if (mediumIdleness < (idlenessSum / nbNodes))
        {
            mediumIdleness = (idlenessSum / nbNodes);
        }
       
    }


}
