using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SimpleCutscene : MonoBehaviour
{
    public bool requiresManualPlay = false; //requires another script to play it
    [Header("References")]
    [SerializeField] PlayableDirector pd;

    BoxCollider2D boxCollider2D;
    private bool isPlaying = false;
    public bool cutsceneHasPlayed = false;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        pd.stopped += FinishCutscene;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (isPlaying && pd.time == pd.duration)
        {
            print("finish cutscene");
            FinishCutscene();
        }
        */
    }

    public void PlayCutscene()
    {
        GameManager.cannotAct = true;
        isPlaying = true;
        pd.Play();
    }

    void FinishCutscene(PlayableDirector pd)
    {
        print("celastview");
        isPlaying = false;
        cutsceneHasPlayed = true;
        GameManager.cannotAct = false;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !requiresManualPlay && !cutsceneHasPlayed)
        {
            if (boxCollider2D) boxCollider2D.enabled = false;
            GameManager.cannotAct = true;
            pd.Play();
            isPlaying = true;
        }
    }

    public void ResetCutscene()
    {
        cutsceneHasPlayed = false;
        if(boxCollider2D)boxCollider2D.enabled = true;
    }
}
