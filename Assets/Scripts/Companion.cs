using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Companion : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer player;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;

    [Header("Shooting Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform shootPos;
    [SerializeField] private float shootingFrequencyPerSecond = 1;

    // Internal tracking variables
    private Vector2 initialOffset;
    private Vector2 currentVelocity = Vector2.zero;
    private bool isFacingRight = true;
    private bool attackUnlocked = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb; // <--- Rigidbody reference

    // Attacking
    private float shootTimer = 0;
    private GameObject currentEnemyTarget;

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

        // Setup Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Ensure it doesn't get pushed by collisions
        //rb.interpolation = RigidbodyInterpolation2D.Interpolate; // <--- Prevents Cinemachine stutter!

        // Lock in starting offset
        initialOffset = transform.position - player.transform.position;
        isFacingRight = true;
    }

    private void Update()
    {
        if (GameManager.cannotAct) return;

        // Sprite visual flipping is fine in Update
        CheckPlayerDirection();

        AttackEnemies();
    }

    void AttackEnemies()
    {
        if (attackUnlocked && currentEnemyTarget != null)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
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

    // --- SWITCHED FROM LATEUPDATE TO FIXEDUPDATE ---
    private void LateUpdate()
    {
        if (player == null) return;

        // Calculate dynamic offset based on facing direction
        Vector2 currentOffset = initialOffset;
        if (!isFacingRight)
        {
            currentOffset.x = -initialOffset.x;
        }

        // Target position based on the player's RIGIDBODY position (not transform)
        // If your player has a Rigidbody2D, reference playerRb.position instead!
        Vector2 targetPosition = (Vector2)player.transform.position + currentOffset;//transform.position + currentOffset;

        // Use Vector3.MoveTowards or Vector3.Lerp instead of SmoothDamp in FixedUpdate
        Vector3 nextPosition = Vector3.MoveTowards(rb.position, targetPosition, followSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);
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
            currentEnemyTarget = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (collision.gameObject == currentEnemyTarget)
            {
                currentEnemyTarget = null;
                ResetShootTimer();
            }
        }
    }
}