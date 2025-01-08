using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(DropAnimation))]
    public class DropAnimationEditor : WatermelonEditor
    {
        private static readonly int POSITION_ANGLE = (int)DropAnimation.PositionType.RandomAngle;
        private static readonly int POSITION_PLACE = (int)DropAnimation.PositionType.OnPlace;
        private static readonly int POSITION_OBJECT = (int)DropAnimation.PositionType.ObjectDirection;

        private static readonly int MOVEMENT_BEZIER = (int)DropAnimation.MovementType.Bezier;
        private static readonly int MOVEMENT_CURVES = (int)DropAnimation.MovementType.Curves;

        private IEnumerable<SerializedProperty> ungroupProperties;

        private SerializedProperty positionTypeProperty;
        private SerializedProperty positionRangeProperty;
        private SerializedProperty positionRandomAngleProperty;
        private SerializedProperty inverseDirectionProperty;

        private SerializedProperty movementTypeProperty;
        private SerializedProperty movementDurationProperty;
        private SerializedProperty movementDelayProperty;
        private SerializedProperty movementEasingProperty;

        private IEnumerable<SerializedProperty> bezierMovementProperties;
        private IEnumerable<SerializedProperty> curvesMovementProperties;

        private SerializedProperty scaleToggleProperty;
        private IEnumerable<SerializedProperty> scaleProperties;

        private SerializedProperty rotationToggleProperty;
        private IEnumerable<SerializedProperty> rotationProperties;

        protected override void OnEnable()
        {
            base.OnEnable();

            positionTypeProperty = serializedObject.FindProperty("positionType");
            positionRangeProperty = serializedObject.FindProperty("positionRange");
            positionRandomAngleProperty = serializedObject.FindProperty("positionRandomAngle");
            inverseDirectionProperty = serializedObject.FindProperty("inverseDirection");

            movementTypeProperty = serializedObject.FindProperty("movementType");
            movementDurationProperty = serializedObject.FindProperty("movementDuration");
            movementDelayProperty = serializedObject.FindProperty("movementDelay");
            movementEasingProperty = serializedObject.FindProperty("movementEasing");

            bezierMovementProperties = serializedObject.GetPropertiesByGroup("MovementBezier");
            curvesMovementProperties = serializedObject.GetPropertiesByGroup("MovementCurve");

            ungroupProperties = serializedObject.GetUngroupProperties();

            scaleToggleProperty = serializedObject.FindProperty("useScale");
            scaleProperties = serializedObject.GetPropertiesByGroup("Scale");

            rotationToggleProperty = serializedObject.FindProperty("useRotation");
            rotationProperties = serializedObject.GetPropertiesByGroup("Rotation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayoutCustom.BeginBoxGroup("Position");
            EditorGUILayout.PropertyField(positionTypeProperty);
            if (positionTypeProperty.intValue != POSITION_PLACE)
            {
                EditorGUILayout.PropertyField(positionRangeProperty);
            }
            if (positionTypeProperty.intValue == POSITION_ANGLE || positionTypeProperty.intValue == POSITION_OBJECT)
            {
                EditorGUILayout.PropertyField(positionRandomAngleProperty);
                EditorGUILayout.PropertyField(inverseDirectionProperty);
            }
            EditorGUILayoutCustom.EndBoxGroup();

            EditorGUILayoutCustom.BeginBoxGroup("Movement");

            EditorGUILayout.PropertyField(movementTypeProperty);
            EditorGUILayout.PropertyField(movementDurationProperty);
            EditorGUILayout.PropertyField(movementDelayProperty);
            EditorGUILayout.PropertyField(movementEasingProperty);
            
            GUILayout.Space(8);

            if (movementTypeProperty.intValue == MOVEMENT_BEZIER)
            {
                foreach(var property in bezierMovementProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            if (movementTypeProperty.intValue == MOVEMENT_CURVES)
            {
                foreach (var property in curvesMovementProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            EditorGUILayoutCustom.EndBoxGroup();

            scaleToggleProperty.boolValue = EditorGUILayoutCustom.BeginToggleBoxGroup("Scale", scaleToggleProperty.boolValue);

            if(scaleToggleProperty.boolValue)
            {
                foreach (var property in scaleProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }

            EditorGUILayoutCustom.EndBoxGroup();

            rotationToggleProperty.boolValue = EditorGUILayoutCustom.BeginToggleBoxGroup("Rotation", rotationToggleProperty.boolValue);
            if (rotationToggleProperty.boolValue)
            {
                foreach (var property in rotationProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            EditorGUILayoutCustom.EndBoxGroup();

            if (ungroupProperties.Any())
            {
                EditorGUILayoutCustom.BeginBoxGroup("Custom fields");
                foreach (var property in ungroupProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
                EditorGUILayoutCustom.EndBoxGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
