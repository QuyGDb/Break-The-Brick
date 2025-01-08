using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class ResourcePointBehavior : MonoBehaviour
    {
        public static ResourceCarrierType[] CARRIER_TYPES = (ResourceCarrierType[])Enum.GetValues(typeof(ResourceCarrierType));

        [SerializeField] ResourceCarrierType carrierType = ResourceCarrierType.Player | ResourceCarrierType.Helper;

        /// <summary>
        /// The amount of time (in seconds) the object is 'sleeping' after taking one resource
        /// </summary>
        [SerializeField] protected float cooldown = 0.1f;

        protected abstract void AddResourceCarrier(GameObject carrierObject);
        protected abstract void RemoveResourceCarrier(GameObject carrierObject);

        private void OnTriggerEnter(Collider other)
        {
            for(int i = 0; i < CARRIER_TYPES.Length; i++)
            {
                var testType = CARRIER_TYPES[i];
                if ((carrierType & testType) != 0)
                {
                    if (other.gameObject.layer == GetCarrierlayer(testType))
                    {
                        AddResourceCarrier(other.gameObject);
                        break;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < CARRIER_TYPES.Length; i++)
            {
                var testType = CARRIER_TYPES[i];

                if ((carrierType & testType) != 0)
                {
                    if (other.gameObject.layer == GetCarrierlayer(testType))
                    {
                        RemoveResourceCarrier(other.gameObject);
                        break;
                    }
                }
            }
        }

        protected static int GetCarrierlayer(ResourceCarrierType carrierType)
        {
            switch(carrierType)
            {
                case ResourceCarrierType.Helper: return PhysicsHelper.LAYER_HELPER;
                case ResourceCarrierType.Player: return PhysicsHelper.LAYER_CHARACTER;
                default: return 0;
            }
        } 
    }
}