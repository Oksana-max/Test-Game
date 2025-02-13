using UnityEngine;
using System.IO;
using UnityEngine.Networking;
public static class PlayerDataLoader
{
    public static PlayerData LoadPlayerData()

    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Player.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        return LoadFromStreamingAssets(filePath);
#else
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<PlayerData>(jsonData);
        }
        else
        {
            Debug.LogError("Файл с данными не найден: " + filePath);
            return new PlayerData();  // Возвращаем пустой объект, чтобы избежать ошибок
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static PlayerData LoadFromStreamingAssets(string filePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        request.SendWebRequest();
        while (!request.isDone) { }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonData = request.downloadHandler.text;
            return JsonUtility.FromJson<PlayerData>(jsonData);
        }
        else
        {
            Debug.LogError("Ошибка при загрузке данных: " + request.error);
            return new PlayerData();
        }
    }
#endif


    // Метод для обновления данных игрока
    public static void UpdatePlayerData(int levelIndex, int totalCoin)
    {
        // Используем путь к PersistentDataPath для записи данных
        string filePath = Path.Combine(Application.streamingAssetsPath, "Player.json");

        if (File.Exists(filePath))
        {
            // Чтение данных из файла
            string jsonData = File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);

            // Обновляем уровень
            string levelIndexString = (levelIndex + 1).ToString();
            if (!playerData.openedLevels.Contains(levelIndexString))
            {
                playerData.openedLevels.Add(levelIndexString);
            }

            // Обновляем общий счёт
            playerData.totalScore += totalCoin;

            // Сохраняем обновленные данные обратно в файл
            string updatedJsonData = JsonUtility.ToJson(playerData, true);
            File.WriteAllText(filePath, updatedJsonData);

            Debug.Log("Данные игрока успешно обновлены!");
        }
        else
        {
            Debug.LogError("Файл с данными не найден: " + filePath);
        }
    }
}
