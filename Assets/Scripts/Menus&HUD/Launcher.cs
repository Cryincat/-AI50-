using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using SmartDLL;


public class Launcher : MonoBehaviour
{
    public static string fileToLoad;
    public TMP_InputField fieldMap;
    public GameObject MapSelection;
    public SmartFileExplorer fileExplorer = new SmartFileExplorer();

    // Start is called before the first frame update
    public void LoadMap()
    {
        string initialDir = Directory.GetCurrentDirectory() + "/AI50_Data/StreamingAssets/Data/";
        bool restoreDir = true;
        string title = "Open a text file corresponding to graph";
        string defExt = ".txt";
        string filter = "txt files (*.txt)|*.txt";

        fileExplorer.OpenExplorer(initialDir, restoreDir, title, defExt, filter);
        fileToLoad = fileExplorer.fileName;
        
        if (fileToLoad != "")
        {
            MapSelection.GetComponent<ToggleGroup>().SetAllTogglesOff();
            fieldMap.textComponent.SetText(fileToLoad);
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
