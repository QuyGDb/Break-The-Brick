using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonneyManager : MonoBehaviour, IPointerClickHandler
{
    private float clickCooldown = 0.25f;
    private bool canClick = true;
    [SerializeField] private PlayerAtributes playerAtributes;
    [SerializeField] private GameObject moneyIcon;
    [SerializeField] PlayerAnimation playerAnimation;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClick)
        {
            playerAtributes.AddMoney();
            StartCoroutine(ClickCooldown());
            Component moneyEffect = PoolManager.Instance.ReuseComponent(moneyIcon, HelperUtilities.GetMouseWorldPosition3D(10), Quaternion.identity);
            moneyEffect.gameObject.SetActive(true);
            playerAnimation.TriggerChopAnim();
        }
    }

    private IEnumerator ClickCooldown()
    {
        canClick = false;
        yield return new WaitForSeconds(clickCooldown);
        canClick = true;
    }
}
