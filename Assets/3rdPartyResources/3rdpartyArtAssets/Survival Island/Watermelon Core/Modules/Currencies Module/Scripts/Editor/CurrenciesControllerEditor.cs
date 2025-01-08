using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(CurrenciesController))]
    public class CurrenciesControllerEditor : Editor
    {
        private CurrenciesController currenciesController;
        private Currency[] currencies;

        private void OnEnable()
        {
            currenciesController = (CurrenciesController)target;         
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(currenciesController.CurrenciesDatabase == null)
            {
                return;
            }
            else
            {
                currencies = currenciesController.CurrenciesDatabase.Currencies;
            }

            EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);
            for (int i = 0; i < currencies.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(currencies[i].CurrencyType.ToString());

                if (GUILayout.Button("+10", EditorCustomStyles.buttonGreen))
                {
                    CurrenciesController.Add(currencies[i].CurrencyType, 10);
                }

                if (GUILayout.Button("+100", EditorCustomStyles.buttonGreen))
                {
                    CurrenciesController.Add(currencies[i].CurrencyType, 100);
                }

                if (GUILayout.Button("+1000", EditorCustomStyles.buttonGreen))
                {
                    CurrenciesController.Add(currencies[i].CurrencyType, 1000);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }
    }
}