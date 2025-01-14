using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickHealth : MonoBehaviour
{
    public float health = 1f;
    [SerializeField] private float currentHealth;
    [HideInInspector] public float percentage;
    private ShatterBrickManager shatterBrickManager;

    private void Awake()
    {
        shatterBrickManager = GetComponent<ShatterBrickManager>();
    }
    private void Start()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState == GameState.FirstPerson)
        {

            health = GameLevel.Instance.firstPersonLevel;
            currentHealth = health;
        }
        if (gameState == GameState.ThirdPerson)
        {

            health = GameLevel.Instance.thirdPersonLevel;
            currentHealth = health;
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        percentage = currentHealth / health;
        shatterBrickManager.ActiveBrickSection();
        if (currentHealth <= 0)
        {
            StaticEventHandler.CallOnBrickDie();
        }
    }
}
