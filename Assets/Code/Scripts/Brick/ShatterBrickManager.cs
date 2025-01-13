
using RayFire;
using UnityEngine;

public class ShatterBrickManager : MonoBehaviour
{
    private BrickHealth brickHealth;
    private LayerMask layerMask;
    [SerializeField] private RayfireActivator rayfireActivator;
    [SerializeField] private BrickSO brickSO;
    public float TestDamage = 1f;
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
            brickHealth.TakeDamage(TestDamage);

        }
    }
    public void ActiveBrickSection()
    {
        float distanceToMove;
        if (brickHealth.percentage <= 0)
        {
            distanceToMove = brickSO.topPosition - brickSO.bottomPosition;
        }
        else
        {
            distanceToMove = brickHealth.percentage * (brickSO.topPosition - brickSO.bottomPosition);
        }

        Debug.Log("newPosition: " + (brickSO.topPosition - distanceToMove) + Time.frameCount);
        float newPosition = brickSO.topPosition - distanceToMove;
        rayfireActivator.transform.localPosition = new Vector3(0, newPosition, 0);
        StaticEventHandler.CallOnBrickDestroy();
    }



}
