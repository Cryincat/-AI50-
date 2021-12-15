using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour
{

    public GameObject loadingScreen;
    public Slider slider;
    public ToggleGroup algos;
    public TMP_InputField nbAgent;
    public TMP_InputField fileMap;
    public ToggleGroup numImage;
    public List<object> dataScene;

    private float tmpNbAgent;
    private string sceneParam;
    private string map1 = "graph_1.txt";
    private string map2 = "graph_2.txt";
    private string map3 = "graph_3.txt";
    private string map4 = "graph_4.txt";
    private string map5 = "graph_5.txt";
    private string map6 = "path6";
    private string map7 = "path7";
    private string map8 = "path8";
    private ArrayList maps;

    public void LoadLevel()
    {
        maps = new ArrayList();
        maps.Add(map1);
        maps.Add(map2);
        maps.Add(map3);
        maps.Add(map4);
        maps.Add(map5);
        maps.Add(map6);
        maps.Add(map7);
        maps.Add(map8);
        dataScene = getData();
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(sceneParam);
        //StartCoroutine(LoadAsynchronously(scene));
    }

    IEnumerator LoadAsynchronously(string scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            yield return null;
        }
    }

    List<object> getData()
    {
        List<object> tmpListData = new List<object>();

        // Get selected algorithm
        int numAlgo = 0;
        for(int i = 0; i < 3; i++)
        {
            if (algos.GetComponentsInChildren<Toggle>()[i].isOn)
            {
                numAlgo = i;
            }
        }
        switch (numAlgo)
        {
            case 0:
                sceneParam = "ParamQLearning";
                break;
            case 1:
                sceneParam = "ParamMultiAgent";
                break;
            case 2:
                sceneParam = "ParamACO";
                break;
        }
        // Get number of agents
        int nbAgents = 1;
        nbAgents = int.Parse(nbAgent.text);

        // Get file's path of the map
        string filePath;
        filePath = fileMap.textComponent.text;
        if(filePath == "")
        {
            for(int i = 0; i < maps.Count; i++)
            {
                if (numImage.GetComponentsInChildren<Toggle>()[i].isOn)
                {
                    filePath = maps[i] as string;
                }
            }
        }

        // Add data in the list
        tmpListData.Add(numAlgo);
        tmpListData.Add(nbAgents);
        tmpListData.Add(filePath);
        tmpListData.Add(500);
        return tmpListData;
    }

    public void Update()
    {
        if(nbAgent.text != "" && nbAgent.text != "-")
        {
            tmpNbAgent = Mathf.Clamp(float.Parse(nbAgent.text), 1, 15);
            nbAgent.text = tmpNbAgent.ToString();
        }
    }
}