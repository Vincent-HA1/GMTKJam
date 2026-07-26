using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject hitbox;
    [SerializeField] GameObject bombSoundEffect;

    [Header("Throw Motion Settings")]
    [SerializeField] private float horizontalSpeed = 8f;     // Horizontal speed
    [SerializeField] private float initialUpwardForce = 10f; // Initial upward arc height
    [SerializeField] private float gravity = 25f;            // Pull down strength

    private Vector2 currentVelocity;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool hasExploded = false;

    bool setDirection = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDirection(float direction, Vector2 position)
    {
        currentVelocity = new Vector2(horizontalSpeed * direction, initialUpwardForce);
        rb.position = position;
        setDirection = true;
        StartCoroutine(WaitForDelay());
    }

    IEnumerator WaitForDelay()
    {
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.enabled = true;
    }

    // Always use FixedUpdate when moving Rigidbody2D components!
    private void FixedUpdate()
    {
        if (hasExploded || !setDirection) return;

        // 1. Apply gravity to the Y velocity over physics time
        currentVelocity.y -= gravity * Time.fixedDeltaTime;

        // 2. Calculate next target position
        Vector2 targetPosition = rb.position + (currentVelocity * Time.fixedDeltaTime);

        // 3. Move smoothly via Rigidbody2D
        rb.MovePosition(targetPosition);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartExplosion();
        }
    }

    void StartExplosion()
    {
        hasExploded = true;
        currentVelocity = Vector2.zero; // Freeze motion
        anim.SetTrigger("Explode");
        Instantiate(bombSoundEffect);
        circleCollider.enabled = false;
    }

    public void Explode()
    {
        hitbox.SetActive(true);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}