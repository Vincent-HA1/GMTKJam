using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum DialogueSpeed
{
    Normal,
    Fast
}

/* Script to handle showing dialogue */
public class DialogueManager : MonoBehaviour
{
    public Action DialogueStart;
    public Action DialogueEnd;
    //public Action<NPC> EnterBattle;
    //public Action<NPC, int> StartQuiz;
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] InputHandler inputHandler;
    [SerializeField] private GameObject dialogueScreen;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueTextBox;

    [Header("Attributes")]
    [SerializeField] private float normalLettersPerSecond = 30;
    [SerializeField] private float fastLettersPerSecond = 50;

    [Header("Test Dialogue")]
    [SerializeField] Dialogue dialogueToPlay;
    [SerializeField] bool playDialogue;
    [SerializeField] bool playedTestDialogue;

    private float lettersPerSecond;
    private bool dialoguePlaying = false;
    private bool dialogueConfirmPressed = false;


    public void SetDialogueSpeed(DialogueSpeed dialogueSpeed)
    {
        switch (dialogueSpeed)
        {
            case DialogueSpeed.Normal:
                lettersPerSecond = normalLettersPerSecond;
                break;
            case DialogueSpeed.Fast:
                lettersPerSecond = fastLettersPerSecond;
                break;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        BindEvents();
        dialogueScreen.SetActive(false); //Hide dialogue screen at the start of the game
        //SetDialogueSpeed(DialogueSpeed.Normal); //Set in settings later
    }

    /* Bind dialogue starting events */
    private void BindEvents()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        CheckForInteractKey();

        //Test dialogue code
        if (playDialogue && !playedTestDialogue)
        {
            StartDialogue(dialogueToPlay);
            playedTestDialogue = true;
        }
    }

    void CheckForInteractKey()
    {
        //Store the input in a bool (so we know it was pressed)
        if (dialoguePlaying && inputHandler.confirmPressed) //&& !BattleManager.inBattle)
        {
            dialogueConfirmPressed = true;
        }
    }

    ///* Start dialogue using the chests' dialogue to show the contents of the chest */
    //private void ShowChestDescription(Chest chest, Dialogue dialogue)
    //{
    //    StartDialogue(dialogue, false, false, null);
    //}

    /* Shows the dialogue that was passed into the function */
    private void StartDialogue(Dialogue dialogue)//, bool triggerBattle, bool triggerQuiz, NPC npc)
    {
        if (dialoguePlaying) return; //if dialogue already playing, ignore this call (in the case of random, erroneous calls to this function)
        GameManager.cannotAct = true;
        dialoguePlaying = true;
        DialogueStart?.Invoke();
        //Show the dialogue
        dialogueScreen.SetActive(true);
        StartCoroutine(ShowDialogueCoroutine(dialogue));
        StartCoroutine(EndDialogue());// triggerBattle, triggerQuiz, npc));
    }

    /* Displays the dialogue sentence by sentence, using small delays to display each sentence character by character. */
    private IEnumerator ShowDialogueCoroutine(Dialogue dialogue)
    {
        //While there are sentences left, show them one by one
        List<string> dialogueQueue = (dialogue.sentences).ToList();
        while (dialogueQueue.Count > 0)
        {
            dialogueConfirmPressed = false; //Set this to false at the start of a sentence, because we want to know if it was pressed after the sentence is being displayed
            string sentence = dialogueQueue[0];
            dialogueQueue.RemoveAt(0);
            //Clear dialogue box initially
            dialogueTextBox.text = "";
            //Show the sentence letter by letter
            for (int i = 0; i < sentence.Length; i++)
            {
                yield return new WaitForEndOfFrame();
                //If the play key was pressed, the sentence shows in full instantly (instead of character by character)
                if (dialogueConfirmPressed)
                {
                    //Skip dialogue
                    dialogueTextBox.text = sentence;
                    dialogueConfirmPressed = false;
                    break;
                }
                //Otherwise, add each character one by one, and wait for a delayed time between each character
                dialogueTextBox.text += sentence[i];
                yield return new WaitForSecondsRealtime(1 / lettersPerSecond);
            }
            //After the sentence has been displayed, wait again for the confirm key for the player to close this sentence.
            dialogueConfirmPressed = false;
            yield return new WaitUntil(() => dialogueConfirmPressed);
        }
        //Finished dialogue
        dialoguePlaying = false;
        dialogueScreen.SetActive(false);
        dialogueTextBox.text = ""; //empty text box just in case

    }

    /* At the end of dialogue, can trigger a battle or a quiz. If any of the bools are true, do this */
    private IEnumerator EndDialogue()//bool triggerBattle, bool triggerQuiz, NPC npc)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => dialoguePlaying == false);
        GameManager.cannotAct = false;
        DialogueEnd?.Invoke();
    }
}
