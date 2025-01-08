using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

namespace Watermelon
{
    public class ControllOverlayTilesDimensionSettings : SceneOverlayCustomControl
    {
        private const int ITEM_CLASS_ID = 114;
        private const string ITEM_SCRIPT_CLASS = "GroundTileBehavior";
        private const string MIN_SLIDER_SAVE_PREFIX = "ground_tile_behaviour_min"; //must be unique
        private const string MAX_SLIDER_SAVE_PREFIX = "ground_tile_behaviour_max"; //must be unique
        private const string GIZMO_TOGGLE_SAVE_PREFIX = "ground_tile_behaviour_gizmo_enabled"; //must be unique
        private MethodInfo setGizmoEnabled;
        private MethodInfo setIconEnabled;
        private Foldout container;
        private FloatField minFloatField;
        private MinMaxSlider slider;
        private FloatField maxFloatField;

        public static float MinValue { get; private set; }
        public static float MaxValue { get; private set; }

        public override VisualElement CreateControl()
        {
#if UNITY_EDITOR
            CacheGizmoMethods();

            int minLimit = -1;
            int maxLimit = 10;

            container = new Foldout();
            container.text = "Tiles Dimension settings";
            Toggle toggle = new Toggle("Gizmo enabled");
            toggle.SetValueWithoutNotify(EditorPrefs.GetBool(GIZMO_TOGGLE_SAVE_PREFIX, true));
            SetGizmoVisibility(toggle.value);
            toggle.RegisterValueChangedCallback(OnToggleValueChanged);

            VisualElement horizontalContainer = new VisualElement();
            minFloatField = new FloatField(6);
            maxFloatField = new FloatField(6);
            slider = new MinMaxSlider(EditorPrefs.GetFloat(MIN_SLIDER_SAVE_PREFIX, minLimit), EditorPrefs.GetFloat(MAX_SLIDER_SAVE_PREFIX, maxLimit), minLimit, maxLimit);
            slider.RegisterValueChangedCallback(OnSliderValueChanged);
            minFloatField.SetValueWithoutNotify(slider.value.x);
            maxFloatField.SetValueWithoutNotify(slider.value.y);
            minFloatField.RegisterValueChangedCallback(OnMinValueChanged);
            maxFloatField.RegisterValueChangedCallback(OnMaxValueChanged);

            container.Add(toggle);
            Label label = new Label("Height range");
            container.Add(label);
            container.Add(slider);
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.alignItems = Align.Stretch;
            horizontalContainer.Add(minFloatField);
            horizontalContainer.Add(maxFloatField);
            container.Add(horizontalContainer);

            MinValue = slider.minValue;
            MaxValue = slider.maxValue;
            return container;
#else
return null;
#endif
        }

        private void OnToggleValueChanged(ChangeEvent<bool> evt)
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool(GIZMO_TOGGLE_SAVE_PREFIX, evt.newValue);
            SetGizmoVisibility(evt.newValue);
#endif
        }

        private void OnMaxValueChanged(ChangeEvent<float> evt)
        {
#if UNITY_EDITOR
            Vector2 newValue = new Vector2(slider.minValue, Mathf.Clamp(evt.newValue, slider.minValue, slider.highLimit));
            slider.SetValueWithoutNotify(newValue);
            maxFloatField.SetValueWithoutNotify(newValue.y);
            maxFloatField.MarkDirtyRepaint();
            EditorPrefs.SetFloat(MIN_SLIDER_SAVE_PREFIX, newValue.x);
            EditorPrefs.SetFloat(MAX_SLIDER_SAVE_PREFIX, newValue.y);

            MinValue = slider.minValue;
            MaxValue = slider.maxValue;
#endif
        }

        private void OnMinValueChanged(ChangeEvent<float> evt)
        {
#if UNITY_EDITOR
            Vector2 newValue = new Vector2(Mathf.Clamp(evt.newValue, slider.lowLimit, slider.maxValue), slider.maxValue);
            minFloatField.SetValueWithoutNotify(newValue.x);
            minFloatField.MarkDirtyRepaint();
            slider.SetValueWithoutNotify(newValue);
            EditorPrefs.SetFloat(MIN_SLIDER_SAVE_PREFIX, newValue.x);
            EditorPrefs.SetFloat(MAX_SLIDER_SAVE_PREFIX, newValue.y);

            MinValue = slider.minValue;
            MaxValue = slider.maxValue;
#endif
        }

        private void OnSliderValueChanged(ChangeEvent<Vector2> evt)
        {
#if UNITY_EDITOR
            EditorPrefs.SetFloat(MIN_SLIDER_SAVE_PREFIX, evt.newValue.x);
            EditorPrefs.SetFloat(MAX_SLIDER_SAVE_PREFIX, evt.newValue.y);
            minFloatField.SetValueWithoutNotify(evt.newValue.x);
            maxFloatField.SetValueWithoutNotify(evt.newValue.y);

            MinValue = slider.minValue;
            MaxValue = slider.maxValue;
#endif
        }


        private void SetGizmoVisibility(bool isEnabled)
        {
            if (isEnabled)
            {
                setGizmoEnabled.Invoke(null, new object[] { ITEM_CLASS_ID, ITEM_SCRIPT_CLASS, 1, false });
                setIconEnabled.Invoke(null, new object[] { ITEM_CLASS_ID, ITEM_SCRIPT_CLASS, 1 });
            }
            else
            {
                setGizmoEnabled.Invoke(null, new object[] { ITEM_CLASS_ID, ITEM_SCRIPT_CLASS, 0, false });
                setIconEnabled.Invoke(null, new object[] { ITEM_CLASS_ID, ITEM_SCRIPT_CLASS, 0 });
            }
        }

        private void CacheGizmoMethods()
        {
#if UNITY_EDITOR
            System.Type type = System.Reflection.Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.AnnotationUtility");
            setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
#endif
        }
    }
}
