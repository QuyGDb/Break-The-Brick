using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// This class provides 3 solutions to help navigation. 
    /// Arrow pointing down at something, arrow showing direction to something and arrows line showing direction to something.
    /// </summary>
    public class NavigationHelper : MonoBehaviour
    {
        private static NavigationHelper instance;

        [SerializeField] GameObject positionPointerPrefab;
        [SerializeField] DirecitonPointersController directionPointersController;

        private Pool positionPointerPool;

        public void Initialise()
        {
            instance = this;

            directionPointersController.Initialise();

            positionPointerPool = new Pool(new PoolSettings(positionPointerPrefab.name, positionPointerPrefab, 1, true));
        }

        private void LateUpdate()
        {
            directionPointersController.LateUpdate();
        }

        // returns ready to use arrow pointing down at specified position
        public static PositionPointerCase CreatePositionPointer(Vector3 position)
        {
            GameObject arrowObject = instance.positionPointerPool.GetPooledObject();
            arrowObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            arrowObject.transform.localScale = Vector3.one;

            return new PositionPointerCase(arrowObject);
        }

        // returns ready to use arrow showing direciton to specified position
        public static ArrowPointerCase CreateGuidingArrow(Vector3 position)
        {
            return DirecitonPointersController.RegisterArrowPointer(PlayerBehavior.InstanceTransform, position);
        }

        // returns ready to use arrow line showing direciton to specified position
        public static ArrowLinePointerCase CreateGuidingLine(Vector3 position)
        {
            return DirecitonPointersController.RegisterArrowLinePointer(PlayerBehavior.InstanceTransform, position);
        }

        // unloads all 3 types of provided helpers
        public static void Unload()
        {
            instance.positionPointerPool.ReturnToPoolEverything(true);

            instance.directionPointersController.Unload();
        }
    }
}