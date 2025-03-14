
using RayFire;
using System.Collections;
using TMPro;
using UnityEngine;

public class BrickDestructionManager : MonoBehaviour
{
    private BrickHealth brickHealth;
    private LayerMask layerMask;
    private RayfireActivator rayfireActivator;
    [SerializeField] private BrickSO brickSO;
    [SerializeField] private GameObject brickDamage;
    [SerializeField] private float radius;
    [SerializeField] private float offset = 2f;
    private BombManager bombManager;
    public BrickType brickType;
    private PlayerManager playerManager;
    private void Awake()
    {
        brickHealth = GetComponent<BrickHealth>();

        rayfireActivator = GetComponentInChildren<RayfireActivator>();
        bombManager = GetComponentInChildren<BombManager>();
    }
    private void OnEnable()
    {
        StaticEventHandler.OnPlayerManager += StaticEventHandler_OnPlayerManager;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnPlayerManager -= StaticEventHandler_OnPlayerManager;
    }

    private void StaticEventHandler_OnPlayerManager(PlayerManager manager)
    {
        playerManager = manager;
    }
    private void Start()
    {
        layerMask = LayerMask.GetMask("Hand");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!Settings.isTrigger)
            return;
        if ((layerMask.value & 1 << other.gameObject.layer) > 0)
        {
            Settings.isTrigger = false;
            if (brickType == BrickType.Explosive)
            {
                brickHealth.TakeDamage(100000f);
                GameManager.Instance.HandleGameState(GameState.Lose);
            }
            else
            {
                brickHealth.TakeDamage(playerManager.atributes.atk);
            }
            ShowDamage(other);
            ShowEffect(other);
            StaticEventHandler.CallOnRotatePlatform(false);
            StopAllCoroutines();
            StartCoroutine(WaitBrickDestroy());
        }
    }

    private IEnumerator WaitBrickDestroy()
    {
        yield return new WaitForSeconds(0.75f);
        StaticEventHandler.CallOnRotatePlatform(true);
    }

    public void ActiveBrickSection(float percentage)
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
            distanceToMove = brickHealth.percentage * (brickSO.topPosition - brickSO.offsetPosition);
        }

        float newPosition = brickSO.topPosition - distanceToMove;
        rayfireActivator.transform.localPosition = new Vector3(0, newPosition, 0);
        StaticEventHandler.CallOnBrickDestroy(brickHealth.percentage);
        bombManager.HandleBrickDestroy(brickHealth.percentage);
    }
    private void ShowDamage(Collider collider)
    {
        Vector3 randomPosition = new Vector3(
     transform.position.x + Random.Range(-radius, radius),
     transform.position.y + offset,
     transform.position.z + Random.Range(-radius, radius));
        GameObject damage = Instantiate(brickDamage, randomPosition, Quaternion.Euler(new Vector3(0, 0, 0)));

        if (GameManager.Instance.gameState == GameState.FirstPerson)
        {
            damage.transform.rotation = Quaternion.Euler(new Vector3(60f, 0, 180f));
        }
        damage.GetComponent<TextMeshPro>().text = HelperUtilities.ToShortString(playerManager.atributes.atk);
    }

    private void ShowEffect(Collider collider)
    {
        Vector3 randomPosition = new Vector3(
    transform.position.x + Random.Range(-radius, radius),
    transform.position.y + offset,
    transform.position.z + Random.Range(-radius, radius));

    }
}
