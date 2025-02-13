using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public static class LevelDataLoader
{
    public static GameData LoadGameData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Level.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        return LoadFromStreamingAssets(filePath);
#else
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GameData>(jsonData);
        }
        else
        {
            Debug.LogError("Файл с данными не найден: " + filePath);
            return new GameData();  // Возвращаем пустой объект, чтобы избежать ошибок
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static GameData LoadFromStreamingAssets(string filePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        request.SendWebRequest();
        while (!request.isDone) { }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonData = request.downloadHandler.text;
            return JsonUtility.FromJson<GameData>(jsonData);
        }
        else
        {
            Debug.LogError("Ошибка при загрузке данных: " + request.error);
            return new GameData();
        }
    }
#endif
}
