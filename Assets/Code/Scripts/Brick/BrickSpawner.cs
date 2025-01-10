using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public GameObject brickToSpawn;
    public int numberOfbricks = 6;
    public float radius = 5f;
    public float yPostionOfBrick = 1f;

    void Start()
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
