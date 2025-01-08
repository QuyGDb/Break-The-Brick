using UnityEditor;

namespace Watermelon
{
    public class EditorFoldoutBool
    {
        public string Key { get; private set; }

        private bool value;
        public bool Value
        {
            get => value;
            set
            {
                this.value = value;

                EditorPrefs.SetBool(Key, value);
            }
        }

        public EditorFoldoutBool(string key, bool defaultValue = true)
        {
            Key = key;
            Value = EditorPrefs.GetBool(key, defaultValue);
        }
    }
}

// -----------------
// Watermelon Editor v1.1
// -----------------

// Changelog
// v 1.1
// • Removed bottle icon
// • Added copy icon
// • Added Unique ID editor module
// • Added custom SceneSaving callback
// v 1.0
// • Basic logic