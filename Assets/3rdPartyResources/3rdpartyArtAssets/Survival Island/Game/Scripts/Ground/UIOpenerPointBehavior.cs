using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public class UIOpenerPointBehavior : MonoBehaviour
    {
        [SerializeField] UIPage pageToOpen;

        private bool IsPlayerInside { get; set; }
        private bool HasPageBeenOpened { get; set; }

        private void Update()
        {
            if(IsPlayerInside && !HasPageBeenOpened)
            {
                UIController.ShowPage(pageToOpen);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_CHARACTER)
            {
                IsPlayerInside = true;
                HasPageBeenOpened = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_CHARACTER)
            {
                IsPlayerInside = false;
                HasPageBeenOpened = false;
            }
        }
    }
}