using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LevelsSripts : MonoBehaviour
{
    public GameObject allButtons;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI scoreText;

    PlayerData playerData;
    // Start is called before the first frame update
    void Start()
    {
        LoadData();
        playerName.text = "Player  " + playerData.id;
        scoreText.text = playerData.totalScore.ToString();

        Button[] buttons = allButtons.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            int currentIndex = i + 1;
            TextMeshProUGUI buttonText = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText == null)
            {
                buttonText = CreateTextForButton(buttons[i], (i + 1).ToString());
            }

            bool isCompleted = playerData.openedLevels.Contains(buttonText.text);
            buttons[i].interactable = isCompleted;

            if (buttonText != null)
            {
                string levelName = buttonText.text;
            }

            buttons[i].onClick.AddListener(() => LoadLevel(currentIndex));

        }
    }

    void LoadLevel(int index)
    {
        LevelManager.SelectedLevelIndex = index;
        SceneManager.LoadScene("Level1");
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void LoadData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "player.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
            Debug.Log("Данные игрока успешно загружены!");
        }
        else
        {
            Debug.LogError("Файл с данными не найден: " + filePath);
        }
    }

    TextMeshProUGUI CreateTextForButton(Button button, string text)
    {
        GameObject textObject = new GameObject("ButtonText");
        textObject.transform.SetParent(button.transform, false);

        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 64;
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return buttonText;
    }
}
