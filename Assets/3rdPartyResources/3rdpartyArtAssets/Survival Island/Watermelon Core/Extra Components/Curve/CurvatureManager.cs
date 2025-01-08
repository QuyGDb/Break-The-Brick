using UnityEngine;
using UnityEngine.Rendering;

namespace Watermelon
{
    public class CurvatureManager : MonoBehaviour
    {
        public static CurvatureManager Instance { get; private set; }

        [SerializeField, OnOff, Order(-1)] bool isCurvatureEnabled;

        [BoxGroup("Data"), ShowIf("isCurvatureEnabled"), UnpackNested]
        [SerializeField] CurveData data;

        public CurveData CurveData { get; private set; }

        public static  bool TempTargetEnabled { get; private set; }
        public static Transform TempTarget { get; private set; }

        private void Awake()
        {
            Instance = this;

            if (isCurvatureEnabled && CurvatureActionsMenu.IsCurvatureDisabled())
            {
                isCurvatureEnabled = false;
            }

            CurveData = data;

            SetCurveValues();
            CalculateCameraCullingMatrix();
        }

        public void SetCurveOverride(CurveData overrideData)
        {
            CurveData = overrideData;
            SetCurveValues();
        }

        public void RemoveCurveOverride()
        {
            CurveData = data;
            SetCurveValues();
        }

        public void CalculateCameraCullingMatrix()
        {
            var camera = Camera.main;
            float aspect = camera.aspect;
            float fov = camera.fieldOfView;
            float viewPortHeight = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
            float viewPortWidth = viewPortHeight * aspect;

            float newFov = fov * CurveData.FovMultiplier;
            float newHeight = Mathf.Tan(Mathf.Deg2Rad * newFov * 0.5f);
            float newAspect = viewPortWidth * CurveData.AspectMultiplier / (newHeight);

            var projection = Matrix4x4.Perspective(newFov, newAspect, camera.nearClipPlane, camera.farClipPlane);
            camera.cullingMatrix = projection * camera.worldToCameraMatrix;
        }

        private void Update()
        {
            var player = PlayerBehavior.GetBehavior();

            if (player != null || TempTargetEnabled)
            {
                CalculateCameraCullingMatrix();
            }

            if (!isCurvatureEnabled)
                return;

            if (TempTargetEnabled)
            {
                Shader.SetGlobalVector("_PlayerPosition", TempTarget.position);
            } else if (player != null)
            {
                Shader.SetGlobalVector("_PlayerPosition", player.transform.position);
            }
        }

        public static void EnableTempTarget(Transform tempTarget)
        {
            TempTarget = tempTarget;
            TempTargetEnabled = true;
        }

        public static void DisableTempTarget()
        {
            TempTarget = null;
            TempTargetEnabled = false;
        }

        public void SetCurveValues()
        {
            if (Application.isPlaying)
            {
                Shader.SetGlobalFloat("_CurveForwardOffset", isCurvatureEnabled ? CurveData.ForwardOffset : 0f);
                Shader.SetGlobalFloat("_CurveForwardPower", isCurvatureEnabled ? CurveData.ForwardPower : 0f);

                Shader.SetGlobalFloat("_CurveSideOffset", isCurvatureEnabled ? CurveData.SideOffset : 0f);
                Shader.SetGlobalFloat("_CurveSidePower", isCurvatureEnabled ? CurveData.SidePower : 0f);
            }
        }

        private void OnDestroy()
        {
            Shader.SetGlobalFloat("_CurveForwardOffset", 0f);
            Shader.SetGlobalFloat("_CurveForwardPower", 0f);

            Shader.SetGlobalFloat("_CurveSideOffset", 0f);
            Shader.SetGlobalFloat("_CurveSidePower", 0f);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if(CurveData == null)
                {
                    CurveData = data;
                }
                SetCurveValues();
            }
        }
    }
}