
using UnityEngine;

public class ShatterBrickManager : MonoBehaviour
{
    private BrickHealth brickHealth;
    private LayerMask layerMask;

    private void Awake()
    {
        brickHealth = GetComponent<BrickHealth>();
    }
    private void Start()
    {
        layerMask = LayerMask.GetMask("Hand");
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & 1 << other.gameObject.layer) > 0)
        {
            gameObject.SetActive(false);
            brickHealth.TakeDamage(GameManager.Instance.playerManager.playerAtributes.atk);
        }
    }
}
