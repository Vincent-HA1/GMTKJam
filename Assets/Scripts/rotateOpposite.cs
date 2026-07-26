using UnityEngine;

public class RotateCounterClockwise : MonoBehaviour
{
    public float rotationSpeed = -360f; 

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}