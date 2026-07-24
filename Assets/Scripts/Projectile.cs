using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [Header("Attributes")]
    [SerializeField] float moveSpeed = 6;

    [SerializeField] Rigidbody2D rigid;
    [SerializeField] SpriteRenderer spriteRenderer;
    Vector2 moveDir;

    bool destroying = false;

    private void Awake()
    {

    }



    public void SetFlight(Vector2 direction, Vector2 startPos)
    {
        moveDir = direction;
        transform.position = startPos;
        spriteRenderer.enabled = true;
    }

    private void FixedUpdate()
    {
        rigid.MovePosition(rigid.position + moveDir * moveSpeed * Time.deltaTime);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !destroying)
        {
            destroying = true;
            StartCoroutine(DestroyAfterDelay());
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        print("invisible");
        Destroy(gameObject);
    }
}
