using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [Header("Other Managers")]
    [SerializeField] OptionsManager optionsManager;
    [SerializeField] HUDManager hudManager;

    [Header("References")]
    [SerializeField] PlayerMovement player;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] Animator sceneFadeAnimator;


    public static bool cannotAct = false;

    bool respawning = false;

    private void Awake()
    {
        LoadGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        hudManager.InitialiseUI(); //fill with player stats health
        BindEvents();
    }

    void BindEvents()
    {
        if (pauseMenu) pauseMenu.Quit += ReturnToTitle;
        //Player events
        player.Healed += UpdateHealth;
        player.Hit += UpdateHealth;
        player.Death += Respawn;
    }

    void UpdateHealth(float health)
    {
        //Update the UI for health
        hudManager.UpdateHealthAmount(health);
    }
    void Respawn()
    {
        if (!respawning)
        {
            respawning = true;
            StartCoroutine(RespawnAfterFade());
        }

    }

    IEnumerator RespawnAfterFade()
    {
        //Fade out, and put the player at their last respawn point.
        cannotAct = true;
        yield return new WaitForEndOfFrame();
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        hudManager.UpdateHealthAmount(player.MaxHealth);
        player.Respawn(new Vector3(0, 0.28f)); //set position for now
        yield return new WaitForSeconds(0.5f);
        //if (movingSpikes) movingSpikes.SetPosition();
        sceneFadeAnimator.SetTrigger("FadeIn");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cannotAct = false;
        respawning = false;
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
