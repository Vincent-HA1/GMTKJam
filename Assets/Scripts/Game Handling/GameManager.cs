using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [Header("Other Managers")]
    [SerializeField] OptionsManager optionsManager;

    [Header("References")]
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] Animator sceneFadeAnimator;


    public static bool cannotAct = false;

    private void Awake()
    {
        LoadGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(pauseMenu) pauseMenu.Quit += ReturnToTitle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveGame()
    {
        //Create the instance of SaveData and save the game
        SaveData data = new SaveData(optionsManager.GetOptions());
        SaveSystem.Save(data);

    }

    /* Load the save data and restore the game state */
    void LoadGame()
    {
        SaveData saveData = SaveSystem.Load();
        //Restore saved options
        optionsManager.SetOptions(saveData != null ? saveData.options : new Options());
    }

    void ReturnToTitle()
    {
        LoadScene("TitleScreen");
    }

    void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAfterFade(sceneName));
        EventSystem.current.enabled = false;
    }

    IEnumerator LoadSceneAfterFade(string sceneToLoad)
    {
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(sceneToLoad); //Filler for now
    }

    private void OnApplicationQuit()
    {
        //Default for now
        SaveGame();
    }
}
