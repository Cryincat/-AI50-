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
    public TMP_InputField nbIterations;
    public TMP_InputField fileMap;
    public ToggleGroup numImage;
    public List<object> dataScene;

    private float tmpNbAgent;
    private string sceneParam;
    private string map1 = "graph_6.txt";
    private string map2 = "graph_7.txt";
    private string map3 = "graph_8.txt";
    private string map4 = "graph_1.txt";
    private string map5 = "graph_2.txt";
    private string map6 = "graph_3.txt";
    private string map7 = "graph_4.txt";
    private string map8 = "graph_5.txt";
    private ArrayList maps;
    private List<Toggle> mapsToggle;


    public void Start()
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
        mapsToggle = new List<Toggle>();
        for (int i = 0; i < maps.Count; i++)
        {
            mapsToggle.Add(numImage.GetComponentsInChildren<Toggle>()[i]);
        }
        for (int i = 3; i < maps.Count; i++)
        {
            mapsToggle[i].gameObject.SetActive(false);
        }
    }
    public void LoadLevel()
    {

        dataScene = getData();
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(sceneParam);
        //StartCoroutine(LoadAsynchronously(scene));
    }

    public void QuitApplication()
    {
        Application.Quit();
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
                sceneParam = "Simulation";
                break;
            case 2:
                sceneParam = "ParamACO";
                break;
        }
        // Get number of agents
        int nbAgents = 1;
        nbAgents = int.Parse(nbAgent.text);

        int nbIterationsBStop = 200;
        nbIterationsBStop = int.Parse(nbIterations.text);

        // Get file's path of the map
        string filePath;
        filePath = fileMap.textComponent.text;
        if(filePath == "")
        {
            for(int i = 0; i < maps.Count; i++)
            {
                if (mapsToggle[i].isOn)
                {
                    filePath = maps[i] as string;
                }
            }
        }

        // Add data in the list
        tmpListData.Add(numAlgo);
        tmpListData.Add(nbAgents);
        tmpListData.Add(filePath);
        tmpListData.Add(nbIterationsBStop);
        return tmpListData;
    }

    public void updateMaps()
    {   
        if (algos.GetComponentsInChildren<Toggle>()[0].isOn)
        {
            for (int i = 3; i < maps.Count; i++)
            {
                mapsToggle[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 3; i < maps.Count; i++)
            {
                mapsToggle[i].gameObject.SetActive(true);
            }
        }
        
    }

    public void Update()
    {
        try
        {
            if (nbAgent.text != "" && nbAgent.text != "-")
            {
                tmpNbAgent = Mathf.Clamp(float.Parse(nbAgent.text), 1, 25);
                nbAgent.text = tmpNbAgent.ToString();
            }
            if (nbIterations.text != "" && nbIterations.text != "-")
            {
                float tmpNbIterations = Mathf.Clamp(float.Parse(nbIterations.text), 1, 1000);
                nbIterations.text = tmpNbIterations.ToString();
            }
        } catch (System.Exception e)
        {

        }

    }
}
