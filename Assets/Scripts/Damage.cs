using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [Header("Damage Value")]
    [SerializeField] float damageValue = 1;

    public void SetDamage(float damageValue)
    {
        this.damageValue = damageValue;
    }

    public float GetDamage()
    {
        return damageValue;
    }
}
