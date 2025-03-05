
using UnityEngine;

public static class SaveLoadManager
{
    public static string ConvertToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static void SaveDataToPlayerPrefs(string nameKey, object obj)
    {
        string jsonString = ConvertToJson(obj);
        PlayerPrefs.SetString(nameKey, jsonString);
        PlayerPrefs.Save();
    }
    public static T LoadDataFromPlayerPrefs<T>(string nameKey)
    {
        string jsonString = PlayerPrefs.GetString(nameKey, "{}");

        if (string.IsNullOrEmpty(jsonString) || jsonString == "{}")
        {
            Debug.Log("No player data found or data is empty.");
            return default(T); // Trả về giá trị mặc định của T (null nếu T là class)
        }

        return JsonUtility.FromJson<T>(jsonString); // Chuyển đổi JSON thành kiểu T cụ thể
    }

}
