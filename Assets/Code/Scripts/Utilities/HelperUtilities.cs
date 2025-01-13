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
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = distanceFromCamera; // Đặt z là khoảng cách từ camera
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }




    public static void ShakeCamera(Camera camera)
    {
        float duration = 0.5f;
        float strength = 1f;
        int vibrato = 10;
        float randomness = 90f;

        camera.transform.DOShakePosition(duration, strength, vibrato, randomness);


    }
}
