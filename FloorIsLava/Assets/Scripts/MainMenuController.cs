using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject main;
    public GameObject options;
    public GameObject howTo;
    public GameObject credits;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
    public void SetMain()
    {
        main.SetActive(true);
        options.SetActive(false);
        howTo.SetActive(false);
        credits.SetActive(false);
    }

    public void SetOptions()
    {
        main.SetActive(false);
        options.SetActive(true);
        howTo.SetActive(false);
        credits.SetActive(false);
    }

    public void SetHowTo()
    {
        main.SetActive(false);
        options.SetActive(false);
        howTo.SetActive(true);
        credits.SetActive(false);
    }

    public void SetCredits()
    {
        main.SetActive(false);
        options.SetActive(false);
        howTo.SetActive(false);
        credits.SetActive(true);
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
}
