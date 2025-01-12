using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickHealth : MonoBehaviour
{
    public float health = 1f;
    private float currentHealth;
    [HideInInspector] public float percentage;
    private ShatterBrickManager shatterBrickManager;
    private void Awake()
    {
        shatterBrickManager = GetComponent<ShatterBrickManager>();
    }
    private void Start()
    {
        currentHealth = health;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        percentage = currentHealth / health;
        shatterBrickManager.ActiveBrickSection();
        if (health <= 0)
        {
            Destroy(gameObject);
            BrickCount();
        }
    }
    private void BrickCount()
    {
        Settings.brickCount++;

        if (Settings.brickCount == 6)
        {
            GameManager.Instance.HandleGameState(GameState.Win);
        }
    }
}
