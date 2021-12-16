using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParamBtnACO : MonoBehaviour
{
                 
    public void StartSimulation(string scene)
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        //
        SceneManager.LoadScene(scene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
