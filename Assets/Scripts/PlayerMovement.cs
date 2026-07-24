using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Action<float> Healed;
    public Action<float> Hit;
    public Action Death;
    public Action Jump;
    public Action PunchAction;
    public Action<float> CooldownChanged;
    public LayerMask groundLayer;

    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    public float Health
    {
        get
        {
            return health;
        }
    }

    [SerializeField] bool ignoreOnGround = false;
    [Header("References")]
    [SerializeField] Transform leftFootPoint;
    [SerializeField] Transform rightFootPoint;
    [SerializeField] Transform headPosition;
    [SerializeField] Transform bombThrowPos;
    [SerializeField] Vector2 footSize = new Vector2(0.25f, 0.12f);
    [SerializeField] BoxCollider2D attackHitbox;
    [SerializeField] GameObject crouchHurtbox;
    [SerializeField] GameObject bomb;


    [Header("Player Attributes")]
    [SerializeField] float maxHealth = 3;
    [SerializeField] float hitInvincibilityDuration = 2;

    [Header("Player Movement Attributes")]
    [SerializeField] float groundMoveSpeed = 6;
    [SerializeField] float maxAirAccelerationChangeRate = 5;
    [SerializeField] float jumpForce = 8.5f;
    [SerializeField] float extraJumpForce = 0.38f;
    [SerializeField] float gravityForce = -35;
    [SerializeField] float terminalNegativeVelocity = -45;
    [SerializeField] float platformBonkVelocityHit = 0.05f;
    [SerializeField] float maxVerticalVelocity = 15;
    [SerializeField] float jumpBufferTime = 0.15f;
    [SerializeField] float coyoteTime = 0.08f;
    [SerializeField] AnimationCurve airAccelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] bool useAccel = true; // toggle for air acceleration

    [Header("Player Attack Attributes")]
    [SerializeField] float attackBufferWindow = 0.2f;
    [SerializeField] float attackInputBufferWindow = 0.15f;

    [Header("Player Special Attack Attributes")]
    [SerializeField] float cooldownTime = 5;


    [Header("Player Hurt Attributes")]
    [SerializeField] float hurtTime = 0.5f;


    // Player Components
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    InputHandler inputHandler;
    BoxCollider2D boxCollider;

    //Platform Handling
    //PlatformToFollow platformToFollow;

    // Input & movement state
    Vector2 movementInput;
    float xMovement;
    float currentHorizontalDir;
    Vector2 platformDelta;

    // Velocity state
    float horizontalVelocity = 0f;
    float verticalVelocity = 0f;

    // Jump buffers
    float jumpBuffer = 0f;
    float coyoteBuffer = 0f;

    // Attack buffers
    float attackBuffer = 0;
    float attackInputBuffer = 0;
    float cooldown = 0;

    //Hurt Timers
    float hurtTimer = 0;
    float invincibilityTimer = 0;

    Vector2 additiveForce;
    float additiveForcePercentage;

    // State flags
    public bool onGround;//{ get; private set; } = false;
    bool canActivateCoyote = false;
    bool isJumping = false;
    bool storingJumpInput = false;
    bool stoppedHoldingJump = false;
    bool crouching = false;
    bool throwingBomb = false;


    // Attacking states
    bool attacking = false;
    bool attackFinished = false;


    // hurt states
    bool hurt = false;
    bool dead = false;

    bool blockHorizontalMovement = false;
    float health;

    public void ReachedEndOfLevel()
    {
        StartCoroutine(EndAnimation());
    }

    IEnumerator EndAnimation()
    {
        //Prevent player from moving and play the victory animation after they land
        blockHorizontalMovement = true;
        verticalVelocity = 0;
        movementInput = new Vector2();
        //For now
        anim.SetFloat("Speed", 0);
        yield return new WaitUntil(() => onGround);
        anim.SetBool("Victory", true);
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

    }


    void Awake()
    {
        // Cache component references
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer.color = Color.white;
        health = maxHealth;
        anim.SetFloat("Horizontal", 1); //start by facing right
    }

    void Update()
    {
        CheckForSurfaces();
        if (GameManager.cannotAct || dead) return;
        // Update animator parameters
        UpdateAnimations();
        //if (GameManager.cannotAct && !dead) return;
        CheckHurtTimer();
        ManageHitboxDirection();
        CheckInvincibilityTimer();
        InvincibilityFlash();
        if (hurt) return;
        // Ground check and timers
        UpdateCoyoteBuffer();
        UpdateJumpBuffer();
        CheckAttackBuffer();
        CheckForCrouch();
        CheckForBombThrow();
        //CheckKickTimer();
        // Process movement input and potentially trigger jump
        ProcessMovementInput();
    }


    void LateUpdate()
    {
        if (GameManager.cannotAct) return;
        // Handle horizontal acceleration in air or direct ground movement
        UpdateHorizontalMovement();
    }

    void FixedUpdate()
    {
        if (GameManager.cannotAct) return;
        // Apply physics-based movement
        ApplyMovement();
    }

    // ----------------------------------------
    // Ground & Coyote
    // ----------------------------------------

    void CheckForSurfaces()
    {
        //Vector3 offset = platformToFollow == null ? Vector2.zero : new Vector2(0, -0.1f);
        //bool left = Physics2D.OverlapBox(leftFootPoint.position + offset, footSize, 0, groundLayer);
        //bool right = Physics2D.OverlapBox(rightFootPoint.position + offset, footSize, 0, groundLayer);
        onGround = CheckForSpecificGroundLayer(groundLayer, footSize);
        //test
        //ignoreOnGround = platformToFollow != null;
        //onGround = ignoreOnGround ? true : onGround;
        if (onGround)
        {
            canActivateCoyote = true;
            stoppedHoldingJump = false;
            if (verticalVelocity <= 0)
            {
                additiveForce = Vector2.zero;
            }
        }
    }

    bool CheckForSpecificGroundLayer(LayerMask layerToCheck, Vector2 boxSize)
    {
        Vector3 offset = new Vector2(0, -0.1f);//platformToFollow == null ? Vector2.zero : new Vector2(0, -0.1f);
        bool left = Physics2D.OverlapBox(leftFootPoint.position + offset, boxSize, 0, layerToCheck);
        bool right = Physics2D.OverlapBox(rightFootPoint.position + offset, boxSize, 0, layerToCheck);
        return left || right;
    }

    void UpdateCoyoteBuffer()
    {
        // Start coyote time when leaving ground
        if (!onGround && !isJumping && canActivateCoyote)
        {
            coyoteBuffer = coyoteTime;
            canActivateCoyote = false;
        }
        coyoteBuffer -= Time.deltaTime;
    }

    // ----------------------------------------
    // Jump Buffer & Input
    // ----------------------------------------

    void UpdateJumpBuffer()
    {
        // Detect jump press and release
        if (isJumping && !inputHandler.jumpHeld)
        {
            stoppedHoldingJump = true;
        }

        //If the buffer has ended, stop storing the input
        if (jumpBuffer <= 0f)
        {
            storingJumpInput = false;
        }

        // Buffer the jump press so the timing for a jump is not as strict
        if (inputHandler.jumpPressed)
        {
            jumpBuffer = jumpBufferTime;
            storingJumpInput = true;
        }




        jumpBuffer -= Time.deltaTime;
    }

    // ----------------------------------------
    // Input Processing & Jump Trigger
    // ----------------------------------------

    void ProcessMovementInput()
    {
        if (attacking || crouching || throwingBomb) return;
        //if (kicking || attacking) return;
        movementInput = new Vector2(inputHandler.movement.x, 0f);
        // Handle jump action if in buffer and within coyote time
        if (storingJumpInput && (onGround || coyoteBuffer > 0f) && !isJumping)
        {
            PerformJump();
            //if (!canKick && !inWater) //only renable kicking when jumping from ground to air
            //{
            //    canKick = true;
            //    ShowFlash();
            //}
        }
        else if (onGround && (!inputHandler.jumpHeld || storingJumpInput) && verticalVelocity <= 0f)
        {
            // Reset jump once button has been released (or if the button is being held, it is within the jump buffer for now)
            isJumping = false;
        }


        // On ground or input zero, snap horizontal direction to input
        if (onGround || movementInput.x == 0f)
            xMovement = movementInput.x;
    }

    void PerformJump(bool jumpWithFullForce = false, bool playSFX = true)
    {
        if (playSFX) Jump?.Invoke();
        //if jump with full force is true, just do the jump regardless of where the player is
        verticalVelocity = jumpForce;
        isJumping = true;
        storingJumpInput = false;

        //Get out of any child things, e.g. moving platforms
        //ExitPlatform();
    }

    // ----------------------------------------
    // Attacking 
    // ----------------------------------------

    void Attack()
    {
        //If not attacking, start the attack
        if (!attacking)
        {
            PunchAction?.Invoke();
            attacking = true;
            attackFinished = false;
        }
        else if (attacking)
        {
            // If the window to start another attack is still open, and the previous attack has finished
            if (attackBuffer > 0 && attackFinished)
            {
                PunchAction?.Invoke();
                //Attack again
                anim.SetTrigger("Attack");
                attackFinished = false;
            }
            else if (!attackFinished)
            {
                //Otherwise, store the input for now (because the current attack hasn't finished)
                attackInputBuffer = attackInputBufferWindow;
            }
        }

    }

    void CheckAttackBuffer()
    {
        // If on the ground, it is possible to attack. The is jumping check is to ensure that the player isn't holidng space (whihc is why they wouldnt be able to jump)
        if (inputHandler.attackPressed && onGround && (!isJumping || inputHandler.jumpHeld) && !crouching && !throwingBomb)
        {
            Attack();
        }
        if (attacking)
        {
            //If attacking, when the window for the next attack runs out, stop attacking
            if (attackBuffer <= 0 && attackFinished)
            {
                attacking = false;
                attackInputBuffer = 0;
            }
            attackBuffer -= Time.deltaTime;
        }
        //Decrement the input buffer regardless of anything
        attackInputBuffer -= Time.deltaTime;

    }

    // Called from the animation event at the end of the attack animation
    public void AttackFinished()
    {
        attackHitbox.enabled = false;
        attackFinished = true;
        attackBuffer = attackBufferWindow; //allow the window for the next attack
        //If an input has already come through (and is within the buffer), attack again
        if (attackInputBuffer > 0)
        {
            Attack();
        }
    }

    void ManageHitboxDirection()
    {
        attackHitbox.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(180 - anim.GetFloat("Horizontal") * 180, 0, 180), 0);

    }

    // Called from animation event
    public void EnableHitbox()
    {
        attackHitbox.enabled = true;
    }

    // ----------------------------------------
    // Crouching 
    // ----------------------------------------

    void CheckForCrouch()
    {
        //If holding down
        if(inputHandler.movement.y < 0 && !isJumping && onGround)
        {
            crouching = true;
            //change collider
            boxCollider.enabled = false;
            crouchHurtbox.SetActive(true);
        }
        else
        {
            crouching = false;
            boxCollider.enabled = true;
            crouchHurtbox.SetActive(false);
        }
    }

    // ----------------------------------------
    // Special Attack 
    // ----------------------------------------

    
    void CheckForBombThrow()
    {
        cooldown -= Time.deltaTime;
        CooldownChanged?.Invoke(cooldown);
        bool canThrowBomb = (onGround && (!inputHandler.jumpHeld || storingJumpInput) && verticalVelocity <= 0f);
        if (cooldown <= 0 && inputHandler.specialPressed && canThrowBomb)
        {
            //Throw the bomb
            StartBombThrow();
        }
    }

    void StartBombThrow()
    {
        throwingBomb = true;
        attacking = true;
        cooldown = cooldownTime; //set later
    }

    public void ThrowBomb()
    {
        GameObject b = Instantiate(bomb, null);
        b.GetComponent<Bomb>().SetDirection(anim.GetFloat("Horizontal"), bombThrowPos.position);
    }

    public void EndBombThrow()
    {
        throwingBomb = false;
        attacking = false;
    }

    // ----------------------------------------
    // Animation Updates
    // ----------------------------------------

    void UpdateAnimations()
    {
        // Horizontal movement parameters
        if (movementInput.x != 0f)
            anim.SetFloat("Horizontal", movementInput.x);
        anim.SetFloat("Speed", Mathf.Abs(movementInput.x));
        if (movementInput.x != 0f)
            spriteRenderer.flipX = movementInput.x < 0f;

        anim.SetBool("Crouching", crouching);

        // Jumping & falling states
        anim.SetBool("Jumping", isJumping && verticalVelocity > 0f);
        anim.SetBool("Falling", verticalVelocity < 0f && !onGround);

        // Attacking states
        anim.SetBool("Attacking", attacking);
        anim.SetBool("ThrowingBomb", throwingBomb);
        anim.SetBool("Hurt", hurt);
        anim.SetBool("Dead", dead);

    }

    // ----------------------------------------
    // Hurt
    // ----------------------------------------


    void CheckHurtTimer()
    {
        if (hurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                hurt = false;
                //enable invincibility
                invincibilityTimer = hitInvincibilityDuration;
                invincible = true;
            }
        }
    }

    bool invincible = false;
    void CheckInvincibilityTimer()
    {
        if (invincible)
        {
            if (invincibilityTimer <= 0)
            {
                invincible = false;
            }
            invincibilityTimer -= Time.deltaTime;
        }
    }

    // ----------------------------------------
    // Platform Logic
    // ----------------------------------------
    public void SetPlatformDelta(Vector2 movement)
    {
        platformDelta = movement;
    }

    //void ExitPlatform()
    //{
    //    if (platformToFollow)
    //    {
    //        platformToFollow.Disengage();
    //        platformToFollow = null;
    //    }
    //    platformDelta = Vector2.zero;
    //}

    // ----------------------------------------
    // Flash
    // ----------------------------------------

    void ShowFlash()
    {
        StartCoroutine(ControlFlashTiming());
    }

    IEnumerator ControlFlashTiming()
    {
        var mat = spriteRenderer.material;
        // 0 = normal, 1 = full white
        mat.SetFloat("_Flash", 1.0f);
        yield return new WaitForSeconds(0.05f);
        mat.SetFloat("_Flash", 0f);
    }

    float startAlpha = 1;
    float endAlpha = 0;
    float alphaLerp;
    void InvincibilityFlash()
    {
        if (invincible)
        {
            if (alphaLerp >= 1)
            {
                alphaLerp = 0;
                float tempAlpha = startAlpha;
                startAlpha = endAlpha;
                endAlpha = tempAlpha;
            }
            //set alpha to and fro
            spriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(startAlpha, endAlpha, alphaLerp));
            alphaLerp += 20 * Time.deltaTime;
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            startAlpha = 1;
            endAlpha = 0;
        }
    }

    // ----------------------------------------
    // Death
    // ----------------------------------------
    void Die()
    {
        //hurt = true;
        anim.SetTrigger("Death");
        dead = true;
        anim.SetBool("Dead", dead);
        invincible = false;
        ResetStates();
        //inWater = false;
        boxCollider.enabled = false;
    }

    //Set after the death animation (to trigger any level end effects)
    public void SetDead()
    {
        print("set dead");
        Death?.Invoke();
    }

    void ResetStates(bool resetVelocity = true)
    {
        isJumping = false;
        stoppedHoldingJump = false;
        crouching = false;
        movementInput = Vector2.zero;
        if (resetVelocity) verticalVelocity = 0;
        attacking = false;
        //if (!inWater || kicking) StopKick();
    }

    // ----------------------------------------
    // Horizontal Movement in Air & Ground
    // ----------------------------------------

    void UpdateHorizontalMovement()
    {
        // A test bool to ignore the air acceleration changes
        if (!useAccel)
        {
            xMovement = movementInput.x;
            currentHorizontalDir = movementInput.x;
            return;
        }

        // If changing direction mid-air, reset momentum (so changing direction stops the movement immediately)
        if (currentHorizontalDir != movementInput.x)
            xMovement = 0f;

        // If the current direction of movement is different to the player's input, accelerate towards the correct direction (not immediate)
        if (xMovement != movementInput.x)
        {
            //Use the difference as an input for the curve I want for the acceleration
            float speedRatio = Mathf.Clamp01(Mathf.Abs(xMovement));
            float curveValue = airAccelCurve.Evaluate(speedRatio);
            //Accelerate by the value on the curve. The curve starts slow and then spikes (so after a short period, acceleration is instant)
            float accelThisFrame = curveValue * maxAirAccelerationChangeRate;
            xMovement += Mathf.Sign(movementInput.x) * accelThisFrame * Time.deltaTime;
            // Clamp to full input once exceeded
            if (Mathf.Abs(xMovement) > 1f)
                xMovement = movementInput.x;
        }

        currentHorizontalDir = movementInput.x;
    }

    // ----------------------------------------
    // Physics Movement
    // ----------------------------------------

    void ApplyMovement()
    {
        Vector2 finalMovement = new Vector2();
        //Only add new movement if not attacking
        if (!attacking)
        {

        }
        // Extra jump force when holding jump (only if they haven't let go yet). Don't apply when above max velocity
        if (inputHandler.jumpHeld && verticalVelocity > 0f && !stoppedHoldingJump && verticalVelocity < maxVerticalVelocity)
        {
            verticalVelocity += extraJumpForce;
        }

        //Decide on stats based on if in water or not (or attacking)
        float moveSpeed = attacking || crouching || throwingBomb ? 0 : groundMoveSpeed;
        float gravityValue = gravityForce;
        float maximumNegativeVelocity = terminalNegativeVelocity;

        // Horizontal velocity is directly from xMovement (unless on ice). If kicking, then move forwards according to the kick speed
        //horizontalVelocity = !kicking ? moveSpeed * xMovement : horizontalVelocity;
        //horizontalVelocity = inKickLag ? 0 : horizontalVelocity;

        horizontalVelocity = moveSpeed * xMovement;

        // Compute displacements (SUVAT)
        // If on ice, accelerate into chosen direction
        float dx = horizontalVelocity * Time.fixedDeltaTime;


        //Don't move downwards if on the ground or kicking
        bool groundedAndNotRisingOrKicking = (verticalVelocity <= 0f && onGround);
        float dy = groundedAndNotRisingOrKicking ?
            0f :
            verticalVelocity * Time.fixedDeltaTime + 0.5f * gravityValue * Time.fixedDeltaTime * Time.fixedDeltaTime;

        // Update vertical velocity (SUVAT), assuming initial velocity is 0. If on the ground, velocity is automatically 0
        verticalVelocity = groundedAndNotRisingOrKicking
            ? 0f
            : Mathf.Max(maximumNegativeVelocity, verticalVelocity + gravityValue * Time.fixedDeltaTime);
        //if (kicking)
        //{
        //    float speedRatio = Mathf.Clamp01(1 - kickTimer / kickLength);
        //    float curveValue = kickAccelCurve.Evaluate(speedRatio);
        //    //Accelerate by the value on the curve. The curve starts slow and then spikes (so after a short period, acceleration is instant)
        //    float accelThisFrame = curveValue * kickSlowdownRate * anim.GetFloat("Horizontal");
        //    //Make sure to move the velocity to 0 from the correct direction when kicking
        //    horizontalVelocity = anim.GetFloat("Horizontal") * kickInitialSpeed * curveValue;
        //}
        //If no horizontal movement, block it
        if (blockHorizontalMovement) dx = 0;
        finalMovement = new Vector2(dx, dy);

        //Platform movement
        //if (platformToFollow)
        //{
        //    finalMovement += platformDelta;
        //}

        //Additive force falloff
        //additiveForcePercentage = Mathf.Max(0, additiveForcePercentage - Time.deltaTime * additiveForceFalloff);
        //finalMovement += additiveForce * additiveForcePercentage; //add any additional force.

        // Apply movement to Rigidbody2D
        rigid.MovePosition(rigid.position + finalMovement);
    }

    void GetHit()
    {
        //Get hit and force the player down (so halt all their movement)
        health -= 1;
        hurtTimer = hurtTime;
        hurt = true;
        ResetStates();
        Hit?.Invoke(health);
        if (health <= 0)
        {
            Die();
        }

    }

    public void Respawn(Vector3 respawnPosition)
    {
        //Puts the player back to the respawn position
        transform.position = respawnPosition;
        health = maxHealth;
        hurt = false;
        ResetStates();
        boxCollider.enabled = true;
        hurtTimer = 0;
        dead = false;
    }

    void Heal(float healAmount)
    {
        health = Mathf.Clamp(health + healAmount, 0, maxHealth);
        Healed?.Invoke(health);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Health"))
        {
            Heal(1); //heal 1 for now
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Gap") && !GameManager.cannotAct) //so if not already dead
        {
            Die();
        }
        if (collision.CompareTag("Enemy") && !hurt && !invincible)//invincibleTimer <= 0)
        {
            //get hit
            GetHit();
        }

        //Platform logic
        //if (collision.CompareTag("MovingPlatform") && platformToFollow == null && onGround)
        //{
        //    print("Touch moving platform");
        //    //Set the moving platform
        //    platformToFollow = collision.GetComponentInParent<PlatformToFollow>();
        //    platformToFollow.SetPlayer(this);
        //}
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if (collision.CompareTag("MovingPlatform") && platformToFollow)
        //{
        //    ExitPlatform();
        //}
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //CHECK FOR COLLISIONS WITH SOLID SURFACES
        //Check if this is ground
        int otherLayer = collision.gameObject.layer;
        if ((groundLayer.value & (1 << otherLayer)) == 0) return;

        // guard: expect one contact but handle 0 defensively
        if (collision.contactCount == 0)
            return;
        // use the single contact (no averaging)
        ContactPoint2D contact = collision.GetContact(0);
        const float threshold = 0.5f;
        Vector2 normal = contact.normal; // points from the other collider -> this collider
        if (normal.y > 0) //For standing on top of objects, there is no threshold
        {
            // landed on top of the object (ground under us)
            Debug.Log("Collision from above (landed on top of object).");
        }
        else if (normal.y < -threshold) //For bumping head on objects
        {
            // hit underside (head bump, so stop the rising input)
            Debug.Log("Collision from below (hit head).");
            inputHandler.jumpHeld = false;
            if (verticalVelocity > 0)
            {
                //Make the player lose velocity somewhat (don't want to float under the platform)
                verticalVelocity -= maxVerticalVelocity * platformBonkVelocityHit;
            }
            //verticalVelocity = 0;
        }

    }
}

