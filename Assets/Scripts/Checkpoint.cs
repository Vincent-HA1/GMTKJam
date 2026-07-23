using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] Vector3 setPosition;

    public bool isEndFlag = false;
    public Action<Checkpoint> CheckpointReached;
    Animator animator;

    bool reached = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Trigger the checkpoint when the player touches it, or if a ghost reaches the end flag
        if ((collision.CompareTag("Player")) && !reached)
        {
            print("hello");
            CheckpointReached?.Invoke(this);
            //animator.SetBool("Found", true);
            reached = true;
        }
    }

    public Vector3 GetPosition()
    {
        return setPosition;
    }
}
