using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject main;
    public GameObject options;
    public GameObject howToPlay;
    public GameObject credits;
    
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        //ArgDisplay.text = System.Environment.CommandLine;
        foreach (string a in args)
        {
            if (a.StartsWith("PORT_") || a.Contains("MASTER"))
            {
                //Load Wan scene.
                SceneManager.LoadScene(1);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void WANConnect()
    {
        //Load Wan scene.
        SceneManager.LoadScene(1);
    }

    public void LANConnect()
    {
        //Load Lan Scene;
        SceneManager.LoadScene(2);
    }

    public void QuitMe()
    {
        Application.Quit();
    }

    public void ShowMain()
    {
        main.SetActive(true);
        options.SetActive(false);
        howToPlay.SetActive(false);
        credits.SetActive(false);
    }

    public void ShowOptions()
    {
        main.SetActive(false);
        options.SetActive(true);
        howToPlay.SetActive(false);
        credits.SetActive(false);
    }

    public void ShowHowToPlay()
    {
        main.SetActive(false);
        options.SetActive(false);
        howToPlay.SetActive(true);
        credits.SetActive(false);
    }

    public void ShowCredits()
    {
        main.SetActive(false);
        options.SetActive(false);
        howToPlay.SetActive(false);
        credits.SetActive(true);
    }

}
