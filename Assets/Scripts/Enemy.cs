using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float health, maxHealth = 3f;
    [SerializeField] private GameObject enemyShip;
    void Start()
    {
        health = maxHealth;
    }
    //take damage
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if(health <= 0){
            Destroy(enemyShip);
        }
    }
}
