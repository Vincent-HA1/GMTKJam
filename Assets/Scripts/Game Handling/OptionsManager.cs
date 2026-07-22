using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* A way to store the current options selected */
[System.Serializable]
public struct Options
{
    public DialogueSpeed dialogueSpeed;
}

public class OptionsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] DialogueManager dialogueManager;


    [Header("UI References")]
    [SerializeField] Button setNormalDialogue;
    [SerializeField] Button setFastDialogue;
    [SerializeField] Sprite selectedSprite;
    [SerializeField] Sprite unselectedSprite;

    Options currentOptions;

    // Start is called before the first frame update
    void Start()
    {
        //Set up on click events
        setNormalDialogue.onClick.AddListener(() => SetTextSpeed(DialogueSpeed.Normal));
        setFastDialogue.onClick.AddListener(() => SetTextSpeed(DialogueSpeed.Fast));

    }
    public Options GetOptions()
    {
        return currentOptions;
    }

    public void SetOptions(Options options)
    {
        currentOptions = options;
        //Update options as soon as this is set
        //SetBattleSpeed(currentOptions.battleSpeed);
        SetTextSpeed(currentOptions.dialogueSpeed);
        UpdateButtonSelection();
    }

    void UpdateButtonSelection()
    {
        if (currentOptions.dialogueSpeed == DialogueSpeed.Normal)
        {
            Select(setNormalDialogue);//, selectedSprite);
            Unselect(setFastDialogue);//, unselectedSprite);
        }
        else
        {
            Select(setFastDialogue);//, selectedSprite);
            Unselect(setNormalDialogue);//, unselectedSprite);
        }
    }


    void SetTextSpeed(DialogueSpeed dialogueSpeed)
    {
        if(dialogueManager) dialogueManager.SetDialogueSpeed(dialogueSpeed);
        currentOptions.dialogueSpeed = dialogueSpeed;
        UpdateButtonSelection();
    }

    void Select(Button button)
    {
        button.image.sprite = selectedSprite;
    }

    void Unselect(Button button)
    {
        button.image.sprite = unselectedSprite;
    }
}
