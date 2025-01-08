using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public abstract class BuildingBehavior : MonoBehaviour, IUnlockable, IWorldElement
    {
        public int InitialisationOrder => 2;

        public string ID { get; private set; }

        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField] protected List<BuildingUpgradeContainer> buildingUpgrades;
        public List<BuildingUpgradeContainer> BuildingUpgrades => buildingUpgrades;

        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField, ShowIf("EditorHaveUpgrades")] protected UpgradesTrigger upgradesTrigger;
        [BoxFoldout("Upgrades", "Upgrades")]
        [SerializeField, ShowIf("EditorHaveUpgrades")] protected bool showUpgradesOnMainPage;

        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] GameObject openedVisuals;
        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] GameObject closedVisuals;

        [BoxFoldout("Visuals", "Visuals")]
        [SerializeField] protected AnimationForUnlockable unlockAnimation;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        protected abstract void RegisterUpgrades();

        protected virtual void Init()
        {
            if (!buildingUpgrades.IsNullOrEmpty())
            {
                RegisterUpgrades();

                if (upgradesTrigger != null)
                {
                    upgradesTrigger.RegisterUpgrades(buildingUpgrades.ConvertAll((upgrade) => (IUpgrade)upgrade.Upgrade));
                }

                if (showUpgradesOnMainPage)
                {
                    for (int i = 0; i < buildingUpgrades.Count; i++)
                    {
                        GlobalUpgradesController.RegisterSimpleUpgrade(buildingUpgrades[i].Upgrade);
                    }
                }
            }
            else if (upgradesTrigger != null)
            {
                upgradesTrigger.gameObject.SetActive(false);
            }
        }

        public virtual void OnWorldLoaded()
        {

        }

        public virtual void OnWorldUnloaded()
        {

        }

        public virtual void SpawnUnlocked()
        {
            Init();

            if (openedVisuals != null)
                openedVisuals?.SetActive(true);

            if (closedVisuals != null)
                closedVisuals?.SetActive(false);
        }

        public virtual void SpanwNotUnlocked()
        {
            if (openedVisuals != null)
                openedVisuals?.SetActive(false);

            if (closedVisuals != null)
                closedVisuals?.SetActive(true);
        }

        public virtual void FullyUnlock()
        {
            Init();

            if (unlockAnimation != null)
                unlockAnimation.RunUnlockedAnimation();

            if (openedVisuals != null)
                openedVisuals?.SetActive(true);

            if (closedVisuals != null)
                closedVisuals?.SetActive(false);

            // waiting for animation to complete before enabling navmesh
            Tween.DelayedCall(unlockAnimation != null ? unlockAnimation.TotalAnimationDuration : 0f, () =>
            {
                NavMeshController.CalculateNavMesh();
            });

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

            AudioController.PlaySound(AudioController.AudioClips.appear);
        }

        public void SetID(string id)
        {
            ID = id;
        }

        #region Editor

        protected bool EditorHaveUpgrades()
        {
            return !buildingUpgrades.IsNullOrEmpty();
        }


        protected bool EditorHaveCapacityUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.StorageCapacity)
                    return true;
            }

            return false;
        }

        protected bool EditorHaveDurationUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.ConversionDuration)
                    return true;
            }

            return false;
        }

        protected bool EditorHaveRecipeUpgrade()
        {
            if (buildingUpgrades == null)
                return false;

            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                if (buildingUpgrades[i].UpgradeType == BuildingUpgradeType.Recipe)
                    return true;
            }

            return false;
        }

        #endregion
    }
}