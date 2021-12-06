using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMethod : MonoBehaviour
{

    public int nbAgent = 0;
    public string graphFile = "";
    public int nbIterationBeforeStop = 0;

    public bool isReady = false;

    public GameObject prefabEventManager;
    public GameObject prefabSols;
    public GameObject prefabAgents;
    public GameObject prefabLoadGraph;

    // Start is called before the first frame update
    void Start()
    {
        LaunchMAM();
    }

    void LaunchMAM()
    {
        // Génération du GameObject loadGraph pour générer le graph.
        GameObject loadGraph = Instantiate(prefabLoadGraph);
        loadGraph.name = "LoadGraph";
        loadGraph.GetComponent<LoadGraph>().textFileName = graphFile;

        // Génération du GameObject Sols (conteneur des gameobjects de terrains, et des scripts pour la méthode)
        GameObject sols = Instantiate(prefabSols);
        sols.GetComponent<AgentManageur>().nbAgent = nbAgent;
        sols.GetComponent<DataManager>().nbIterationBeforeStop = nbIterationBeforeStop;
        sols.GetComponent<DataManager>().graphName = graphFile;
        sols.GetComponent<DataManager>().methodName = "Multi-Agent Method";
        sols.name = "Sols";

        // Génération du GameObject Agents (conteneur pour les instances d'agents)
        GameObject agents = Instantiate(prefabAgents);
        agents.name = "Agents";

        // Génération du GameObject EventManager
        GameObject eventManager = Instantiate(prefabEventManager);
        eventManager.name = "EventManager";

        isReady = true;
    }

}
