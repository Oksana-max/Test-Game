using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    // PlayerData playerData;
    // Start is called before the first frame update
    void Start()
    {
        // LoadPlayerData();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // void LoadPlayerData()
    // {
    //     string filePath = Path.Combine(Application.streamingAssetsPath, "Player.json");

    //     if (File.Exists(filePath))
    //     {
    //         string jsonData = File.ReadAllText(filePath);
    //         playerData = JsonUtility.FromJson<PlayerData>(jsonData);
    //         Debug.Log("Данные игрока успешно загружены!");
    //     }
    //     else
    //     {
    //         Debug.LogError("Файл с данными не найден: " + filePath);
    //     }
    // }
}
