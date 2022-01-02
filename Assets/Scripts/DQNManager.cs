using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DQNManager : MonoBehaviour
{
    public bool learningFromScratch;
    public int nbIter;
    public string fileWeights;

    public int nbAgent = 1;
    public GameObject agentPrefab;

    private Graph graph;
    private GameObject parent;
    private LoadGraph loadGraph;
    public bool areAgentGenerated = false;

    public GameObject prefabLoadGraph;

    List<string> messagesToSend;
    private bool firstMessageTosSendDone;
    UdpSocket udpSocket;
    internal bool isReady;

    public string graphName = "";
    public int nbIterationBeforeStop = 0;
    public int numMethod = 0;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        numMethod = (int)levelLoader.dataScene[0];
        nbAgent = (int)levelLoader.dataScene[1];
        graphName = levelLoader.dataScene[2] as string;
        nbIterationBeforeStop = (int)levelLoader.dataScene[3];

        messagesToSend = new List<string>();
        GameObject loadGraph = Instantiate(prefabLoadGraph);
        loadGraph.name = "LoadGraph";
        loadGraph.GetComponent<LoadGraph>().textFileName = graphName;
        yield return new WaitUntil(() => loadGraph.GetComponent<LoadGraph>().isGenerated);

        learningFromScratch = levelLoader.dataScene[4] is int;
        if (!learningFromScratch) fileWeights = (string)levelLoader.dataScene[4];
        else nbIter = (int)levelLoader.dataScene[4];

        DataManager dataManager = FindObjectOfType<DataManager>();
        dataManager.nbAgent = nbAgent;
        dataManager.nbIterationBeforeStop = nbIterationBeforeStop;
        dataManager.numMethod = numMethod;
        dataManager.graphName = graphName;
        

        graph = loadGraph.GetComponent<LoadGraph>().graph;
        parent = GameObject.FindGameObjectWithTag("Agents");
        StartCoroutine(CheckForMessageToSend());
        udpSocket = GetComponent<UdpSocket>();

        yield return new WaitUntil(() => udpSocket.isRunning);

        if (learningFromScratch) fileWeights = Directory.GetCurrentDirectory() + "/Assets/Data/Weights/" + graphName.Split('\\').ToList().Last() +
                 "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

        string message = learningFromScratch ? "t," + nbAgent + "," + nbIter + "," + fileWeights : "f," + nbAgent + "," + fileWeights;
        print("Sending infos for init : \n" + message);
        udpSocket.SendData(message);
        yield return new WaitForSecondsRealtime(0.1f);

        yield return null;
        if (learningFromScratch)
        {
            var agents = "";
            for (int i = 0; i < nbAgent; i++)
                agents += "(0,0);";
            message = transform.name + "\n" +  "(0,0)" + "\n" + agents + "\n" + loadGraph.GetComponent<LoadGraph>().graph.SaveAsStringWithTimes();
            print("Sending Environnement infos for training : \n" + message);
            udpSocket.SendData(message);
            yield return new WaitForSecondsRealtime(1);
            print("Loading menu scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
        else
        {
            System.Random random = new System.Random();
            GameObject agents = new GameObject("Agents");

            for (int i = 0; i < nbAgent; i++)
            {
                GameObject agent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, agents.transform);
                agent.name = ("Agent_" + i);
                int randomNode = random.Next(0, graph.nodes.Count);
                Node startPoint = graph.nodes.Values.ToList<Node>()[randomNode];
                AgentDQN agentScript = agent.GetComponent<AgentDQN>();
                agentScript.transform.position = startPoint.realPosFromAgentHeights;
                //if (i == 0) yield return new WaitForSeconds(2);
            }
            areAgentGenerated = true;
        }
        isReady = true;
    }

    internal void SendData(string message)
    {
        messagesToSend.Add(message);

    }

    IEnumerator CheckForMessageToSend()
    {
        while (true)
        {
            if (messagesToSend.Count > 0)
            {
                string message = messagesToSend.First();
                udpSocket.SendData(message);
                Debug.Log(message.Substring(0, 20));
                messagesToSend.RemoveAt(0);
                if(!firstMessageTosSendDone) 
                {
                    firstMessageTosSendDone = true;
                    yield return new WaitForSeconds(2);
                }
                yield return new WaitForSeconds(1f / nbAgent);
            }
            else yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
