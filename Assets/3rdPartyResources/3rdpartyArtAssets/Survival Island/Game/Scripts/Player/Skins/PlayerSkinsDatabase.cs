using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Player Skins Database", menuName = "Content/Player Skins Database")]
    public class PlayerSkinsDatabase : ScriptableObject
    {
        [SerializeField] PlayerSkinData[] skins;
        public PlayerSkinData[] Skins => skins;

        [BoxFoldout("CCT", label: "Skin Creation Tools", order: 100)]
        [SerializeField] GameObject templatePrefab;
        public GameObject TemplatePrefab => templatePrefab;

        [BoxFoldout("CCT")]
        [SerializeField] Object defaultAnimator;
        public Object DefaultAnimator => defaultAnimator;

    }
}
