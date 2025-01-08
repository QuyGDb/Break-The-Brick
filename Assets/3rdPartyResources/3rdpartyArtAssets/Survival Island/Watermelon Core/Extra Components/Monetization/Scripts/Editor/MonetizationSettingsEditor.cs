using UnityEngine;
using UnityEditor;

namespace Watermelon.Monetization
{
    [CustomEditor(typeof(MonetizationSettings))]
    public class MonetizationSettingsEditor : WatermelonEditor
    {
        private SerializedProperty activeProperty;
        private SerializedProperty verboseLoggingProperty;
        private SerializedProperty debugModeProperty;

        private SerializedProperty adsSettingsProperty;
        private SerializedProperty iapSettingsProperty;

        private GUIContent[] tabs;
        private static int currentTab = 0;

        private Editor tabEditor;

        protected override void OnEnable()
        {
            base.OnEnable();

            adsSettingsProperty = serializedObject.FindProperty("adsSettings");
            iapSettingsProperty = serializedObject.FindProperty("iapSettings");

            activeProperty = serializedObject.FindProperty("isModuleActive");
            verboseLoggingProperty = serializedObject.FindProperty("verboseLogging");
            debugModeProperty = serializedObject.FindProperty("debugMode");

            tabs = new GUIContent[]
            {
                new GUIContent("  Ads", EditorCustomStyles.GetIcon("icon_ads"), "Advertisement"),
                new GUIContent("  IAP", EditorCustomStyles.GetIcon("icon_iap"), "IAP"), 
            };

            showScriptField = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Rect panelRect = EditorGUILayout.BeginVertical();

            activeProperty.boolValue = EditorGUILayoutCustom.BeginToggleBoxGroup("Mobile Monetization", activeProperty.boolValue);

            if(activeProperty.boolValue)
            {
                EditorGUILayout.PropertyField(verboseLoggingProperty);
                EditorGUILayout.PropertyField(debugModeProperty);
            }

            EditorGUILayoutCustom.EndBoxGroup();

            GUILayout.Space(15);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            if (activeProperty.boolValue)
            {
                GUI.contentColor = new Color(0.8f, 0.8f, 0.8f);
                int tempTab = GUI.Toolbar(new Rect(0, panelRect.y + panelRect.height + 5, Screen.width, 30), currentTab, tabs, EditorCustomStyles.tab);
                if (tempTab != currentTab)
                {
                    if (tabEditor != null)
                    {
                        DestroyImmediate(tabEditor);
                    }

                    currentTab = tempTab;

                    GUI.FocusControl(null);
                }
                GUI.contentColor = Color.white;

                GUILayout.Space(35);

                if (currentTab == 0)
                {
                    if (tabEditor == null)
                        Editor.CreateCachedEditor(adsSettingsProperty.objectReferenceValue, null, ref tabEditor);

                    tabEditor.serializedObject.Update();
                    tabEditor.OnInspectorGUI();
                    tabEditor.serializedObject.ApplyModifiedProperties();
                }
                else if (currentTab == 1)
                {
                    if (tabEditor == null)
                        Editor.CreateCachedEditor(iapSettingsProperty.objectReferenceValue, null, ref tabEditor);

                    tabEditor.serializedObject.Update();
                    tabEditor.OnInspectorGUI();
                    tabEditor.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            if (tabEditor != null)
            {
                DestroyImmediate(tabEditor);
            }
        }

        [MenuItem("Assets/Create/Settings/Monetization Settings")]
        public static void CreateAsset()
        {
            MonetizationSettings monetizationSettings = EditorUtils.CreateScriptableObject<MonetizationSettings>("Monetization Settings");

            SerializedObject serializedObject = new SerializedObject(monetizationSettings);

            SerializedProperty adsProperty = serializedObject.FindProperty("adsSettings");

            // Create ads object
            AdsSettings adsSettings = ScriptableObject.CreateInstance<AdsSettings>();
            adsSettings.name = "Ads Settings";

            AssetDatabase.AddObjectToAsset(adsSettings, monetizationSettings);

            SerializedProperty iapProperty = serializedObject.FindProperty("iapSettings");

            // Create IAP object
            IAPSettings iapSettings = ScriptableObject.CreateInstance<IAPSettings>();
            iapSettings.name = "IAP Settings";

            AssetDatabase.AddObjectToAsset(iapSettings, monetizationSettings);

            serializedObject.Update();

            iapProperty.objectReferenceValue = iapSettings;
            adsProperty.objectReferenceValue = adsSettings;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            Selection.activeObject = monetizationSettings;

            EditorGUIUtility.PingObject(monetizationSettings);
        }
    }
}
