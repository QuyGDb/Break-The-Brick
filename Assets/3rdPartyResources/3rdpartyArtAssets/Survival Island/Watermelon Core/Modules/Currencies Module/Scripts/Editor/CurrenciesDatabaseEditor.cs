using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(CurrenciesDatabase))]
    public class CurrenciesDatabaseEditor : WatermelonEditor
    {
        private CurrenciesDatabase currenciesDatabase;

        protected override void OnEnable()
        {
            base.OnEnable();

            currenciesDatabase = (CurrenciesDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create Sprite Atlas"))
            {
                CreateAtlas();
            }
        }

        private void CreateAtlas()
        {
            if (currenciesDatabase == null) return;

            List<TMPAtlasGenerator.SpriteData> atlasElements = new List<TMPAtlasGenerator.SpriteData>();

            Currency[] currencies = currenciesDatabase.Currencies;

            //Set Full Rect Type manualy
            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].Icon != null)
                {
                    TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(currencies[i].Icon));

                    TextureImporterSettings settings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    textureImporter.SetTextureSettings(settings);
                    textureImporter.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();

            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].Icon != null)
                {
                    atlasElements.Add(new TMPAtlasGenerator.SpriteData(currencies[i].Icon, currencies[i].CurrencyType.ToString()));
                }
            }

            TMPAtlasGenerator.Create(atlasElements, "");
        }
    }
}
