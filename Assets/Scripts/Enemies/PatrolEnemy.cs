using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PatrolEnemy : BaseEnemy
{
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] protected Transform groundPos;

    [Header("Ground Enemy Attributes")]
    [SerializeField] protected float moveSpeed = 2;
    [SerializeField] protected float groundCheckXOffset = 0.6f;
    [SerializeField] protected float wallCheckDistance = 0.8f;
    [SerializeField] protected float playerCheckDistance = 4;
    [SerializeField] protected float minDistanceToPlayer = 0.3f;

    protected bool onGround = false;
    protected bool wallThere = false;
    protected bool hitOtherEnemy = false;
    protected Vector2 moveDirBeforeGettingHit;

    protected override void Update()
    {
        base.Update();
        float xOffset = groundCheckXOffset; //if jumping, dont use the offset
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundPos.position + new Vector3(xOffset * moveDirection.x, 0), 0.18f, groundLayer); //offset the check so dont go over the ledge
        int groundCount = groundColliders.Count(x => x.gameObject != this.gameObject); //Ignore all ground colliders that are on this gameobject
        onGround = groundCount > 0;
    }

    protected override void ResumeFromGettingHit()
    {
        base.ResumeFromGettingHit();
        //After getting hit, make sure to return back to normal functionality
        if (moveDirection == Vector2.zero)
        {
            //So continue moving like before
            moveDirection = moveDirBeforeGettingHit;
        }
    }

    protected override void DetectPlayer()
    {
        base.DetectPlayer();
        RaycastHit2D[] wallHitResults = new RaycastHit2D[1];
        int wallHitCount = boxCollider.Raycast(moveDirection, wallHitResults, wallCheckDistance, groundLayer);//RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDirection, wallCheckDistance, groundLayer);
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, playerLayer);
        wallThere = wallHitCount > 0;//wallHit.collider != null;
        if (!player && playerHit.collider != null)
        {
            player = playerHit.collider.transform;
        }
        if ((playerHit.collider != null || !playerTooFar && playerDetected) && CanMove()) //Can only detect player if can move
        {
            //Check if wall is not in between the player and the enemy by doing the same raycast for the player but for the ground now
            //RaycastHit2D wallPlayerCheck = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, groundLayer);
            wallHitCount = boxCollider.Raycast(moveDirection, wallHitResults, playerCheckDistance, groundLayer);
            float playerDistance = playerHit.collider == null ? 0 : Vector2.Distance(playerHit.point, transform.position);
            float wallDistance = Vector2.Distance(wallHitResults[0].point, transform.position);//wallPlayerCheck.point, transform.position);
            if (wallHitCount <= 0 || wallDistance > playerDistance)//wallPlayerCheck.collider == null || wallDistance > playerDistance)
            {
                if (!playerDetected)
                {
                    //So player in front of wall, so can detect now
                    playerDetected = true;
                    moving = true;
                }
            }
            //Check if wall now in between player and the enemy. If so, have to cancel the chase, regardless of distance
            else if (wallHitCount > 0 && wallDistance <= playerDistance)//wallPlayerCheck.collider != null && wallDistance <= playerDistance)
            {
                //Stop moving for now
                if (playerDetected)
                {
                    playerDetected = false;
                    moveTimer = 0;
                }
            }
        }
        else
        {
            //Stop moving for now, as player too far or cannot move over there (for real, not just in jump lag)
            if (playerDetected && ((!CanMove()) || playerTooFar))
            {
                playerDetected = false;
                moveTimer = 0;
            }
        }

        //Checking for other enemies
        List<RaycastHit2D> enemiesHit = Physics2D.RaycastAll(transform.position, moveDirection, 0.8f, enemyLayer).ToList();
        hitOtherEnemy = false;
        foreach (RaycastHit2D h in enemiesHit)
        {
            if (h.collider != null && h.collider.gameObject != gameObject)
            {
                // First valid non-self hit
                hitOtherEnemy = true;
            }
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        Vector2 difference = (player.position - transform.position);
        moveDirection = new Vector2(Mathf.Sign(difference.x), 0);
        //Move towards player while it is allowed (i.e. player is far away enough)
        if (CanMove() && Mathf.Abs(difference.x) > minDistanceToPlayer)
        {
            moving = true;
        }

        print("moving towards player");
    }

    protected override void Patrol()
    {
        if (!playerDetected)
        {
            if (moving && !CanMove())
            {
                //If run into something, change direction
                moveTimer = 0;
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    //Called when changing direction during patrolling.
    protected override void ChangeDirection()
    {
        //If hit a wall, then need to turn backwards. Otherwise, choose a random direction
        if (!CanMove())
        {
            moveDirection = -moveDirection;
        }
        else
        {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 0 ? moveDirection : -moveDirection;
        }
        base.ChangeDirection();
    }


    protected virtual bool CanMove()
    {
        //Returns the conditions for not being able to move
        return onGround && !wallThere && !hitOtherEnemy;
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ApplyMovement();
    }

    protected virtual void ApplyMovement()
    {
        if (!moving) return; //Don't do anything if standing still 

        float dx = (moveDirection * Time.fixedDeltaTime * moveSpeed).x;

        Vector2 finalMovement = new Vector2(dx, 0);//new Vector2(dx, dy);

        // Apply movement to Rigidbody2D
        rigid.MovePosition(rigid.position + finalMovement);
    }

    protected override void GetHit(float damage = 1)
    {
        base.GetHit(damage);
        moveDirBeforeGettingHit = moveDirection;
        moveDirection = Vector2.zero;
    }
}