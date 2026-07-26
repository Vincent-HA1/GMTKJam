using UnityEngine;

public class PlatformMoveUp : MonoBehaviour
{
    public float moveDistance = 3f; 
    public float moveSpeed = 2f;      

    private Vector3 targetPosition;
    private bool move = false;

    void Start()
    {
        targetPosition = transform.position + Vector3.up * moveDistance;
    }

    void Update()
    {
        if (move)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)    {
        move = true;
    }
}