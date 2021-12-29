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
    private int numMethod;

    private Load load;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<Load>().isGenerated);
        load = FindObjectOfType<Load>();
        yield return new WaitUntil(() => load.isGenerated);
        numMethod = load.typeMethod;

        switch (numMethod)
        {
            case 0: // Method RL
                yield return new WaitUntil(() => FindObjectOfType<DQNManager>().isReady);
                break;
            case 1: // MAM
                yield return new WaitUntil(() => FindObjectOfType<LoadMethod>().isReady);
                break;
            case 2: // ACO
                yield return new WaitUntil(() => FindObjectOfType<Manager_ACO>().isReady);
                break;
            default:
                throw new System.Exception("Method num is unknown.");
                break;
        }
        
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
        if (isGenerated && dataM)
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
        speed = 5;
        if (Time.timeScale != 0)
        {
            Time.timeScale = speed;
        }
    }

    public void clickx2()
    {
        speed = 10;
        if (Time.timeScale != 0)
        {
            Time.timeScale = speed;
        }
    }

    public void clickx5()
    {
        speed = 15;
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
