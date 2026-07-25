using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Companion : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer player;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    
     

    [Header("Shooting Settings")]
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootingFrequencyPerSecond = 1;


    // Internal tracking variables
    private Vector3 initialOffset;
    private Vector3 currentVelocity = Vector3.zero;
    private bool isFacingRight = true;
    private bool attackUnlocked = false;

    SpriteRenderer spriteRenderer;

    //Attacking
    float shootTimer = 0;
    GameObject currentEnemyTarget;

    public void SetAttackUnlocked()
    {
        attackUnlocked = true;
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Companion: Player transform is not assigned in the inspector!", this);
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        // Lock in starting offset
        initialOffset = transform.position - player.transform.position;

        // Determine initial facing direction based on scale or movement
        isFacingRight = true;
    }

    private void Update()
    {
        if (GameManager.cannotAct) return;
        AttackEnemies();
    }

    void AttackEnemies()
    {
        if (attackUnlocked && currentEnemyTarget != null)
        {
            shootTimer -=Time.deltaTime;
            if(shootTimer <= 0)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        ResetShootTimer();
        GameObject p = Instantiate(projectile);
        Vector2 directionToFace = (currentEnemyTarget.transform.position - transform.position).normalized;
        p.GetComponent<Projectile>().SetFlight(directionToFace, shootPos.position);
    }

    void ResetShootTimer()
    {
        shootTimer = 1 / shootingFrequencyPerSecond;
    }


    void LateUpdate()
    {
        if (player == null) return;

        // Check if player flipped (using localScale.x as standard direction indicator)
        CheckPlayerDirection();

        // Calculate dynamic offset based on facing direction
        Vector3 currentOffset = initialOffset;

        if (!isFacingRight)
        {
            // Flip the X offset to move to the opposite shoulder
            currentOffset.x = -initialOffset.x;
        }

        // Calculate final target position
        Vector3 targetPosition = player.transform.position + currentOffset;

        // Smoothly move toward the target
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);
    }

    private void CheckPlayerDirection()
    {
        isFacingRight = !player.flipX;
        spriteRenderer.flipX = isFacingRight;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && currentEnemyTarget == null)
        {
            //Enemy has entered range
            currentEnemyTarget = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //Enemy has left range
            if(collision.gameObject == currentEnemyTarget)
            {
                currentEnemyTarget = null;
                ResetShootTimer();
            }
        }
    }
}