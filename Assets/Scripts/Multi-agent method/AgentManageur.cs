using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class AgentManageur : MonoBehaviour
{
    // Correspond à la valeur seuil de l'oisiveté au delà de laquelle on remarque le noeud comme a visité d'urgence
    public float threshold = 60;

    // Correspond respectivement au délai de lancement (en seconde) et au taux de répétition (en seconde) de la méthode de check du graphe.
    public float repeatRate = 1;
    public float delay = 1;
    public int nbAgent = 3;

    private int maxPriority = 8;


    string path = Directory.GetCurrentDirectory() + "/Assets/Data/ShortestPathData/";
    public bool isGenerated = false;
    public bool isGraphLoading = false;
    public GameObject parent;
    public GameObject agentPrefab;

    private Graph graph;
    private GraphGenerator graphGenerator;
    private LoadGraph loadGraph;

    private Dictionary<Node, bool> managerTool;
    private Dictionary<Node, int> nodePriority;

    public bool areAgentGenerated = false;

    IEnumerator Start()
    {
        // Liaison entre les évenements et leurs méthodes
        EventManager.current.onSettingNodeToTrue += OnSetNodeToTrue;
        EventManager.current.onSettingNodeToFalse += OnSetNodeToFalse;
        EventManager.current.onUpdateNewMaxIdleness += OnUpdateMaxIdleness;
        EventManager.current.onRemoveNodeFromNodeAssignation += OnSettingNodePriorityToOne;

        // Récupération des instance d'autres scripts
        if (isGraphLoading)
        {
            loadGraph = FindObjectOfType<LoadGraph>();
        }
        else
        {
            graphGenerator = FindObjectOfType<GraphGenerator>();
        }
        

        // Initialisation des variables
        managerTool = new Dictionary<Node, bool>();
        nodePriority = new Dictionary<Node, int>();

        // Attente de la génération du script
        if (isGraphLoading)
        {
            yield return new WaitUntil(() => loadGraph.isGenerated);
            graph = loadGraph.graph;
        }
        else
        {
            yield return new WaitUntil(() => graphGenerator.isGenerated);
            graph = graphGenerator.graph;
        }

        System.Random random = new System.Random();
        for (int i = 0; i < nbAgent; i++)
        {
            GameObject agent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, parent.transform);
            agent.name = ("Agent_" + i);
            int randomNode = random.Next(0, graph.nodes.Count);
            Node startPoint = graph.nodes.Values.ToList<Node>()[randomNode];
            AgentPatrouilleur agentScript = agent.GetComponent<AgentPatrouilleur>();
            agentScript.startPoint = startPoint;
        }
        areAgentGenerated = true;

        // Méthode d'initialisation
        LoadManagerTool();
        LoadNodePriority();
        LoadPaths(path);


        // Génération = OK
        
        yield return new WaitUntil(() => isGenerated);
        print("| Manageur | Génération des variables terminée...");


        // Lancement à interval régulier de la méthode de check 
        InvokeRepeating("CheckGraphLeveling", delay, repeatRate);

    }

    void LoadNodePriority()
    {
        foreach (Node node in graph.nodes.Values)
        {
            nodePriority.Add(node, 1);
        }
    }

    // Méthode permettant de set l'ensemble des nodes dans managerTool à false
    void LoadManagerTool()
    {
        //print("| Manageur | Initialisation 'managerTool'...");
        foreach (Node node in graph.nodes.Values)
        {
            managerTool.Add(node, false);
        }
        print("| Manageur | 'managerTool' initialisé.");
    }

    IEnumerator LoadPathFromFile(List<string> content)
    {
        print("Loading from existing file !!");
        int count = 0;
        content.RemoveAt(0);
        Dictionary<(Node, Node), List<Node>> shortestPathData = new Dictionary<(Node, Node), List<Node>>();
        // Forme du fichier text : 
        // X1,Y1|X2,Y2|x1,y1;x2,y2;... (NODE|NODE|LIST<NODE>)
        foreach (string line in content)
        {
            count++;
            if (count % 1000 == 0)
            {
                print("| Manageur | " + count + " paths loaded.");
                yield return null;
            }
            string[] lineSplitted = line.Split('|');
            string[] node1Str = lineSplitted[0].Split(',');
            string[] node2Str = lineSplitted[1].Split(',');
            string[] listStr = lineSplitted[2].Split(';');
            Node node1 = graph.nodes[(int.Parse(node1Str[0]), int.Parse(node1Str[1]))];
            Node node2 = graph.nodes[(int.Parse(node2Str[0]), int.Parse(node2Str[1]))];
            List<Node> shortestPath = new List<Node>();
            foreach(string nodeStr in listStr)
            {
                if (nodeStr != "")
                {
                    string[] nodeStrSplitted = nodeStr.Split(',');
                    shortestPath.Add(graph.nodes[(int.Parse(nodeStrSplitted[0]), int.Parse(nodeStrSplitted[1]))]);
                }
                
            }
            shortestPathData.Add((node1, node2), shortestPath);
        }
        print("| Manageur | " + count + " paths loaded, this is the end.");
        EventManager.current.SendShortestPathData(shortestPathData);
        isGenerated = true;

    }

    void LoadPaths(string path)
    {
        print("LoadPath(path) path : " + path);
        string graphHash = getHashGraph();
        if (isAnyFileInDirectory(path))
        {
            List<string> content = GetAnyFileCorrespondingInDirectory(path, graphHash);
            if (content != null)
            {
                StartCoroutine(LoadPathFromFile(content));
                return;
            }
        }
        StartCoroutine(LoadPaths());
        return;
    }

    bool isAnyFileInDirectory(string path)
    {
        var directory = new DirectoryInfo(path);
        List<FileInfo> files = directory.GetFiles().ToList<FileInfo>();
        print("how much file in directory  :" + files.Count);
        if (files.Count == 0)
        {
            return false;
        }
        return true;
    }

    List<string> GetAnyFileCorrespondingInDirectory(string path, string graphHash)
    {
        var directory = new DirectoryInfo(path);
        List<FileInfo> files = directory.GetFiles().ToList<FileInfo>();
        foreach(FileInfo file in files)
        {
           
            string path2 = path + "/" +  file.Name;
            print(path2);
            List<string> content = File.ReadAllLines(path2).ToList<string>();
            if (content[0] == graphHash)
            {
                return content;
            }
        }
        return null;
    }

    string getHashGraph()
    {
        string hash = "";

        foreach(Node node in graph.nodes.Values)
        {
            hash += node.pos.Item1 + "," +  node.pos.Item2 + ";";
        }
        print("hash : " + hash);
        return hash;
    }

    // Méthode permettant de charger préalablement tous les paths existants entre 2 nodes dans le graphe.
    IEnumerator LoadPaths()
    {
        print("| Manageur | Itinialisation des données des shortestPaths...");
        AStar aStar = FindObjectOfType<AStar>();
        int count = 0;
        int nbNode = graph.nodes.Values.Count;
        int totalPath = nbNode * nbNode;
        Dictionary<(Node, Node), List<Node>> shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        foreach (Node start in graph.nodes.Values)
        {
            foreach (Node end in graph.nodes.Values)
            {
                count++;
                if (count % 250 == 0)
                {
                    print("| Manageur | Path " + count + "/" + totalPath);
                    yield return null;
                }
                shortestPathData.Add((start, end), aStar.GetShortestPathAstar(start, end, graph));

            }
        }
        print("| Manageur | Path " + count + "/" + totalPath);
        EventManager.current.SendShortestPathData(shortestPathData);
        Save(shortestPathData, path);
        print("| Manageur | ShortestPaths initialisés.");
    }

    void Save(Dictionary<(Node,Node),List<Node>> data, string path)
    {
        List<string> dataToSave = new List<string>();
        path += "data_" + getFileCount(path) + ".txt";

        // Insert hash to first
        dataToSave.Insert(0, getHashGraph());

        // Insert all data
        foreach((Node,Node) couple in data.Keys)
        {
            // Saving (Node,Node) as '(X1,Y1)|(X2,Y2);'
            string lineToSave = couple.Item1.pos.Item1 + "," + couple.Item1.pos.Item2 + "|" + couple.Item2.pos.Item1 + "," + couple.Item2.pos.Item2 + "|";

            // Saving list corresponding to (Node,Node)
            lineToSave += ToString(data[couple]);
            dataToSave.Add(lineToSave);
        }
        File.WriteAllLines(path, dataToSave);
        isGenerated = true;
    }

    int getFileCount(string path)
    {
        var directory = new DirectoryInfo(path);
        List<FileInfo> files = directory.GetFiles().ToList<FileInfo>();
        return files.Count;
    }

    string ToString(List<Node> data)
    {
        string temp = "";
        foreach(Node node in data)
        {
            temp += node.pos.Item1 + "," + node.pos.Item2 + ";";
        }
        return temp;
    }

    void CheckGraphLeveling()
    {
            int priority = -1;
            foreach (Node node in graph.nodes.Values)
            {
                if (managerTool[node] == false || (managerTool[node] == true && nodePriority[node] < maxPriority))
                {
                    float x = node.timeSinceLastVisit;
                    // 0% <= x < 15%
                    if (x < 0.15 * threshold)
                    {
                        priority = 1;
                    }
                    // 15% <= x < 30%
                    if (x >= 0.15 * threshold && x < 0.30 * threshold)
                    {
                        priority = 2;
                    }
                    // 30% <= x < 50%
                    if (x >= 0.30 * threshold && x < 0.50 * threshold)
                    {
                        priority = 3;
                    }
                    // 50% <= x < 65%
                    if (x >= 0.50 * threshold && x < 0.65 * threshold)
                    {
                        priority = 4;
                    }
                    // 65% <= x < 80%
                    if (x >= 0.65 * threshold && x < 0.80 * threshold)
                    {
                        priority = 5;
                    }
                    // 80% <= x < 90%
                    if (x >= 0.80 * threshold && x < 0.90 * threshold)
                    {
                        priority = 6;
                    }
                    // 90% <= x
                    if (x >= 0.90 * threshold)
                    {
                        priority = 7;
                    }

                    if (priority == -1)
                    {
                        throw new Exception("La priorité du node est toujours à -1, alors qu'elle devrait être égale à 1..maxPriority.");
                    }

                    // Updating new priority
                    nodePriority[node] = priority;

                    if (priority > 1)
                    {
                        // Si le node est en cours de visite mais que sa priorité doit être revu à la hausse, on le fait quand même passé dans l'évent pour prévenir le gestionnaire.
                        if (managerTool[node] == true)
                        {
                            if (nodePriority[node] < priority)
                            {
                                //if (hasNeighbourWithBetterPriorityAlreadySpotted(node, priority) == false)
                                //{
                                EventManager.current.HasToBeVisited(node);
                                //}
                            }
                        }
                        else
                        {
                            //if (hasNeighbourWithBetterPriorityAlreadySpotted(node, priority) == false)
                            //{
                            OnSetNodeToTrue(node);
                            EventManager.current.HasToBeVisited(node);
                            //}
                        }

                    }
                }

            }
            // Sending new priority to gestionnaire and agents
            EventManager.current.UpdateNodePriority(nodePriority);
    }

    // Méthode vérifiant si un des voisins du noeud 'node' n'est pas déjà prévu d'être visité, et si oui vérifiant qu'il est bien de priorité égale ou supérieure.
    // Si oui, cela signifie que le noeud 'node' va être visité également, et donc il n'est pas nécessaire de l'ajouter à la liste des neouds à visiter.
    bool hasNeighbourWithBetterPriorityAlreadySpotted(Node node, int priority)
    {
        // Pour chaque voisin du node, on vérifie qu'il n'existe pas un voisin déjà prévu d'être visité avec un niveau de priorité similaire ou supérieur au node à check.
        foreach (Edge edge in node.neighs)
        {
            if (priority <= nodePriority[edge.to])
            {
                return true;
            }
        }
        return false;
    }

    // Méthode permettant de set to true un node qui est en cours de visite, ainsi que tous ses voisins.
    public void OnSetNodeToTrue(Node node)
    {
        managerTool[node] = true;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = true;
        }
    }

    // Méthode permettant de set to false un node qui n'est plus en cours de visite; ainsi que tous ses voisins.
    public void OnSetNodeToFalse(Node node)
    {
        managerTool[node] = false;
        foreach (Edge edge in node.neighs)
        {
            managerTool[edge.to] = false;
        }
    }

    // Méthode lié à l'évent d'update de l'idleness max, detecté dans le script DataManager.
    void OnUpdateMaxIdleness(float value)
    {
        threshold = value;
    }

    // Méthode lié à l'évent de passage sur un node. Le node a été visité, donc il n'est plus prioritaire.
    void OnSettingNodePriorityToOne(Node node)
    {
        nodePriority[node] = 1;
    }

}
