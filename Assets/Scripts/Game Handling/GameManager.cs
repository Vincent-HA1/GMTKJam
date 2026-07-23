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
    [SerializeField] List<Checkpoint> checkpointList;


    public static bool cannotAct = false;

    bool respawning = false;
    Checkpoint currentCheckpoint;

    private void Awake()
    {
        LoadGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(hudManager)hudManager.InitialiseUI(); //fill with player stats health
        currentCheckpoint = checkpointList[0];
        BindEvents();
        SaveGame(); //save here
        StartCoroutine(WaitForSceneFade());
    }

    void BindEvents()
    {
        if (pauseMenu) pauseMenu.Quit += ReturnToTitle;
        //Player events
        player.Healed += UpdateHealth;
        player.Hit += UpdateHealth;
        player.Death += Respawn;

        //Checkpoints
        foreach (Checkpoint checkpoint in checkpointList)
        {
            if (!checkpoint.isEndFlag)
            {
                checkpoint.CheckpointReached += UpdateCurrentCheckpoint;
            }
            else
            {
                //Final flag ends the level
                checkpoint.CheckpointReached += EndLevel;
            }
        }
    }

    IEnumerator WaitForSceneFade()
    {
        cannotAct = true;
        yield return new WaitForEndOfFrame();
        //if (movingSpikes) movingSpikes.SetPosition();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cannotAct = false;
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
        player.Respawn(currentCheckpoint.GetPosition()); //set position for now
        yield return new WaitForSeconds(0.5f);
        //if (movingSpikes) movingSpikes.SetPosition();
        sceneFadeAnimator.SetTrigger("FadeIn");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cannotAct = false;
        respawning = false;
    }

    void UpdateCurrentCheckpoint(Checkpoint checkpoint)
    {
        print(checkpoint);
        currentCheckpoint = checkpoint;
    }


    void EndLevel(Checkpoint endFlag)
    {
        cannotAct = true;
        //Player reached end of stage
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        print(currentSceneIndex);
        print(SceneManager.sceneCountInBuildSettings - 1);
        if(currentSceneIndex >= SceneManager.sceneCountInBuildSettings - 1)
        {
            //WE'RE AT THE END OF THE GAME. DO SOMETHING
            //For now, return to title
            ReturnToTitle();
        }
        else
        {
            player.ReachedEndOfLevel();
            LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Save at start and end of level
    void SaveGame()
    {
        //Create the instance of SaveData and save the game
        SaveData data = new SaveData(optionsManager.GetOptions(), SceneManager.GetActiveScene().buildIndex);
        SaveSystem.Save(data);

    }

    /* Load the save data and restore the game state */
    void LoadGame()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData != null)
        {
            //Restore saved options
            optionsManager.SetOptions(saveData != null ? saveData.options : new Options());
        }

    }

    void ReturnToTitle()
    {
        LoadScene(0);
    }

    void LoadScene(int sceneIndex)
    {
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

    private void OnApplicationQuit()
    {
        //Default for now
        //SaveGame();
    }
}
