using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickHealth : MonoBehaviour
{
    public float health;
    [SerializeField] private float currentHealth;
    [HideInInspector] public float percentage;
    private BrickDestructionManager shatterBrickManager;

    private void Awake()
    {
        shatterBrickManager = GetComponent<BrickDestructionManager>();
    }
    private void Start()
    {
        if (GameManager.Instance.gameState == GameState.FirstPerson)
        {
            health = Settings.FirstPersonLevel;
            currentHealth = health;
        }
        else if (GameManager.Instance.gameState == GameState.ThirdPerson)
        {
            health = Settings.ThirdPersonLevel;
            currentHealth = health;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        percentage = currentHealth / health;
        shatterBrickManager.ActiveBrickSection(percentage);
        if (currentHealth <= 0)
        {
            StaticEventHandler.CallOnBrickDie();
        }
    }
}
