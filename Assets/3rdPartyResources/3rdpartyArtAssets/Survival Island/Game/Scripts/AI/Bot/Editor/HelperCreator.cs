using System.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Watermelon
{
    public static class HelperCreator
    {
        private const string LOADING_TITLE = "Helper Creation";

        [MenuItem("GameObject/Prepare Helper Skin", false, -200)]
        private static void ContextMenu(MenuCommand command)
        {
            GameObject characterObject = (GameObject)command.context;
            if (characterObject != null)
            {
                if (characterObject == null)
                {
                    Debug.LogError("[Helper Creator]: Prefab can't be null!");

                    return;
                }

                string path = EditorUtility.SaveFilePanelInProject("Save Prefab", characterObject.name, "prefab", "");
                if (!string.IsNullOrEmpty(path))
                {
                    EditorCoroutines.Execute(CharacterCoroutine(path, characterObject));
                }
            }
        }

        [MenuItem("GameObject/Prepare Helper Skin", true)]
        private static bool ContextMenu()
        {
            return Selection.activeGameObject != null;
        }

        private static IEnumerator CharacterCoroutine(string path, GameObject characterObject)
        {
            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating template...", 0);

            yield return null;

            characterObject.transform.localPosition = new Vector3(0, 0, 0);

            Object defaultAnimatorController = EditorUtils.GetAsset<Object>("Bot Animator Controller");

            InteractionData interactionData = EditorUtils.GetAsset<InteractionData>("Character Interaction Data");

            Animator characterAnimator = characterObject.GetOrSetComponent<Animator>();
            if (characterAnimator != null)
            {
                characterAnimator.runtimeAnimatorController = (AnimatorController)defaultAnimatorController;
                characterAnimator.applyRootMotion = false;
            }

            characterObject.GetOrSetComponent<ToolsPlacementHelper>();

            characterObject.SetActive(true);

            yield return null;

            HelperGraphics helperGraphics = characterObject.AddComponent<HelperGraphics>();
            helperGraphics.MoveComponentUp();

            GameObject toolHolderObject = new GameObject("Tool Holder");
            toolHolderObject.transform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.RightHand));
            toolHolderObject.transform.localPosition = new Vector3(-0.075f, 0.125f, 0.02f);
            toolHolderObject.transform.localEulerAngles = new Vector3(-5.5f, -78, 95);

            SerializedObject playerSerializedObject = new SerializedObject(helperGraphics);
            playerSerializedObject.Update();
            playerSerializedObject.FindProperty("animator").objectReferenceValue = characterAnimator;
            playerSerializedObject.FindProperty("interactionAnimations").FindPropertyRelative("interactionData").objectReferenceValue = interactionData;
            playerSerializedObject.FindProperty("interactionAnimations").FindPropertyRelative("toolHolderTransform").objectReferenceValue = toolHolderObject;
            playerSerializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(helperGraphics);

            yield return null;

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Saving prefab...", 0);

            GameObject characterPrefab = PrefabUtility.SaveAsPrefabAsset(characterObject, path);

            GameObject.DestroyImmediate(characterObject);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorUtility.ClearProgressBar();

            yield return null;

            Selection.activeObject = characterPrefab;

            Debug.Log("[Helper Creator]: Character created successfully!");
        }
    }
}
