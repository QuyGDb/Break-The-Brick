using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(100)]
    public sealed partial class CinemachineCameraController : MonoBehaviour
    {
        private const int ACTIVE_CAMERA_PRIORITY = 100;
        private const int UNACTIVE_CAMERA_PRIORITY = 0;

        private static CinemachineCameraController cameraController;

        [SerializeField] CameraType firstCamera;

        [Space]
        [SerializeField] VirtualCameraCase[] virtualCameras;

        private static Dictionary<CameraType, int> virtualCamerasLink;

        private static Camera mainCamera;
        public static Camera MainCamera => mainCamera;

        private static Transform mainTarget;
        public static Transform MainTarget => mainTarget;

        private static VirtualCameraCase activeVirtualCamera;
        public static VirtualCameraCase ActiveVirtualCamera => activeVirtualCamera;

        private CinemachineBrain cameraBrain;

        private void Awake()
        {
            cameraController = this;

            // Get camera component
            mainCamera = GetComponent<Camera>();
            cameraBrain = GetComponent<CinemachineBrain>();
            cameraBrain.enabled = false;

            // Initialise cameras link
            virtualCamerasLink = new Dictionary<CameraType, int>();
            for(int i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Initialise();
                virtualCamerasLink.Add(virtualCameras[i].CameraType, i);
            }

            PreviewCamera.Initialise();

            EnableCamera(firstCamera);
        }

        public static void SetMainTarget(Transform target)
        {
            // Link target
            mainTarget = target;

            cameraController.cameraBrain.enabled = false;

            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Follow = target;
                cameraController.virtualCameras[i].VirtualCamera.LookAt = target;
            }

            cameraController.cameraBrain.transform.position = target.position;
            cameraController.cameraBrain.enabled = true;
        }

        public static VirtualCameraCase GetCamera(CameraType cameraType)
        {
            return cameraController.virtualCameras[virtualCamerasLink[cameraType]];
        }

        public static void EnableCamera(CameraType cameraType)
        {
            if (activeVirtualCamera != null && activeVirtualCamera.CameraType == cameraType)
                return;

            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Priority = UNACTIVE_CAMERA_PRIORITY;
            }

            activeVirtualCamera = cameraController.virtualCameras[virtualCamerasLink[cameraType]];
            activeVirtualCamera.VirtualCamera.Priority = ACTIVE_CAMERA_PRIORITY;
        }

        public static void Expand(float time, SimpleCallback callback)
        {
            if (activeVirtualCamera != null)
            {
                var transposer = activeVirtualCamera.VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    Vector3 startValue = transposer.m_FollowOffset;
                    Tween.DoFloat(0, 1, 0.8f, (result) =>
                    {
                        transposer.m_FollowOffset = Vector3.Lerp(startValue, startValue * 2, result);
                    }).SetEasing(Ease.Type.SineInOut).OnComplete(() => callback?.Invoke());

                    Tween.DoFloat(1, 0, 0.8f, (result) =>
                    {
                        transposer.m_FollowOffset = Vector3.Lerp(startValue, startValue * 2, result);
                    }, 0.8f + time).SetEasing(Ease.Type.SineInOut);
                }
            }
        }
    }
}