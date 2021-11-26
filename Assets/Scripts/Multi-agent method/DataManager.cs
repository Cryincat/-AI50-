using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;

    public float maxIdlenessVisited = -1;
    public float maxIdlenessTheoric = -1;
    public float mediumIdleness = -1;
    public float maxIdlenessRealTime = -1;
    public float mediumIdlenessRealTime = -1;

    private float nbNodes;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        graphGenerator = FindObjectOfType<GraphGenerator>();
        yield return new WaitUntil(() => graphGenerator.isGenerated);
        graph = graphGenerator.graph;

        loadGraph = FindObjectOfType<LoadGraph>();
        //yield return new WaitUntil(() => loadGraph.isGenerated);
       // graph = loadGraph.graph;




        InvokeRepeating("CheckMaxIdleness", 1, 1);
        nbNodes = graph.nodes.Count;
    }

    void CheckMaxIdleness()
    {
        float idlenessSum = 0;
        float temp = 0;

        foreach(Node node in graph.nodes.Values)
        {
            idlenessSum += node.timeSinceLastVisit;
            if (node.timeSinceLastVisit > maxIdlenessTheoric)
            {
                maxIdlenessTheoric = node.timeSinceLastVisit;
            }
            if (node.timeSinceLastVisit > temp)
            {
                temp = node.timeSinceLastVisit;
            }
        }

        maxIdlenessRealTime = temp;
        mediumIdlenessRealTime = idlenessSum / nbNodes;
        if (mediumIdleness < (idlenessSum / nbNodes))
        {
            mediumIdleness = (idlenessSum / nbNodes);
        }

        EventManager.current.UpdateNewMaxIdleness(maxIdlenessTheoric);
    }


}
