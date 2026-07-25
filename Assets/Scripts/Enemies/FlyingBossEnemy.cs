using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBossEnemy : BaseEnemy
{
    public enum BossPhase { Flying, Descending, Grounded, Ascending }

    [Header("References")]
    [SerializeField] SimpleCutscene deathCutscene;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] List<Transform> walls;
    [SerializeField] float wallStartY;

    [Header("Boss State")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Flying;

    [Header("Flying Phase Settings")]
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float hoverHeightAbovePlayer = 4f;
    [SerializeField] private float flyingPhaseDuration = 8f;

    [Header("Orbit / Circle Settings")]
    [SerializeField] private float orbitRadiusX = 5f;     // Horizontal width of circle
    [SerializeField] private float orbitRadiusY = 1.5f;   // Vertical height variation (ellipse)
    [SerializeField] private float orbitSpeed = 2f;       // Speed of rotation around player

    [Header("Ground Phase Settings")]
    [SerializeField] private float groundPhaseDuration = 5f;
    [SerializeField] private float transitionSpeed = 5f; // Speed when diving or ascending
    [SerializeField] private LayerMask groundLayer;

    [Header("Boss Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPos;
    [SerializeField] private float shootInterval = 1.5f;

    // Timers & Position tracking
    private float phaseTimer;
    private float shootTimer;
    private float orbitAngle;
    private Vector2 targetGroundPosition;

    protected override void Start()
    {
        base.Start();

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null)
        {
            player = pm.transform;
            pm.Death += () => base.Start();
            pm.Respawned += ResetSelf;
        }

        // Set initial state
        phaseTimer = flyingPhaseDuration;
        shootTimer = shootInterval;

        if (rigid != null)
        {
            rigid.isKinematic = true;
            rigid.position = transform.position;
        }
    }

    private void OnEnable()
    {
        enemySpawner.gameObject.SetActive(true);
    }

    void ResetSelf()
    {
        base.Start();
        foreach (Transform wall in walls)
        {
            wall.transform.position = new Vector2(wall.transform.position.x, wallStartY);
        }
        enemySpawner.ResetSelf();
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        if (GameManager.cannotAct) return;
        base.Update();


        HandlePhasesAndTimers();
    }

    protected override void ManageMoveTimers() { }

    private void HandlePhasesAndTimers()
    {
        phaseTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            case BossPhase.Flying:
                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0f)
                {
                    ShootAtPlayer();
                    shootTimer = shootInterval;
                }

                if (phaseTimer <= 0f)
                {
                    StartDescending();
                }
                break;

            case BossPhase.Grounded:
                if (phaseTimer <= 0f)
                {
                    StartAscending();
                }
                break;
        }
    }

    protected override void FixedUpdate()
    {
        if (player == null || GameManager.cannotAct) return;

        switch (currentPhase)
        {
            case BossPhase.Flying:
                OrbitAbovePlayer();
                break;

            case BossPhase.Descending:
                MoveTowardPosition(targetGroundPosition, () => {
                    currentPhase = BossPhase.Grounded;
                    phaseTimer = groundPhaseDuration;
                });
                break;

            case BossPhase.Ascending:
                Vector2 targetAirPos = new Vector2(player.position.x, player.position.y + hoverHeightAbovePlayer);

                MoveTowardPosition(targetAirPos, () => {
                    currentPhase = BossPhase.Flying;
                    phaseTimer = flyingPhaseDuration;
                    shootTimer = shootInterval;
                });
                break;
        }
    }

    /* --- Movement Routines --- */

    private void OrbitAbovePlayer()
    {
        // Advance angle over time
        orbitAngle += orbitSpeed * Time.fixedDeltaTime;

        // Calculate offset position on an elliptical circle around player's head
        float xOffset = Mathf.Cos(orbitAngle) * orbitRadiusX;
        float yOffset = Mathf.Sin(orbitAngle) * orbitRadiusY;

        Vector2 targetPos = new Vector2(
            player.position.x + xOffset,
            player.position.y + hoverHeightAbovePlayer + yOffset
        );

        // Move towards target position smoothly
        Vector2 nextPos = Vector2.MoveTowards(rigid.position, targetPos, flySpeed * Time.fixedDeltaTime);
        rigid.MovePosition(nextPos);

        // Face towards player center, not motion direction
        float xDiff = player.position.x - transform.position.x;
        if (Mathf.Abs(xDiff) > 0.3f)
        {
            moveDirection.x = xDiff > 0 ? 1 : -1;
        }
        moving = true;
    }

    private void MoveTowardPosition(Vector2 destination, System.Action onArrived)
    {
        Vector2 nextPos = Vector2.MoveTowards(rigid.position, destination, transitionSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(nextPos);

        if (Vector2.Distance(rigid.position, destination) <= 0.25f)
        {
            onArrived?.Invoke();
        }
    }

    /* --- Phase Transition Triggers --- */

    private void StartDescending()
    {
        currentPhase = BossPhase.Descending;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 30f, groundLayer);
        if (hit.collider != null)
        {
            float halfHeight = boxCollider != null ? boxCollider.size.y / 2f : 0.5f;
            targetGroundPosition = new Vector2(transform.position.x, hit.point.y + halfHeight);
        }
        else
        {
            targetGroundPosition = new Vector2(transform.position.x, player.position.y);
        }
    }

    private void StartAscending()
    {
        currentPhase = BossPhase.Ascending;
    }

    /* --- Combat Logic --- */

    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || shootPos == null) return;

        GameObject proj = Instantiate(projectilePrefab, shootPos.position, Quaternion.identity);
        Vector2 shootDirection = (player.position - shootPos.position).normalized;

        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.SetFlight(shootDirection, shootPos.position);
        }
    }

    protected override void Die()
    {
        if (deathCutscene != null)
        {
            enemySpawner.ResetSelf();
            deathCutscene.PlayCutscene();
        }
        base.Die();
    }
}