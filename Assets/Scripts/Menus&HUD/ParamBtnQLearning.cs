using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParamBtnQLearning : MonoBehaviour
{
    public static string fileToLoad;
    public ToggleGroup isTraining;
    public TMP_InputField nbIterations;
    public TMP_InputField weightField;
    public Button weightBtn;
    //public GameObject MapSelection;

    public void LoadWeightFile()
    {
        fileToLoad = EditorUtility.OpenFilePanel("Select weight file", "", "txt");
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
            float tmpNbAgent = Mathf.Clamp(float.Parse(nbIterations.text), 1, 10000);
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
