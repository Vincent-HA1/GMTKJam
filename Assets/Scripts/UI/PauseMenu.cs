using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Action Quit;

    [Header("References")]
    [SerializeField] InputHandler inputs;

    [Header("UI References")]
    [SerializeField] GameObject background;
    [SerializeField] GameObject pauseMenu;

    [Header("Pause Menu References")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;



    public bool paused { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ClosePauseScreen);
        quitButton.onClick.AddListener(QuitGame);
        pauseMenu.SetActive(false);
        //retryButton.onClick.AddListener(Retry);
        //returnToStageButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        //if (LevelManager.cannotAct && !paused) return;
        if (inputs.pausePressed && !paused)
        {
            PauseGame();
        }
        else if (inputs.cancelPressed || inputs.pausePressed)
        {
            if (paused && pauseMenu.activeInHierarchy)
            {
                ClosePauseScreen();
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        background.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        paused = true;
        //LevelManager.cannotAct = true;
    }

    void ClosePauseScreen()
    {
        background.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        paused = false;
        //LevelManager.cannotAct = false;
        EventSystem.current.SetSelectedGameObject(null);
        inputs.ResetAllBools(); //Clear inputs on unpausing
    }

    void Retry()
    {
        //RetryStage?.Invoke();
    }

    void QuitGame()
    {
        Quit?.Invoke();
    }

}
