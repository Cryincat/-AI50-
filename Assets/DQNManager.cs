using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DQNManager : MonoBehaviour
{
    public bool learningFromScratch;
    public int nbIter;
    public string fileWeights; 

    public int nbAgent = 0;
    public GameObject agentPrefab;

    private Graph graph;
    public bool isGenerated;
    private GameObject parent;
    private LoadGraph loadGraph;
    public bool areAgentGenerated = false;

    UdpSocket udpSocket;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        loadGraph = FindObjectOfType<LoadGraph>();
        yield return new WaitUntil(() => loadGraph.isGenerated);
        graph = loadGraph.graph;
        parent = GameObject.FindGameObjectWithTag("Agents");
        udpSocket = GetComponent<UdpSocket>();

        yield return new WaitUntil(() => udpSocket.isRunning);
        string message = learningFromScratch ? "t" + nbIter + "," + fileWeights : "f" + fileWeights;
        print("Sending infos for init : \n" + message);
        udpSocket.SendData(message);

        yield return null;
        if (learningFromScratch)
        {
            message = transform.name + "\n" + (0, 0) + "\n" + loadGraph.graph.SaveAsStringWithTimes();
            print("Sending Environnement infos for training : \n" + message);
            udpSocket.SendData(message);
            yield return new WaitForSecondsRealtime(1);
            print("Loading menu scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
        else
        {
            System.Random random = new System.Random();
            for (int i = 0; i < nbAgent; i++)
            {
                GameObject agent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, parent.transform);
                agent.name = ("Agent_" + i);
                int randomNode = random.Next(0, graph.nodes.Count);
                Node startPoint = graph.nodes.Values.ToList<Node>()[randomNode];
                AgentDQN agentScript = agent.GetComponent<AgentDQN>();
                agentScript.transform.position = startPoint.realPosFromAgentHeights;
            }
            areAgentGenerated = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}