using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator sceneFadeAnimator;
    [SerializeField] InputHandler inputHandler;

    [Header("UI References")]
    [SerializeField] Button newGameButton;
    [SerializeField] Button continueGameButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button optionsButton;
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject optionsScreen;

    EventSystem eventSystem;
    SaveData saveToLoad;

    bool loadingScene = false;
    // Start is called before the first frame update
    void Start()
    {
        //Hide cursor automatically
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        eventSystem = EventSystem.current;
        eventSystem.enabled = false;
        Time.timeScale = 1;
        CheckForSave();
        newGameButton.onClick.AddListener(() => StartGame());
        continueGameButton.onClick.AddListener(() => StartGame(true));
        quitButton.onClick.AddListener(QuitGame);
        optionsButton.onClick.AddListener(OpenOptions);
        StartCoroutine(WaitForSceneFade());
    }

    void CheckForSave()
    {
        //See if there is anything to continue from
        saveToLoad = SaveSystem.Load();
        if (saveToLoad != null)
        {
            //enable continue
            continueGameButton.gameObject.SetActive(true);
        }
    }

    IEnumerator WaitForSceneFade()
    {
        //Wait for screen wipe before allowing input
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        eventSystem.enabled = true;
        if (continueGameButton.isActiveAndEnabled)
        {
            eventSystem.SetSelectedGameObject(continueGameButton.gameObject);
        }
        else
        {
            eventSystem.SetSelectedGameObject(newGameButton.gameObject);
        }
    }

    private void Update()
    {
        if (loadingScene) return;
        if(inputHandler.pausePressed && optionsScreen.activeInHierarchy)
        {
            CloseOptions();
        }
    }

    void OpenOptions()
    {
        titleScreen.SetActive(false);
        optionsScreen.SetActive(true);
    }

    void CloseOptions()
    {
        titleScreen.SetActive(true);
        optionsScreen.SetActive(false);
    }

    void StartGame(bool loadSave = false)
    {
        if (loadSave)
        {
            //Load saved scene
            LoadScene(saveToLoad.levelIndex);
        }
        else
        {
            //Delete save
            SaveSystem.DeleteSave();
            LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // load next scene
        }
    }

    void LoadScene(int sceneIndex)
    {
        loadingScene = true;
        StartCoroutine(LoadSceneAfterFade(sceneIndex));
        EventSystem.current.enabled = false;
    }

    IEnumerator LoadSceneAfterFade(int sceneIndex)
    {
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(sceneIndex); 
    }

    void QuitGame()
    {
        EventSystem.current.enabled = false;
        Application.Quit();
    }
}
