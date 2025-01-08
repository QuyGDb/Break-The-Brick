using System.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Watermelon
{
    public static class CharacterCreator
    {
        private const string LOADING_TITLE = "Character Creation";
                        
        [MenuItem("GameObject/Prepare Player Skin", false, -200)]
        private static void ContextMenu(MenuCommand command)
        {
            GameObject characterObject = (GameObject)command.context;
            if(characterObject != null)
            {
                if (characterObject == null)
                {
                    Debug.LogError("[Character Creator]: Prefab can't be null!");

                    return;
                }

                Renderer characterRenderer = characterObject.GetComponentInChildren<Renderer>();
                if (characterRenderer == null)
                {
                    Debug.LogError("[Character Creator]: Render component can't be found!");

                    return;
                }

                PlayerSkinsDatabase skinsDatabase = EditorUtils.GetAsset<PlayerSkinsDatabase>();
                if (skinsDatabase == null)
                {
                    Debug.LogError("[Character Creator]: Player Skins Database can't be found!");

                    return;
                }

                if (skinsDatabase.TemplatePrefab == null)
                {
                    Debug.LogError("[Character Creator]: Skins Template can't be null!");

                    return;
                }

                string path = EditorUtility.SaveFilePanelInProject("Save Prefab", characterObject.name, "prefab", "");
                if (!string.IsNullOrEmpty(path))
                {
                    EditorCoroutines.Execute(CharacterCoroutine(path, characterObject, characterRenderer, skinsDatabase));
                }
            }
        }

        [MenuItem("GameObject/Prepare Player Skin", true)]
        private static bool ContextMenu()
        {
            return Selection.activeGameObject != null;
        }

        private static IEnumerator CharacterCoroutine(string path, GameObject characterObject, Renderer characterRenderer, PlayerSkinsDatabase skinsDatabase)
        {
            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating template...", 0);

            GameObject parentObject = (GameObject)PrefabUtility.InstantiatePrefab(skinsDatabase.TemplatePrefab);
            parentObject.transform.ResetGlobal();

            yield return null;

            characterObject.name = "Graphics";
            characterObject.transform.SetParent(parentObject.transform);
            characterObject.transform.localPosition = new Vector3(0, 0, 0);

            Animator characterAnimator = characterObject.GetOrSetComponent<Animator>();
            if (characterAnimator != null)
            {
                characterAnimator.runtimeAnimatorController = (AnimatorController)skinsDatabase.DefaultAnimator;
                characterAnimator.applyRootMotion = false;

                characterAnimator.gameObject.AddComponent<PlayerAnimationHandler>();
            }

            characterObject.SetActive(true);

            yield return null;

            PlayerGraphics playerGraphics = parentObject.GetComponent<PlayerGraphics>();

            GameObject toolHolderObject = new GameObject("Tool Holder");
            toolHolderObject.transform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.RightHand));
            toolHolderObject.transform.localPosition = new Vector3(-0.075f, 0.125f, 0.02f);
            toolHolderObject.transform.localEulerAngles = new Vector3(-5.5f, -78, 95);

            SerializedObject playerSerializedObject = new SerializedObject(playerGraphics);
            playerSerializedObject.Update();
            playerSerializedObject.FindProperty("animator").objectReferenceValue = characterAnimator;
            playerSerializedObject.FindProperty("interactionAnimations").FindPropertyRelative("toolHolderTransform").objectReferenceValue = toolHolderObject;
            playerSerializedObject.FindProperty("bodyRenderer").objectReferenceValue = characterRenderer;
            playerSerializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(playerGraphics);

            yield return null;

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Saving prefab...", 0);

            GameObject characterPrefab = PrefabUtility.SaveAsPrefabAsset(parentObject, path);

            GameObject.DestroyImmediate(parentObject);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorUtility.ClearProgressBar();

            yield return null;

            if(EditorUtility.DisplayDialog("Character Creator", "Add new character to database?", "Add", "Skip"))
            {
                SerializedObject databaseSerializedObject = new SerializedObject(skinsDatabase);
                databaseSerializedObject.Update();

                SerializedProperty skinsProperty = databaseSerializedObject.FindProperty("skins");
                skinsProperty.arraySize++;

                SerializedProperty addedProperty = skinsProperty.GetArrayElementAtIndex(skinsProperty.arraySize - 1);

                addedProperty.FindPropertyRelative("prefab").objectReferenceValue = characterPrefab;
                addedProperty.FindPropertyRelative("id").stringValue = UniqueIDUtils.GetUniqueID();

                databaseSerializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(skinsDatabase); 
                
                Selection.activeObject = skinsDatabase;
            }
            else
            {
                Selection.activeObject = characterPrefab;
            }

            Debug.Log("[Character Creator]: Character created successfully!");
        }
    }
}
