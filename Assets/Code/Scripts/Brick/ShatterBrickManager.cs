
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
            Debug.Log("OnTriggerEnter");
            brickHealth.TakeDamage(TestDamage);

        }
    }
    public void ActiveBrickSection()
    {
        Debug.Log("ActiveBrickSection");
        float distanceToMove = brickHealth.percentage * (brickSO.topPosition - brickSO.bottomPosition);
        float newPosition = brickSO.topPosition - distanceToMove;
        rayfireActivator.transform.localPosition = new Vector3(rayfireActivator.transform.position.x, newPosition, rayfireActivator.transform.position.z);
        StaticEventHandler.CallOnBrickDestroy();
    }



}
