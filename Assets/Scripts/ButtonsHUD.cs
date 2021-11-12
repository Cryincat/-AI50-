using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsHUD : MonoBehaviour
{

    public TextMeshProUGUI timerText;
    public float timer;
    public float speed = 1;
    public Agent agent;
    public bool playing;
    public Image buttonPausePlay;
    public Sprite pauseImage;
    public Sprite playImage;
    // Start is called before the first frame update
    void Start()
    {
        agent = FindObjectOfType<Agent>();
        playing = true;
        timer = 0;
        speed = 1;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * speed;
        string minutes = Mathf.Floor((timer % 3600) / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");
        timerText.text = minutes + ":" + seconds;
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
            Time.timeScale = 1;
            buttonPausePlay.sprite = pauseImage;
        }
    }

    public void clickx1()
    {
        speed = 1;
        agent.speed = speed;
    }

    public void clickx2()
    {
        speed = 2;
        agent.speed = speed;
    }

    public void quitSimulation()
    {
        //SceneManager.LoadScene("Data");
        //Destroy(FindObjectOfType<ScriptFields>());
    }
}
