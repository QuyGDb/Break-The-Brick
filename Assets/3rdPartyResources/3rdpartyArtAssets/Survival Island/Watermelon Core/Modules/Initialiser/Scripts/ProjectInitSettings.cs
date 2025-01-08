#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Project Init Settings", menuName = "Core/Project Init Settings")]
    public class ProjectInitSettings : ScriptableObject
    {
        [SerializeField] InitModule[] modules;
        public InitModule[] Modules => modules;

        public void Initialise(Initialiser initialiser)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                if(modules[i] != null)
                {
                    modules[i].CreateComponent(initialiser.gameObject);
                }
            }
        }
    }
}