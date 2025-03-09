using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class HelperUtilities
{
    public static Camera mainCamera;
    /// <summary>
    /// Empty string debug check
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }
    /// <summary>
    /// null value debug check
    /// </summary>
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }
    /// <summary>
    /// list empty or contains null value check - returns true if there is an error
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " is null in object " + thisObject.name.ToString());
            return true;
        }


        foreach (var item in enumerableObjectToCheck)
        {

            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisObject.name.ToString());
            error = true;
        }

        return error;
    }
    public static Vector3 GetMousePositionInUI(RectTransform rectTransform, Camera uiCamera = null)
    {

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();

        // Clamp mouse position to screen size
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                mouseScreenPosition,
                uiCamera,
                out Vector2 localPoint
            );
        Vector3 mousePosition = rectTransform.TransformPoint(localPoint);

        return mousePosition;
    }

    public static Vector3 GetMouseWorldPosition3D(float distanceFromCamera)
    {
        Vector3 pointerPosition;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0) // Kiểm tra có touch trên màn hình không
        {
            pointerPosition = Touchscreen.current.touches[0].position.ReadValue(); // Lấy vị trí touch đầu tiên
        }
        else if (Mouse.current != null) // Nếu không có touch, kiểm tra chuột
        {
            pointerPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            return Vector3.zero; // Nếu không có input, trả về (0,0,0)
        }

        pointerPosition.z = distanceFromCamera; // Thiết lập Z để chuyển đổi tọa độ
        return Camera.main.ScreenToWorldPoint(pointerPosition);
    }

    public static string ToShortString(this double num)
    {
        if (num < 100) return num.ToString("0.##");
        if (num < 1_000) return num.ToString("0.##");
        if (num < 1_000_000) return (num / 1_000).ToString("0.##") + "K";
        if (num < 1_000_000_000) return (num / 1_000_000).ToString("0.##") + "M";
        if (num < 1_000_000_000_000) return (num / 1_000_000_000).ToString("0.##") + "B";
        return (num / 1_000_000_000_000).ToString("0.##") + "T";
    }

    public static void ShakeCamera(Camera camera)
    {
        float duration = 0.5f;
        float strength = 1f;
        int vibrato = 10;
        float randomness = 90f;
        Transform cameraTransform = camera.transform;
        Transform transform = camera.transform;
        DOTween.Kill(camera.transform);
        camera.transform.position = transform.position;
        camera.transform.DOShakePosition(duration, strength, vibrato, randomness).OnComplete(() =>
         {
             camera.transform.position = cameraTransform.position;
             camera.transform.rotation = cameraTransform.rotation;
         });
    }

    /// <summary>
    /// Convert the linear volume scale to decibels
    /// </summary>
    public static float LinearToDecibels(float linear)
    {
        float linearScaleRange = 20f;

        // formula to convert from the linear scale to the logarithmic decibel scale
        return Mathf.Log10((float)linear / linearScaleRange) * 20f;
    }

    /// <summary>
    /// positive value debug check - if zero is allowed set isZeroAllowed to true. Returns true if there is an error
    /// </summary>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }
}
