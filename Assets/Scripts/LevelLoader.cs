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
    static ArrayList dataScene;

    private float tmpNbAgent;
    private string map1 = "path1";
    private string map2 = "path2";
    private string map3 = "path3";
    private string map4 = "path4";
    private string map5 = "path5";
    private string map6 = "path6";
    private ArrayList maps;

    public void LoadLevel(string scene)
    {
        maps = new ArrayList();
        maps.Add(map1);
        maps.Add(map2);
        maps.Add(map3);
        maps.Add(map4);
        maps.Add(map5);
        maps.Add(map6);
        dataScene = getData();
        StartCoroutine(LoadAsynchronously(scene));
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

    ArrayList getData()
    {
        ArrayList tmpListData = new ArrayList();
        // Get selected algorithm
        int numAlgo = 0;
        Debug.Log(algos.GetComponentsInChildren<Toggle>());
        for(int i = 0; i < 3; i++)
        {
            if (algos.GetComponentsInChildren<Toggle>()[i].isOn)
            {
                numAlgo = i;
            }
        }
        // Get number of agents
        int nbAgents = 1;
        nbAgents = int.Parse(nbAgent.text);
        // Get file's path of the map
        string filePath;
        filePath = fileMap.textComponent.text;
        if(filePath == "")
        {
            for(int i = 0; i < 6; i++)
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
