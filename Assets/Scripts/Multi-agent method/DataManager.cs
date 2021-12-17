using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using System.Linq;

public class DataManager : MonoBehaviour
{
    private Graph graph;
    private LoadGraph loadGraph;


    public string graphName = "";
    public string methodName = "";
    public float maxIdleness = -1;
    public float mediumIdleness = -1;
    public float maxIdlenessRealTime = -1;
    public float mediumIdlenessRealTime = -1;
    public int numMethod;
    public int nbAgent;

    private static string RL = "Reinforcment learning method";
    private static string ACO = "Ant Colony Optimization method";
    private static string MAM = "Multi-Agent Method";
    private float nbNodes;
    private Dictionary<int, float> dataRealTime;
    string pathForSave = Directory.GetCurrentDirectory() + "/Assets/Data/DataSimulation";

    public float simulationTime = 0;
    public int count = 0;
    public int nbIterationBeforeStop = 500;

    // Start is called before the first frame update
    IEnumerator Start()
    {

        DontDestroyOnLoad(gameObject);
        dataRealTime = new Dictionary<int, float>();
        //nbIterationBeforeStop = FindObjectOfType<LoadMethod>().nbIterationBeforeStop;
        loadGraph = FindObjectOfType<LoadGraph>();
        yield return new WaitUntil(() => loadGraph.isGenerated);
        determineMethodName(numMethod);
        graph = loadGraph.graph;

        InvokeRepeating("CheckIdleness", 1, 1);
        nbNodes = graph.nodes.Count;
    }

    private void Update()
    {
        simulationTime += Time.deltaTime;
    }

    void determineMethodName(int numMethod)
    {
        switch (numMethod)
        {
            case 0:
                methodName = RL;
                break;
            case 1:
                methodName = MAM;
                break;
            case 2:
                methodName = ACO;
                break;
        }
    }

    void CheckIdleness()
    {
        float idlenessSum = 0;
        float temp = 0;
        count++;
        foreach(Node node in graph.nodes.Values)
        {
            idlenessSum += node.timeSinceLastVisit;
            if (node.timeSinceLastVisit > maxIdleness)
            {
                maxIdleness = node.timeSinceLastVisit;
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

        if (!dataRealTime.Keys.Contains((int)simulationTime))
        {
            dataRealTime.Add((int)simulationTime, mediumIdleness);
        }
        
        if (methodName != ACO)
        {
            if (count >= nbIterationBeforeStop)
            {
                print("Cela fait " + nbIterationBeforeStop + " que la mediumIdleness = " + mediumIdleness + " n'a pas augmenté.");
                FindObjectOfType<ButtonsHUD>().speed = 0;
                SaveSimulationData(methodName, nbAgent, graphName, dataRealTime);
            }
        }
        if (methodName == MAM) EventManager.current.UpdateNewMaxIdleness(maxIdleness);
    }

    void SaveSimulationData(string methodName, int nbAgent, string graphName, Dictionary<int,float> dataRealTime)
    {
        string path = pathForSave;
        string actualDate = DateTime.Now.ToString();
        string[] actualDateSplitted = actualDate.Split(' ');
        string[] actualDateDateSplitted = actualDateSplitted[0].Split('/');
        string[] actualDateTimeSplitted = actualDateSplitted[1].Split(':');
        path += "/Simulation_" + actualDateDateSplitted[0] + "_" + actualDateDateSplitted[1] + "_" + actualDateDateSplitted[2] + "__" + actualDateTimeSplitted[0] + "-" + actualDateTimeSplitted[1] + "-" + actualDateTimeSplitted[2] + ".txt";
        List<string> dataToSave = new List<string>();

        dataToSave.Add("Simulation date : " + actualDate);
        dataToSave.Add("");
        dataToSave.Add("Method used : " + methodName);
        dataToSave.Add("Graph used : " + graphName);
        dataToSave.Add("Agent number : " + nbAgent);
        dataToSave.Add("Simulation time : " + simulationTime);
        dataToSave.Add("Maximum idleness reach : " + maxIdleness);
        dataToSave.Add("Medium idleness reach : " + mediumIdleness);
        dataToSave.Add("");
        dataToSave.Add("Medium idleness from start to end : (format -> (XX : YY) with X = time in second and Y = value of mediumIdleness at X seconds");

        foreach(int item in dataRealTime.Keys)
        {
            dataToSave.Add(item + " : " + dataRealTime[item]);
        }

        File.WriteAllLines(path, dataToSave);
        print("All data were writed in a new file at path : " + path);
    }

    public void Save()
    {
        SaveSimulationData(methodName, nbAgent, graphName, dataRealTime);
    }

}
