using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//No patrolling, just shoots when it detects the enemy
public class ShootingEnemy : BaseEnemy
{
    [Header("Shooting Enemy References")]
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject enemyProjectile;


    [Header("Shooting Enemy Attributes")]
    [SerializeField] float playerCheckDistance = 4;
    [SerializeField] float minShootTime = 2;
    [SerializeField] float maxShootTime = 3;
    [SerializeField] Vector2 directionToFace;

    float shootTimer;

    protected override void Start()
    {
        base.Start();
        moveDirection = directionToFace; //Fixed
    }

    protected override void Update()
    {
        base.Update();
        ManageShootDirection();
        ManageShootTimers();
    }

    protected override void ManageMoveTimers() { } //No movement

    protected override void DetectPlayer()
    {
        base.DetectPlayer();
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, playerLayer);
        if (!player && playerHit.collider != null)
        {
            player = playerHit.collider.transform;
            playerDetected = true;
        }

        //Don't undetect player
    }

    /* Shooting Logic */

    void ManageShootDirection()
    {
        shootPos.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(180 - directionToFace.x * 180, 0, 180), 0);

    }

    void ManageShootTimers()
    {
        if (!playerDetected) return;
        shootTimer-=Time.deltaTime;
        if (shootTimer <= 0)
        {
            //Shoot
            Shoot();
        }
    }

    void Shoot()
    {
        RestartTimer();
        GameObject projectile = Instantiate(enemyProjectile);
        projectile.GetComponent<EnemyProjectile>().SetFlight(directionToFace, shootPos.position);
    }

    void RestartTimer()
    {
        shootTimer = UnityEngine.Random.Range(minShootTime, maxShootTime);
    }

    protected override void GetHit(float damage = 1)
    {
        base.GetHit(damage);
        RestartTimer(); //restart shooting on hit
    }
}
