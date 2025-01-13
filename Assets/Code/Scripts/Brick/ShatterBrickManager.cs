
using RayFire;
using UnityEngine;

public class ShatterBrickManager : MonoBehaviour
{
    private BrickHealth brickHealth;
    private LayerMask layerMask;
    private RayfireActivator rayfireActivator;
    [SerializeField] private BrickSO brickSO;
    private void Awake()
    {
        brickHealth = GetComponent<BrickHealth>();
        rayfireActivator = GetComponentInChildren<RayfireActivator>();
    }
    private void Start()
    {
        layerMask = LayerMask.GetMask("Hand");
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & 1 << other.gameObject.layer) > 0)
        {
            brickHealth.TakeDamage(GameManager.Instance.playerManager.atk);
        }
    }
    public void ActiveBrickSection()
    {
        float distanceToMove;
        if (brickHealth.percentage <= 0)
        {
            distanceToMove = brickSO.topPosition - brickSO.bottomPosition;
        }
        else if (brickHealth.percentage >= 1)
        {
            distanceToMove = 0;
        }
        else
        {
            distanceToMove = (1 - brickHealth.percentage) * (brickSO.topPosition - brickSO.bottomPosition);
        }

        float newPosition = brickSO.topPosition - distanceToMove;
        rayfireActivator.transform.localPosition = new Vector3(0, newPosition, 0);
        StaticEventHandler.CallOnBrickDestroy();
    }



}
