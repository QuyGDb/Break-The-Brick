using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PlayerSkinsController : MonoBehaviour
    {
        [SerializeField] PlayerSkinsDatabase database;

        private static PlayerSkinData[] availableSkins;

        private static PlayerSkinGlobalSave save;

        public static event SkinCallback SkinUnlocked;
        public static event SkinCallback SkinSelected;

        public void Initialise()
        {
            if (database == null) Debug.LogError("Skins database isn't linked to PlayerSkinsController", this);
            if (database.Skins.Length == 0) Debug.LogError("Add at least one skin to Skins Database!", this);

            availableSkins = database.Skins;

            foreach(PlayerSkinData skin in availableSkins)
            {
                skin.Initialise();
            }

            save = SaveController.GetSaveObject<PlayerSkinGlobalSave>("playerSkins");

            if (string.IsNullOrEmpty(save.SelectedSkinID))
            {
                ActivateDefaultSkin();
            }
        }

        public static PlayerSkinData GetActiveSkin()
        {
            foreach (PlayerSkinData skin in availableSkins)
            {
                if (skin.ID == save.SelectedSkinID)
                {
                    return skin;
                }
            }

            return ActivateDefaultSkin();
        }

        private static PlayerSkinData ActivateDefaultSkin()
        {
            PlayerSkinData defaultSkin = availableSkins[0];

            defaultSkin.Unlock();

            save.SelectedSkinID = defaultSkin.ID;

            return defaultSkin;
        }

        public static void SelectSkin(string skinID)
        {
            SelectSkin(GetSkinData(skinID));
        }

        public static void SelectSkin(PlayerSkinData skinData)
        {
            if (skinData != null && skinData.IsUnlocked)
            {
                save.SelectedSkinID = skinData.ID;

                SkinSelected?.Invoke(skinData);
            }
        }

        public static void UnlockSkin(string skinID, bool select = false)
        {
            PlayerSkinData skinData = GetSkinData(skinID);
            if(skinData != null)
            {
                if(!skinData.IsUnlocked)
                {
                    skinData.Unlock();

                    SkinUnlocked?.Invoke(skinData);
                }

                if(select)
                    SelectSkin(skinData);
            }
        }

        public static PlayerSkinData GetSkinData(string skinID)
        {
            foreach (PlayerSkinData skin in availableSkins)
            {
                if (skin.ID == skinID)
                    return skin;
            }

            return null;
        }

        public static bool IsSkinUnlocked(string skinID)
        {
            foreach (PlayerSkinData skin in availableSkins)
            {
                if (skin.ID == skinID)
                {
                    return skin.IsUnlocked;
                }
            }

            return false;
        }

        public delegate void SkinCallback(PlayerSkinData skinData);
    }
}
