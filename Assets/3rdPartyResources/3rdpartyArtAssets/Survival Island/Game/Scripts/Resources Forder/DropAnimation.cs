using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Resource Drop Animation", menuName = "Content/Resource Drop Animation")]
    public class DropAnimation : ScriptableObject
    {
        // Position
        [Group("Position")]
        [SerializeField] PositionType positionType;
        [Group("Position")]
        [SerializeField] DuoFloat positionRange = new DuoFloat(1.0f, 1.2f);
        [Group("Position")]
        [SerializeField] float positionRandomAngle = 90;
        [Group("Position")]
        [SerializeField] bool inverseDirection = true;


        // Movement
        [Group("MovementType")]
        [SerializeField] MovementType movementType;
        [Group("Movement")]
        [SerializeField] float movementDuration = 0.2f;
        [Group("Movement")]
        [SerializeField] float movementDelay = 0f;
        [Group("Movement")]
        [SerializeField] Ease.Type movementEasing;

        // Bezier
        [Group("MovementBezier")]
        [SerializeField] DuoFloat bezierUpOffset = new DuoFloat(0, 1);
        [Group("MovementBezier")]
        [SerializeField] DuoFloat bezierRightOffset = new DuoFloat(0, 0);
        [Group("MovementBezier")]
        [SerializeField] DuoFloat bezierForwardOffset = new DuoFloat(0, 0);

        // Curves
        [Group("MovementCurve")]
        [SerializeField] AnimationCurve movementCurveX = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
        [Group("MovementCurve")]
        [SerializeField] AnimationCurve movementCurveY = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
        [Group("MovementCurve")]
        [SerializeField] AnimationCurve movementCurveZ = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

        // Scale
        [Group("ScaleToggle")]
        [SerializeField] bool useScale = false;
        [Group("Scale")]
        [SerializeField] float scaleDuration = 0.5f;
        [Group("Scale")]
        [SerializeField] AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));

        // Rotation
        [Group("RotationToggle")]
        [SerializeField] bool useRotation;
        [Group("Rotation")]
        [SerializeField] float rotationDuration = 0.5f;
        [Group("Rotation")]
        [SerializeField] Vector3 rotationAngle;
        [Group("Rotation")]
        [SerializeField] Ease.Type rotationEasing;

        public IEnumerator AnimationEnumerator(Data animationData, SimpleCallback onAnimationFinished)
        {
            Vector3 targetPosition = animationData.StartPosition.AddToY(animationData.VerticalOffset);
            if (positionType == PositionType.Random360)
            {
                targetPosition = GetRandom360Position(animationData).AddToY(animationData.VerticalOffset);
            }
            else if (positionType == PositionType.RandomAngle)
            {
                targetPosition = GetRandomPositionAroundObjectWithAngle((animationData.HitPosition - animationData.StartPosition).normalized * (inverseDirection ? 1 : -1), animationData.StartPosition, animationData.DropParentTransform.position.y, positionRandomAngle, positionRange.Random()).AddToY(animationData.VerticalOffset);
            }
            else if (positionType == PositionType.FromCenter)
            {
                targetPosition = GetRandomPositionAroundObjectWithAngle((animationData.DropParentTransform.position - animationData.StartPosition).normalized, animationData.StartPosition, animationData.DropParentTransform.position.y, positionRandomAngle, positionRange.Random()).AddToY(animationData.VerticalOffset);
            }
            else if (positionType == PositionType.ObjectDirection)
            {
                targetPosition = GetRandomPositionAroundObjectWithAngle((animationData.DropParentTransform.forward).normalized * (inverseDirection ? 1 : -1), animationData.StartPosition, animationData.DropParentTransform.position.y, positionRandomAngle, positionRange.Random()).AddToY(animationData.VerticalOffset);
            }

            TweenCase movementTweenCase;
            if (movementType == MovementType.Bezier)
            {
                movementTweenCase = new TransformTweenCases.BezierPosition(animationData.DropTransform, targetPosition, animationData.DropTransform.position + new Vector3(bezierUpOffset.Random(), bezierRightOffset.Random(), bezierForwardOffset.Random())).SetDuration(movementDuration).SetDelay(movementDelay).SetEasing(movementEasing);
            }
            else if (movementType == MovementType.Curves)
            {
                movementTweenCase = new CurvePositions(animationData.DropTransform, movementCurveX, movementCurveY, movementCurveZ, targetPosition).SetDuration(movementDuration).SetDelay(movementDelay);
            }
            else
            {
                movementTweenCase = new TransformTweenCases.Position(animationData.DropTransform, targetPosition).SetDuration(movementDuration).SetDelay(movementDelay).SetEasing(movementEasing);
            }

            TweenCase scaleTweenCase = null;
            if (useScale)
            {
                scaleTweenCase = new CurveScale(animationData.DropTransform, scaleCurve).SetDuration(scaleDuration);
            }

            TweenCase rotationTweenCase = null;
            if (useRotation)
            {
                rotationTweenCase = new AngleRotation(animationData.DropTransform, rotationAngle).SetDuration(rotationDuration).SetEasing(rotationEasing);
            }

            bool allTweensCompleted;

            while (true)
            {
                allTweensCompleted = true;

                if (!movementTweenCase.IsCompleted)
                {
                    if (movementTweenCase.Delay > 0 && movementTweenCase.Delay > movementTweenCase.CurrentDelay)
                    {
                        movementTweenCase.UpdateDelay(Time.deltaTime);
                    }
                    else
                    {
                        movementTweenCase.UpdateState(Time.deltaTime);

                        movementTweenCase.Invoke(0);
                    }

                    allTweensCompleted = false;
                }

                if (scaleTweenCase != null && !scaleTweenCase.IsCompleted)
                {
                    scaleTweenCase.UpdateState(Time.deltaTime);
                    scaleTweenCase.Invoke(0);

                    allTweensCompleted = false;
                }

                if (rotationTweenCase != null && !rotationTweenCase.IsCompleted)
                {
                    rotationTweenCase.UpdateState(Time.deltaTime);
                    rotationTweenCase.Invoke(0);

                    allTweensCompleted = false;
                }

                if (allTweensCompleted)
                {
                    break;
                }

                yield return null;
            }

            onAnimationFinished?.Invoke();
        }

        private Vector3 GetRandom360Position(Data animationData)
        {
            int counter = 0;
            Vector3 endPos;

            Transform dropParent = animationData.DropParentTransform;

            float terrainHeight = dropParent.position.y + 100;

            do
            {
                endPos = dropParent.position.GetRandomPositionAroundObject(positionRange.Random());
                counter++;

                if (NavMesh.SamplePosition(endPos, out var hit, 0.1f, NavMesh.AllAreas) && Mathf.Abs(hit.position.y - endPos.y) < 0.5f)
                {
                    terrainHeight = hit.position.y;
                }
            }
            while (Mathf.Abs(terrainHeight - dropParent.position.y) >= 0.5f && counter < 20);

            return endPos;
        }

        private Vector3 GetRandomPositionAroundObjectWithAngle(Vector3 directionNormalizedVector, Vector3 objectPosition, float parentYPosition, float randomAngleOffset, float radius)
        {
            // Calculate the angle of this direction in radians
            float angleToHittable = Mathf.Atan2(directionNormalizedVector.z, directionNormalizedVector.x);

            float halfAngleRad = randomAngleOffset / 2 * Mathf.Deg2Rad;

            int counter = 0;
            Vector3 endPos;

            float terrainHeight = parentYPosition + 100;

            do
            {
                // Reverse the angle and add a random offset (convert to degrees for Random.Range)
                float reversedAngle = angleToHittable + Mathf.PI + Random.Range(-halfAngleRad, halfAngleRad);

                // Calculate the new position based on the reversed angle and the given radius
                float newX = objectPosition.x + Mathf.Cos(reversedAngle) * radius;
                float newZ = objectPosition.z + Mathf.Sin(reversedAngle) * radius;
                float newY = parentYPosition; // Keep the same Y position as the resource

                endPos = new Vector3(newX, newY, newZ);
                counter++;

                if (NavMesh.SamplePosition(endPos, out var hit, 0.1f, NavMesh.AllAreas) && Mathf.Abs(hit.position.y - endPos.y) < 0.5f)
                {
                    terrainHeight = hit.position.y;
                }
            }
            while (Mathf.Abs(terrainHeight - parentYPosition) >= 0.5f && counter < 20);

            // Return the new position
            return endPos;
        }

        public enum PositionType { Random360, RandomAngle, FromCenter, OnPlace, ObjectDirection }
        public enum MovementType { Default, Bezier, Curves }

        public class Data
        {
            public Transform DropParentTransform;
            public Transform DropTransform;
            public float VerticalOffset;

            public Vector3 StartPosition;
            public Vector3 HitPosition;
        }

        public class CurvePositions : TweenCase
        {
            private AnimationCurveEasingFunction curveXEasing;
            private AnimationCurveEasingFunction curveYEasing;
            private AnimationCurveEasingFunction curveZEasing;

            private Transform transform;

            private Vector3 startValue;
            private Vector3 resultValue;

            public CurvePositions(Transform tweenObject, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, Vector3 targetPosition)
            {
                parentObject = tweenObject.gameObject;
                transform = tweenObject;

                startValue = tweenObject.position;
                resultValue = targetPosition;

                curveXEasing = new AnimationCurveEasingFunction(curveX);
                curveYEasing = new AnimationCurveEasingFunction(curveY);
                curveZEasing = new AnimationCurveEasingFunction(curveZ);
            }

            public override bool Validate()
            {
                return parentObject != null;
            }

            public override void DefaultComplete()
            {
                transform.position = resultValue;
            }

            public override void Invoke(float deltaTime)
            {
                transform.position = new Vector3(Mathf.LerpUnclamped(startValue.x, resultValue.x, state) + curveXEasing.Interpolate(state), Mathf.LerpUnclamped(startValue.y, resultValue.y, state) + curveYEasing.Interpolate(state), Mathf.LerpUnclamped(startValue.z, resultValue.z, state) + curveZEasing.Interpolate(state));
            }
        }

        public class CurveScale : TweenCase
        {
            private Transform transform;
            private Vector3 defaultScale;

            public CurveScale(Transform tweenObject, AnimationCurve scaleCurve)
            {
                parentObject = tweenObject.gameObject;

                transform = tweenObject;
                defaultScale = transform.localScale;

                SetCurveEasing(scaleCurve);
            }

            public override bool Validate()
            {
                return parentObject != null;
            }

            public override void DefaultComplete()
            {
                transform.localScale = defaultScale;
            }

            public override void Invoke(float deltaTime)
            {
                transform.localScale = defaultScale * Interpolate(state);
            }
        }

        public class AngleRotation : TweenCase
        {
            private Transform transform;
            private Vector3 defaultRotation;
            private Vector3 targetRotation;

            private Vector3 rotationOffset;

            public AngleRotation(Transform tweenObject, Vector3 rotationOffset)
            {
                this.rotationOffset = rotationOffset;

                parentObject = tweenObject.gameObject;

                transform = tweenObject;
                defaultRotation = transform.eulerAngles;

                targetRotation = defaultRotation + rotationOffset;
            }

            public override bool Validate()
            {
                return parentObject != null;
            }

            public override void DefaultComplete()
            {
                transform.rotation = Quaternion.Euler(targetRotation);
            }

            public override void Invoke(float deltaTime)
            {
                transform.rotation = Quaternion.Euler(Vector3.LerpUnclamped(defaultRotation, targetRotation, Interpolate(state)));
            }
        }
    }
}