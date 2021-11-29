using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;
    public bool isGraphLoading = false;

    public float maxIdlenessVisited = -1;
    public float maxIdlenessTheoric = -1;
    public float mediumIdleness = -1;
    public float maxIdlenessRealTime = -1;
    public float mediumIdlenessRealTime = -1;

    private float nbNodes;

    public float simulationTime = 0;
    public int count = 0;
    public int nbIterationBeforeStop = 0;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (isGraphLoading)
        {
            loadGraph = FindObjectOfType<LoadGraph>();
            yield return new WaitUntil(() => loadGraph.isGenerated);
            graph = loadGraph.graph;
        }
        else
        {
            graphGenerator = FindObjectOfType<GraphGenerator>();
            yield return new WaitUntil(() => graphGenerator.isGenerated);
            graph = graphGenerator.graph;
        }

        InvokeRepeating("CheckIdleness", 1, 1);
        nbNodes = graph.nodes.Count;
    }

    private void Update()
    {
        simulationTime += Time.deltaTime;
    }

    void CheckIdleness()
    {
        float idlenessSum = 0;
        float temp = 0;
        count++;
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
            count = 0;
        }
        
        if(count >= nbIterationBeforeStop)
        {
            print("Cela fait " + nbIterationBeforeStop + " que la mediumIdleness = " + mediumIdleness + " n'a pas augmenté.");
            FindObjectOfType<TimeManager>().delta = 0;
            Application.Quit();
        }

        EventManager.current.UpdateNewMaxIdleness(maxIdlenessTheoric);
    }


}
