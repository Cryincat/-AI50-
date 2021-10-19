using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    public static string fileToLoad;
    // Start is called before the first frame update
    public void LoadScene()
    {
        fileToLoad = EditorUtility.OpenFilePanel("Select graph file", "","txt");
        if (fileToLoad != "")
        {
            SceneManager.LoadScene("Load");
        } 
        else
        {
            throw new Exception("File not found");
        }
    }
}
