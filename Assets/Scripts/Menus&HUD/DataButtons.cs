using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataButtons : MonoBehaviour
{

    public TextMeshProUGUI algo;
    public TextMeshProUGUI avIdleValue;
    public TextMeshProUGUI maxIdleValue;

    private DataManager dataManager;
    private LevelLoader levelLoader;
    private GameObject simuLoader;
    private GameObject prefabAlgo;

    // Start is called before the first frame update
    void Start()
    {
        dataManager = FindObjectOfType<DataManager>();
        levelLoader = FindObjectOfType<LevelLoader>();
        prefabAlgo = dataManager.gameObject;
        simuLoader = levelLoader.gameObject;
        algo.text = dataManager.methodName;
        avIdleValue.text = Math.Round(dataManager.mediumIdleness).ToString();
        maxIdleValue.text = Math.Round(dataManager.maxIdleness).ToString();

    }

    public void newSimulation()
    {
        Destroy(simuLoader);
        Destroy(prefabAlgo);
        SceneManager.LoadScene("Parameters");
    }

    public void exportData()
    {
        DataManager dataManager = FindObjectOfType<DataManager>();
        dataManager.Save();
    }
}
