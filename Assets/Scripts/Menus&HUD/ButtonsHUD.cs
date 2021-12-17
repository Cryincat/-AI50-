using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsHUD : MonoBehaviour
{

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI maxIdleValue;
    public TextMeshProUGUI averageIdleValue;
    public float timer;
    public float speed = 1;
    public bool playing;
    public Image buttonPausePlay;
    public Sprite pauseImage;
    public Sprite playImage;

    private DataManager dataM;
    private bool isGenerated;
    private GameObject loadGraph;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<load>().isGenerated);
        yield return new WaitUntil(() => FindObjectOfType<LoadMethod>().isReady);
        yield return new WaitUntil(() => FindObjectOfType<LoadGraph>().isGenerated);
        dataM = FindObjectOfType<DataManager>();
        playing = true;
        timer = 0;
        speed = 1;
        Time.timeScale = speed;
        isGenerated = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGenerated)
        {
            timer += Time.deltaTime;
            string minutes = Mathf.Floor((timer % 3600) / 60).ToString("00");
            string seconds = (timer % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
            maxIdleValue.text = (Mathf.Round(dataM.maxIdleness)).ToString(); // Recupérer l idleness max
            averageIdleValue.text = (Mathf.Round(dataM.mediumIdleness)).ToString(); // Recuperer l idleness moyenne
        }
    }

    public void clickPlay()
    {
        if (playing)
        {
            playing = false;
            Time.timeScale = 0;
            buttonPausePlay.sprite = playImage;
        }
        else
        {
            playing = true;
            Time.timeScale = speed;
            buttonPausePlay.sprite = pauseImage;
        }
    }

    public void clickx1()
    {
        speed = 1;
        if (Time.timeScale != 0)
        {
            Time.timeScale = speed;
        }
    }

    public void clickx2()
    {
        speed = 2;
        if (Time.timeScale != 0)
        {
            Time.timeScale = speed;
        }
    }

    public void clickx5()
    {
        speed = 5;
        if (Time.timeScale != 0)
        {
            Time.timeScale = speed;
        }
    }

    public void quitSimulation()
    {
        Time.timeScale = 0;
        SceneManager.LoadScene("Data");
        //Destroy(FindObjectOfType<ScriptFields>());
    }
}
