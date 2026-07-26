using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] GameObject explosionSound;

    private void Start()
    {
        Instantiate(explosionSound);
    }
    // Run from animation event. Ends the explosion animation
    public void EndExplosion()
    {
        Destroy(gameObject);
    }
}
