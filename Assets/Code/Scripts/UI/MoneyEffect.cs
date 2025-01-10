
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyEffect : MonoBehaviour
{

    public void InitializeMoneyIcon()
    {
        gameObject.SetActive(true);
    }
    private void OnEnable()
    {
        transform.DOMoveY(transform.position.y + 2, 1).OnComplete(() => gameObject.SetActive(false));
    }



}
