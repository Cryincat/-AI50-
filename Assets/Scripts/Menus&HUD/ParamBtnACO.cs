using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParamBtnACO : MonoBehaviour
{

    public TMP_InputField alpha;
    public TMP_InputField beta;
    public TMP_InputField evaporation;
    public TMP_InputField colony;

    public void StartSimulation(string scene)
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        levelLoader.dataScene.Add(int.Parse(alpha.text));
        levelLoader.dataScene.Add(int.Parse(beta.text));
        levelLoader.dataScene.Add(int.Parse(evaporation.text));
        levelLoader.dataScene.Add(int.Parse(colony.text));
        SceneManager.LoadScene(scene);
    }

    // Update is called once per frame
    void Update()
    {
        if (alpha.text != "" && alpha.text != "-")
        {
            float tmpAlpha = Mathf.Clamp(float.Parse(alpha.text), 0, 10);
            alpha.text = tmpAlpha.ToString();
        }
        if (beta.text != "" && beta.text != "-")
        {
            float tmpBeta = Mathf.Clamp(float.Parse(beta.text), 0, 10);
            beta.text = tmpBeta.ToString();
        }
        if (evaporation.text != "" && evaporation.text != "-")
        {
            float tmpEvap = Mathf.Clamp(float.Parse(evaporation.text), 0, 100);
            evaporation.text = tmpEvap.ToString();
        }
        if (colony.text != "" && colony.text != "-")
        {
            float tmpColony = Mathf.Clamp(float.Parse(colony.text), 1, 20);
            colony.text = tmpColony.ToString();
        }
    }
}
