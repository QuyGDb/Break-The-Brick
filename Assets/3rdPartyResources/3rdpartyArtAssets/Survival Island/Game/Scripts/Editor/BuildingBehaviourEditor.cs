using UnityEngine;
using UnityEditor;
using System;

namespace Watermelon
{
    [CustomEditor(typeof(BuildingBehavior), true)]
    public class BuildingBehaviourEditor : WatermelonEditor
    {
        private readonly string UPGRADES_HOLDER_NAME = "Upgrades Holder";
        private readonly string BUILDING_UPGRADES_PROPERTY_PATH = "buildingUpgrades";
        private const string UPGRADE_TYPE_PROPERTY_PATH = "upgradeType";
        private const string UPGRADE_PROPERTY_PATH = "upgrade";

        private Transform upgradesHolder;
        private BuildingBehavior buildingRef;
        private bool isFoldoutExpanded;
        private BuildingUpgradeType upgradeType;
        private SerializedProperty buildingUpgradesProperty;
        private BuildingUpgradeType[] upgradeTypes;
        private bool[] existingUpgradeTypes;
        private Rect dropdownButtonRect;
        private bool upgradeHolderSet;

        protected override void OnEnable()
        {
            base.OnEnable();

            buildingRef = target as BuildingBehavior;
            upgradeHolderSet = false;
            buildingUpgradesProperty = serializedObject.FindProperty(BUILDING_UPGRADES_PROPERTY_PATH);
            upgradeTypes = (BuildingUpgradeType[])Enum.GetValues(typeof(BuildingUpgradeType));
            existingUpgradeTypes = new bool[upgradeTypes.Length];
            upgradeType = BuildingUpgradeType.StorageCapacity;
        }

        private void SetUpgradeHolder()
        {
            // searching for existing upgrades holder
            for (int i = 0; i < buildingRef.transform.childCount; i++)
            {
                if (buildingRef.transform.GetChild(i).name.Equals(UPGRADES_HOLDER_NAME))
                {
                    upgradesHolder = buildingRef.transform.GetChild(i);
                }
            }

            // if not found - creating new holder
            if (upgradesHolder == null)
            {
                upgradesHolder = new GameObject(UPGRADES_HOLDER_NAME).transform;
                upgradesHolder.SetParent(buildingRef.transform);
                upgradesHolder.transform.localPosition = Vector3.zero;
                upgradesHolder.transform.localRotation = Quaternion.identity;
                upgradesHolder.transform.localScale = Vector3.one;
            }

            upgradeHolderSet = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (targets.Length > 1)
            {
                EditorGUILayout.LabelField("Multi edit is not supported");
            }

            isFoldoutExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldoutExpanded, "Upgrades Helper");

            if (isFoldoutExpanded)
            {
                EditorGUILayout.BeginHorizontal();


                if (EditorGUILayout.DropdownButton(new GUIContent(upgradeType.ToString()), FocusType.Passive))
                {
                    HandleDropdownMenu();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    dropdownButtonRect = GUILayoutUtility.GetLastRect();
                }

                if (GUILayout.Button("Create"))
                {
                    Create();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void HandleDropdownMenu()
        {
            for (int i = 0; i < upgradeTypes.Length; i++)
            {
                existingUpgradeTypes[i] = false;
            }

            for (int typeIndex = 0; typeIndex < upgradeTypes.Length; typeIndex++)
            {
                for (int i = 0; i < buildingUpgradesProperty.arraySize; i++)
                {
                    if (buildingUpgradesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(UPGRADE_TYPE_PROPERTY_PATH).intValue == (int)upgradeTypes[typeIndex])
                    {
                        existingUpgradeTypes[typeIndex] = true;
                        break;
                    }
                }
            }

            GenericMenu genericMenu = new GenericMenu();

            for (int i = 0; i < upgradeTypes.Length; i++)
            {
                if (existingUpgradeTypes[i])
                {
                    genericMenu.AddDisabledItem(new GUIContent(upgradeTypes[i].ToString()));
                }
                else
                {
                    genericMenu.AddItem(new GUIContent(upgradeTypes[i].ToString()), false, SetUpgradeType, i);
                }
            }

            genericMenu.DropDown(dropdownButtonRect);
        }

        private void SetUpgradeType(object userData)
        {
            upgradeType = upgradeTypes[(int)userData];
        }

        private void Create()
        {
            for (int i = 0; i < buildingUpgradesProperty.arraySize; i++)
            {
                if (buildingUpgradesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(UPGRADE_TYPE_PROPERTY_PATH).intValue == (int)upgradeType)
                {
                    Debug.LogError("Upgrade with type " + upgradeType + " already exists.");
                    return;
                }
            }

            if (!upgradeHolderSet)
            {
                SetUpgradeHolder();
            }

            GameObject upgradeObject = new GameObject(upgradeType + " Upgrade", GetScriptTypeForUpgrade(upgradeType));
            upgradeObject.transform.SetParent(upgradesHolder);

            BuildingUpgradeContainer upgradeContrainer = new BuildingUpgradeContainer(upgradeType, upgradeObject.GetComponent<AbstractLocalUpgrade>());

            buildingUpgradesProperty.arraySize++;

            SerializedProperty newElement = buildingUpgradesProperty.GetArrayElementAtIndex(buildingUpgradesProperty.arraySize - 1);
            newElement.FindPropertyRelative(UPGRADE_TYPE_PROPERTY_PATH).intValue = (int)upgradeType;
            newElement.FindPropertyRelative(UPGRADE_PROPERTY_PATH).objectReferenceValue = upgradeObject.GetComponent<AbstractLocalUpgrade>();

            serializedObject.ApplyModifiedProperties();
        }



        private Type GetScriptTypeForUpgrade(BuildingUpgradeType type)
        {
            if (type == BuildingUpgradeType.ConversionDuration)
            {
                return typeof(SimpleFloatUpgrade);
            }
            else if (type == BuildingUpgradeType.StorageCapacity)
            {
                return typeof(ConverterCapacityUpgrade);
            }
            else if (type == BuildingUpgradeType.Recipe)
            {
                return typeof(ConverterRecipeUpgrade);
            }

            return typeof(SimpleIntUpgrade);
        }
    }
}
