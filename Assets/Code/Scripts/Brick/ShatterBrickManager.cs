
using RayFire;
using System.Collections;
using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ShatterBrickManager : MonoBehaviour
{
    private BrickHealth brickHealth;
    private LayerMask layerMask;
    private RayfireActivator rayfireActivator;
    [SerializeField] private BrickSO brickSO;
    [SerializeField] private GameObject brickDamage;
    [SerializeField] private float radius;
    [SerializeField] private float offset = 2f;
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
        if (!Settings.isTrigger)
            return;
        if ((layerMask.value & 1 << other.gameObject.layer) > 0)
        {
            Settings.isTrigger = false;
            brickHealth.TakeDamage(GameManager.Instance.playerManager.atk);
            ShowDamage(other);
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
        StaticEventHandler.CallOnBrickDestroy(brickHealth.percentage);
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
        damage.GetComponent<TextMeshPro>().text = GameManager.Instance.playerManager.atk.ToString("F2"); ;
    }


}
