using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Scene Overlay Database", menuName = "Core/Editor/Scene Overlay Database")]
    public class SceneOverlayDatabase : ScriptableObject
    {
        [SerializeField] SearchGroup[] gizmoGroups;
        [SerializeField] SearchGroup[] pickabilityGroups;
        [SerializeField] SearchGroup[] visibilityGroups;
    }

    [System.Serializable]
    public class SearchGroup
    {
        [SerializeField] string name;
        [SerializeField] string[] queries;
    }
}
