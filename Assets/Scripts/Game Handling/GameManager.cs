using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{

    [Header("Other Managers")]
    //[SerializeField] OptionsManager optionsManager;
    [SerializeField] HUDManager hudManager;
    [SerializeField] GameObject victoryScreen;

    [Header("References")]
    [SerializeField] PlayerMovement player;
    [SerializeField] FlyingBossEnemy boss;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] Animator sceneFadeAnimator;
    [SerializeField] List<Checkpoint> checkpointList;
    [SerializeField] List<SimpleCutscene> cutsceneList;
    [SerializeField] GameObject collectiblesParent;


    public static bool cannotAct = false;

    bool respawning = false;
    Checkpoint currentCheckpoint;

    //Player stats stuff
    int fragmentAmount;
    PlayerUpgrades currentPlayerUpgrades;
    PlayerStats currentPlayerStats;

    private bool finalStage = false;
    private void Awake()
    {
        LoadGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentCheckpoint = checkpointList[0];
        player.Respawn(currentCheckpoint.GetPosition());
        Time.timeScale = 1;
        BindEvents();
        SaveGame(); //save here
        StartCoroutine(WaitForSceneFade());
    }

    void BindEvents()
    {
        pauseMenu.Quit += ReturnToTitle;
        pauseMenu.RetryStage += RetryLevel;

        //Player events
        player.Healed += UpdateHealth;
        player.Hit += UpdateHealth;
        player.Death += FailLevel;
        player.CooldownChanged += hudManager.UpdateCooldown;

        //Boss
        if (boss != null) boss.Death += IsFinalStage;

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

        //Collectibles

        List<Collectible> allCollectibles = collectiblesParent.GetComponentsInChildren<Collectible>().ToList();
        //Collectible events on pick up
        foreach (Collectible collectible in allCollectibles)
        {
            switch (collectible.GetCollectibleType())
            {
                case Collectible.CollectibleType.Fragment:
                    collectible.PickedUp += UpdateFragmentAmount;
                    break;
                case Collectible.CollectibleType.Puzzle:
                    //BigCoin bigCoin = (BigCoin)collectible;
                    //bigCoin.PickedUpBigCoin += FoundBigCoin;
                    //bigCoins.Add(bigCoin);
                    break;
            }
        }

        //Cutscenes
        foreach(SimpleCutscene cutscene in cutsceneList)
        {
            player.Respawned += cutscene.ResetCutscene;
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

    void IsFinalStage()
    {
        finalStage = true;
    }

    void UpdateHealth(float health)
    {
        //Update the UI for health
        hudManager.UpdateHealthAmount(health);
    }

    void UpdateFragmentAmount()
    {
        fragmentAmount += 10;
        hudManager.UpdateFragmentAmount(fragmentAmount);
    }

    //puzzle piece

    //void FoundBigCoin(BigCoin bigCoinFound)
    //{
    //    int bigCoinIndex = bigCoins.IndexOf(bigCoinFound);
    //    hudManager.UpdateBigCoinIndicator(bigCoinIndex);
    //    currentStageSave.bigCoinsFound[bigCoinIndex] = 1; //set to found
    //}

    void FailLevel()
    {
        print("hello");
        cannotAct = true;
        pauseMenu.OpenRetryMenu();
        Time.timeScale = 0;
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

        if (finalStage)
        {
            //WE'RE AT THE END OF THE GAME. DO SOMETHING
            //For now, return to title
            StartCoroutine(ShowVictory());
        }
        else
        {
            player.ReachedEndOfLevel();
            SaveGame();
            LoadScene("Shop"); //go to shop
        }
        
        ////Last scene is shop, so check if scene before that
        //if(currentSceneIndex >= SceneManager.sceneCountInBuildSettings - 2)
        //{
        //    //WE'RE AT THE END OF THE GAME. DO SOMETHING
        //    //For now, return to title
        //    ReturnToTitle();
        //}
        //else
        //{
        //    player.ReachedEndOfLevel();
        //    SaveGame();
        //    LoadScene("Shop"); //go to shop
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ShowVictory()
    {
        GameManager.cannotAct = true;
        victoryScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        ReturnToTitle();
    }

    //Save at start and end of level
    void SaveGame()
    {
        //Create the instance of SaveData and save the game
        //REMOVED OPTION MANAGER, JUST INITIALISE OPTIONS
        SaveData data = new SaveData(new Options(), SceneManager.GetActiveScene().buildIndex, fragmentAmount, currentPlayerUpgrades, currentPlayerStats);
        SaveSystem.Save(data);
    }

    /* Load the save data and restore the game state */
    void LoadGame()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData != null)
        {
            fragmentAmount = saveData.fragments;
            currentPlayerUpgrades = saveData.upgrades;
            currentPlayerStats = saveData.stats;
            //Restore saved options
            //optionsManager.SetOptions(saveData != null ? saveData.options : new Options());
            hudManager.InitialiseUI(3 + currentPlayerStats.healthAdd, fragmentAmount, currentPlayerUpgrades.specialUnlocked);
        }
        else
        {
            //No Save, initialise everyting
            hudManager.InitialiseUI();
            currentPlayerUpgrades = new PlayerUpgrades();
            currentPlayerStats = new PlayerStats();
        }
        //Set player stuff
        player.SetPlayerUpgrades(currentPlayerUpgrades);
        player.ApplyStatUpgrades(currentPlayerStats);

    }

    void RetryLevel()
    {
        LoadScene(SceneManager.GetActiveScene().name);
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

    IEnumerator LoadSceneAfterFade(string sceneName)
    {
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(sceneName); 
    }


    private void OnApplicationQuit()
    {
        //Default for now
        //SaveGame();
    }
}
