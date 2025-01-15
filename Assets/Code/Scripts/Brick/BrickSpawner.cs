using System;
using Unity.VisualScripting;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public GameObject brickToSpawn;
    public int numberOfbricks = 6;
    public float radius = 5f;
    public float yPostionOfBrick = 1f;
    [HideInInspector] public int brickCount = 0;
    public bool isRotating = false;
    private void OnEnable()
    {
        StaticEventHandler.OnBrickDie += BrickCount;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnBrickDie -= BrickCount;
    }

    private void BrickCount()
    {
        brickCount++;
        Debug.Log("Brick Count: " + brickCount);
        StaticEventHandler.CallOnBrickCount(brickCount, numberOfbricks);
        if (brickCount == numberOfbricks)
        {
            GameManager.Instance.HandleGameState(GameState.Win);
        }
    }

    void Awake()
    {
        SpawnBrickInPlatform();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBrickInPlatform();
        }
    }

    void SpawnBrickInPlatform()
    {
        for (int i = 0; i < numberOfbricks; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfbricks;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 position = new Vector3(x, yPostionOfBrick, z) + transform.position;
            Instantiate(brickToSpawn, position, Quaternion.identity, transform);
        }
    }
}
