using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Action RetryStage;
    public Action Quit;

    [Header("References")]
    [SerializeField] InputHandler inputs;

    [Header("UI References")]
    [SerializeField] GameObject background;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject retryMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject optionsMenu;

    [Header("Pause Menu References")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;



    public bool paused { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ClosePauseScreen);
        quitButton.onClick.AddListener(QuitGame);
        pauseMenu.SetActive(false);
        restartButton.onClick.AddListener(Retry);
        optionsButton.onClick.AddListener(OpenOptions);
        //returnToStageButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.cannotAct && !paused) return;
        if (inputs.pausePressed && !paused)
        {
            PauseGame();
        }
        else if (inputs.cancelPressed || inputs.pausePressed)
        {
            if (paused)
            {
                if (pauseMenu.activeInHierarchy)
                {
                    ClosePauseScreen();
                }
                else if (optionsMenu.activeInHierarchy)
                {
                    CloseOptions();
                }
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        pauseMenu.SetActive(true);
        background.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        paused = true;
        GameManager.cannotAct = true;
    }

    void ClosePauseScreen()
    {
        background.SetActive(false);
        pauseScreen.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        paused = false;
        GameManager.cannotAct = false;
        EventSystem.current.SetSelectedGameObject(null);
        inputs.ResetAllBools(); //Clear inputs on unpausing
    }

    void OpenOptions()
    {
        optionsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenRetryMenu()
    {
        pauseScreen.SetActive(true);
        retryMenu.SetActive(true);
    }

    public void Retry()
    {
        RetryStage?.Invoke();
    }

    public void QuitGame()
    {
        Quit?.Invoke();
    }

}
