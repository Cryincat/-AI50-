using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using SmartDLL;

public class ParamBtnQLearning : MonoBehaviour
{
    public static string fileToLoad;
    public ToggleGroup isTraining;
    public TMP_InputField nbIterations;
    public TMP_InputField weightField;
    public Button weightBtn;
    public SmartFileExplorer fileExplorer = new SmartFileExplorer();
    //public GameObject MapSelection;

    public void LoadWeightFile()
    {

        string initialDir = Directory.GetCurrentDirectory() + "/AI50_Data/StreamingAssets/Data/";
        bool restoreDir = true;
        string title = "Select weight file";

        fileExplorer.OpenExplorer(initialDir, restoreDir, title, null, null);
        fileToLoad = fileExplorer.fileName;
        if (fileToLoad != "")
        {
            weightField.textComponent.SetText(fileToLoad);
        }
        else
        {
            weightField.textComponent.SetText("");
            throw new Exception("File not found");
        }
    }

    public void StartSimulation(string scene)
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        if(nbIterations.text != "")
        {
            levelLoader.dataScene.Add(int.Parse(nbIterations.text));
        }
        else
        {
            levelLoader.dataScene.Add(weightField.textComponent.text);
        }
        SceneManager.LoadScene(scene);
    }

    public void Update()
    {
        if (nbIterations.text != "" && nbIterations.text != "-")
        {
            float tmpNbAgent = Mathf.Clamp(float.Parse(nbIterations.text), 1, 100000000);
            nbIterations.text = tmpNbAgent.ToString();
        }

        if (isTraining.GetComponentsInChildren<Toggle>()[0].isOn)
        {
            nbIterations.interactable = true;
            weightBtn.interactable = false;
            weightField.textComponent.SetText("");

        }
        else
        {
            nbIterations.interactable = false;
            weightBtn.interactable = true;
            nbIterations.text = "";
        }
    }
}
