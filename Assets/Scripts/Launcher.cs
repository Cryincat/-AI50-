using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
    public static string fileToLoad;
    public TMP_InputField fieldMap;
    public GameObject MapSelection;

    // Start is called before the first frame update
    public void LoadMap()
    {
        fileToLoad = EditorUtility.OpenFilePanel("Select graph file", "","txt");
        if (fileToLoad != "")
        {
            MapSelection.GetComponent<ToggleGroup>().SetAllTogglesOff();
            fieldMap.textComponent.SetText(fileToLoad);

            //SceneManager.LoadScene("Load");
        } 
        else
        {
            throw new Exception("File not found");
        }
    }

    public void Update()
    {

        if (MapSelection.GetComponent<ToggleGroup>().AnyTogglesOn())
        {
            fileToLoad = "";
            fieldMap.textComponent.SetText(fileToLoad);
        }
    }
}
