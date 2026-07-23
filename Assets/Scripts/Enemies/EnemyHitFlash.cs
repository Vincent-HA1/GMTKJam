using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Cache the SpriteRenderer and store its starting color
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Call this method whenever the enemy takes damage.
    /// </summary>
    public void Flash()
    {
        // If the enemy is already flashing, stop the previous routine so it doesn't get stuck red
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Set to flash color
        spriteRenderer.color = flashColor;

        // Wait for the duration
        yield return new WaitForSeconds(flashDuration);

        // Reset back to original color
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }
}