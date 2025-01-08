using UnityEngine;

namespace Watermelon
{
    public class PlayerSkinReward : Reward
    {
        [PlayerSkinPicker]
        [SerializeField] string skinID;

        [SerializeField] bool disableIfSkinIsUnlocked;

        private void OnEnable()
        {
            PlayerSkinsController.SkinUnlocked += OnSkinUnlocked;    
        }

        private void OnDisable()
        {
            PlayerSkinsController.SkinUnlocked -= OnSkinUnlocked;
        }

        public override void ApplyReward()
        {
            PlayerSkinsController.UnlockSkin(skinID, true);
        }

        public override bool CheckDisableState()
        {
            if(disableIfSkinIsUnlocked)
            {
                return PlayerSkinsController.IsSkinUnlocked(skinID);
            }

            return false;
        }

        private void OnSkinUnlocked(PlayerSkinData skinData)
        {
            if(disableIfSkinIsUnlocked)
            {
                if(skinData.ID == skinID)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
