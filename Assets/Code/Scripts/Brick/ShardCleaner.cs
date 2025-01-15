using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardCleaner : MonoBehaviour
{
    private LayerMask layerMask;
    private WaitForSeconds wait = new WaitForSeconds(3f);
    private void Awake()
    {
        layerMask = LayerMask.GetMask("Brick");
    }


    private void OnTriggerExit(Collider other)
    {
        if ((layerMask.value & 1 << other.gameObject.layer) > 0)
        {
            StartCoroutine(WaitDestroy(other.gameObject));
        }
    }
    private IEnumerator WaitDestroy(GameObject shard)
    {
        yield return wait;
        shard.gameObject.SetActive(false);
    }
}
