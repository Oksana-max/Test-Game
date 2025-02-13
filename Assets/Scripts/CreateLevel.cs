using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CreateLevel : MonoBehaviour
{
    int rows;
    int cols;
    int levelIndex;
    int bombCount;
    int totalCoin;
    int safeCellsCount; // Общее количество безопасных клеток
    int openedCellsCount = 0; // Количество открытых клеток
    int prompt = 1;
    bool gameStarted = false;
    public bool isPaused = false;  // Флаг для отслеживания паузы
    public Terrain terrain;
    public GameObject cell;
    public GameObject menuLost;
    public GameObject menuWon;
    public GameObject menuPause;
    public GameObject menuPrompt;
    GameObject instantiatedCell;
    GameObject[,] gridCell;
    public Button buttonPropmt;
    Vector3 terrainCenter;
    private Vector3 cameraOriginalPosition;
    private bool isZoomed = false;
    public Transform player;  // Ссылка на персонажа
    public Camera mainCamera;  // Ссылка на основную камеру
    public float zoomSpeed = 2f;  // Скорость зума

    public GameData gameData;
    public PlayerData playerData;

    // void LoadGameData()
    // {
    //     string filePath = Path.Combine(Application.streamingAssetsPath, "Level.json");

    //     if (File.Exists(filePath))
    //     {
    //         string jsonData = File.ReadAllText(filePath);
    //         gameData = JsonUtility.FromJson<GameData>(jsonData);
    //         Debug.Log("Данные успешно загружены!");
    //     }
    //     else
    //     {
    //         Debug.LogError("Файл с данными не найден: " + filePath);
    //     }
    // }


    void GetLevelInfo()
    {
        foreach (var locations in gameData.locations)
        {
            foreach (var level in locations.levels)
            {
                if (level.levelIndex == levelIndex)
                {
                    rows = level.fieldWidth;
                    cols = level.fieldHeight;
                    bombCount = level.bombCount;
                    levelIndex = level.levelIndex;
                    totalCoin = level.rewardPoints;
                }
            }
        }
    }

    void PositionCamera(Vector3 terrainCenter, int gridWidth, int gridHeight)
    {
        float cellSize = 0.5f;

        float halfWidth = gridWidth * cellSize / 2f;
        float halfHeight = gridHeight * cellSize / 2f;

        float centerX = terrainCenter.x + halfWidth - cellSize / 2f;
        float centerZ = terrainCenter.z + halfHeight - cellSize / 2f;
        Vector3 centralCell = new Vector3(centerX, terrainCenter.y, centerZ);
        Debug.Log(centralCell);

        // Позиционирование камеры на определённой высоте и смещении по оси Z
        float distance = Mathf.Max(gridWidth, gridHeight) * 0.7f;  // Дистанция зависит от размера поля
        Camera.main.transform.position = new Vector3(centralCell.x, distance, centralCell.z - distance);

        // Установка угла наклона камеры
        Camera.main.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        // Направляем камеру на центральную клетку
        // Camera.main.transform.LookAt(centralCell);
    }


    public void ShowPrompt()
    {
        List<GameObject> safeCell = new List<GameObject>();
        List<GameObject> randomSafeCells = new List<GameObject>();
        List<int> pickedIndices = new List<int>();  // Список использованных индексов
        int cellCount = (rows * cols - bombCount) / 2;
        if (prompt > 0)
        {
            for (int i = 0; i < gridCell.GetLength(0); i++)
            {
                for (int j = 0; j < gridCell.GetLength(1); j++)
                {
                    if (!gridCell[i, j].GetComponent<Cell>().isBomb && !gridCell[i, j].GetComponent<Cell>().isOpen)
                        safeCell.Add(gridCell[i, j]);
                }
            }
            prompt--;
        }
        else SetActiveMenuPrompt();


        while (cellCount < safeCell.Count)
        {
            Debug.Log("подсказка");
            int randomIndex = Random.Range(0, cellCount);

            if (!pickedIndices.Contains(randomIndex))
            {
                pickedIndices.Add(randomIndex);
                randomSafeCells.Add(safeCell[randomIndex]);
            }
            cellCount++;
        }

        for (int i = 0; i < randomSafeCells.Count; i++)
        {
            CellHighlighter highlighter = randomSafeCells[i].GetComponent<CellHighlighter>();
            Debug.Log(highlighter);
            if (highlighter != null)
            {
                //Debug.Log("анимация");
                highlighter.HighlightWithGlow();
            }

            // randomSafeCells[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.blue);
            // Debug.Log(randomSafeCells[i]);
        }

    }



    void Start()
    {
        buttonPropmt.GetComponentInChildren<TextMeshProUGUI>().text = prompt.ToString();
        gameData = LevelDataLoader.LoadGameData();
        playerData = PlayerDataLoader.LoadPlayerData();
        levelIndex = LevelManager.SelectedLevelIndex;
        GetLevelInfo();
        Debug.Log("Загружен " + levelIndex + "уровень");
        Animations.OnCellChecked += CheckCell;
        gridCell = new GameObject[rows, cols];
        safeCellsCount = rows * cols - bombCount; // Общее количество клеток минус количество бомб
        terrainCenter = GetTerrainCenter(terrain);
        float fixedY = 0.03f; // Постоянная высота по оси Y
        for (int i = 0; i < gridCell.GetLength(0); i++)
        {
            for (int j = 0; j < gridCell.GetLength(1); j++)
            {
                // Вычисляем позицию клетки
                float x = terrainCenter.x + j * 0.5f;
                float z = terrainCenter.z + i * 0.5f;

                // Создаём клетку на вычисленных координатах
                Vector3 position = new Vector3(x, fixedY, z);
                instantiatedCell = Instantiate(cell, position, Quaternion.identity);
                instantiatedCell.transform.Rotate(90, 0, 0);
                gridCell[i, j] = instantiatedCell;
                instantiatedCell.name = $"Cell_{i}_{j}";

                //.......
                TextMeshPro textNum = instantiatedCell.GetComponentInChildren<TextMeshPro>();
                textNum.enabled = false;
                GameObject imageBomb = instantiatedCell.transform.Find("Bomb")?.gameObject;
                imageBomb.gameObject.SetActive(false);

            }
        }

        PositionCamera(terrainCenter, rows, cols);

        PlaceBomb(bombCount, gridCell);  // Расставляем бомбы
        PlaceNumbersAroundBombs(gridCell); // Расставляем числа вокруг бомб

        cameraOriginalPosition = mainCamera.transform.position;  // Сохраняем исходное положение камеры
    }

    void Update()
    {
        if (prompt <= 0)
        {
            buttonPropmt.GetComponentInChildren<TextMeshProUGUI>().text = "+";
        }
    }

    public void OnZoomButtonPressed()
    {
        if (!isZoomed)
        {
            StartCoroutine(ZoomToPlayer());
        }
        else
        {
            StartCoroutine(ZoomOut());
        }
        isZoomed = !isZoomed;
    }

    IEnumerator ZoomToPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y + 2, player.position.z - 2);
        float duration = 0.5f;  // Длительность зума
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.position;

        while (elapsedTime < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition;  // Убедиться, что камера в точке
    }

    IEnumerator ZoomOut()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.position;

        while (elapsedTime < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, cameraOriginalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = cameraOriginalPosition;
    }



    void CheckCell(GameObject cell)
    {
        Cell cellComponent = cell.GetComponent<Cell>();

        if (!gameStarted)
        {
            gameStarted = true;

            if (cellComponent.isBomb)
            {
                Debug.Log("Первая клетка была бомбой. Перемещаем бомбу...");

                // Найти новую безопасную клетку
                GameObject newSafeCell = FindSafeCell();
                if (newSafeCell != null)
                {
                    // Переместить бомбу
                    cellComponent.isBomb = false;
                    newSafeCell.GetComponent<Cell>().isBomb = true;
                    newSafeCell.GetComponent<Renderer>().material.color = Color.red;

                    // Обновить все числа вокруг бомб
                    PlaceNumbersAroundBombs(gridCell);
                }
            }
            OpenNeighbor(gridCell, cell);
        }

        else
        {
            if (cellComponent.isBomb)
            {
                menuLost.SetActive(true);
            }
        }

        // Продолжить стандартное открытие клетки
        OpenNeighbor(gridCell, cell);
        cell.GetComponent<Renderer>().material.color = Color.gray;
        TextMeshPro textNum = cell.GetComponentInChildren<TextMeshPro>();
        textNum.enabled = true;
    }


    GameObject FindSafeCell()
    {
        List<GameObject> safeCells = new List<GameObject>();

        for (int i = 0; i < gridCell.GetLength(0); i++)
        {
            for (int j = 0; j < gridCell.GetLength(1); j++)
            {
                Cell cellComponent = gridCell[i, j].GetComponent<Cell>();
                if (!cellComponent.isBomb)
                {
                    safeCells.Add(gridCell[i, j]);
                }
            }
        }

        if (safeCells.Count > 0)
        {
            // Возвращаем случайную безопасную клетку
            return safeCells[Random.Range(0, safeCells.Count)];
        }

        return null;
    }


    // Метод для вычисления центра Terrain
    Vector3 GetTerrainCenter(Terrain terrain)
    {
        // Получаем размеры Terrain
        float width = terrain.terrainData.size.x;
        float length = terrain.terrainData.size.z;

        // Находим центр
        float centerX = terrain.transform.position.x + width / 2;
        float centerZ = terrain.transform.position.z + length / 2;

        // Получаем высоту на позиции центра Terrain
        float centerY = terrain.SampleHeight(new Vector3(centerX, 0, centerZ));

        return new Vector3(centerX, centerY, centerZ);
    }


    void OpenNeighbor(GameObject[,] gridCell, GameObject pressedCell)
    {
        for (int i = 0; i < gridCell.GetLength(0); i++)
        {
            for (int j = 0; j < gridCell.GetLength(1); j++)
            {
                if (gridCell[i, j] == pressedCell)
                {
                    Cell pressedCellComponent = pressedCell.GetComponent<Cell>();
                    if (!pressedCellComponent.isBomb && !pressedCellComponent.isOpen)
                    {
                        pressedCellComponent.isOpen = true;
                        pressedCell.GetComponent<Renderer>().material.color = Color.gray;
                        TextMeshPro textNum = pressedCell.GetComponentInChildren<TextMeshPro>();
                        textNum.enabled = true;
                        openedCellsCount++;

                        // Проверяем, не победил ли игрок
                        if (openedCellsCount == safeCellsCount)
                        {
                            Debug.Log("Вы выиграли!");
                            menuWon.SetActive(true);
                            PlayerDataLoader.UpdatePlayerData(levelIndex, totalCoin);
                            return;
                        }

                        // Если число на клетке 0, открываем всех соседей
                        if (pressedCellComponent.bombAroundCount == 0)
                        {
                            for (int x = -1; x <= 1; x++)
                            {
                                for (int y = -1; y <= 1; y++)
                                {
                                    int newX = i + x;
                                    int newY = j + y;

                                    if (newX >= 0 && newX < gridCell.GetLength(0) &&
                                        newY >= 0 && newY < gridCell.GetLength(1))
                                    {
                                        GameObject neighbor = gridCell[newX, newY];
                                        Cell neighborComponent = neighbor.GetComponent<Cell>();

                                        // Рекурсивно открываем соседей, если они не бомбы и ещё не открыты
                                        if (!neighborComponent.isBomb && !neighborComponent.isOpen)
                                        {
                                            OpenNeighbor(gridCell, neighbor);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            }
        }
    }


    void PlaceBomb(int bombCount, GameObject[,] gridCell)
    {
        System.Random rand = new System.Random();
        List<Vector2Int> availableCells = new List<Vector2Int>();

        // Собираем список всех клеток, куда можно поставить бомбы
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Cell cellComponent = gridCell[i, j].GetComponent<Cell>();
                if (!cellComponent.isOpen) // Исключаем уже открытые клетки
                {
                    availableCells.Add(new Vector2Int(i, j));
                }
            }
        }

        // Перемешиваем список случайным образом
        availableCells = availableCells.OrderBy(a => rand.Next()).ToList();

        // Размещаем бомбы в первых bombCount доступных клетках
        for (int i = 0; i < bombCount && i < availableCells.Count; i++)
        {
            int x = availableCells[i].x;
            int y = availableCells[i].y;
            gridCell[x, y].GetComponent<Cell>().isBomb = true;
            gridCell[x, y].GetComponent<Renderer>().material.color = Color.red; // Для теста
        }
    }


    void PlaceNumbersAroundBombs(GameObject[,] gridCell)
    {
        for (int i = 0; i < gridCell.GetLength(0); i++)
        {
            for (int j = 0; j < gridCell.GetLength(1); j++)
            {
                Cell cellComponent = gridCell[i, j].GetComponent<Cell>();

                if (!cellComponent.isBomb)
                {
                    int bombCount = 0;

                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            int newX = i + x;
                            int newY = j + y;

                            // Проверяем, что координаты внутри границ массива
                            if (newX >= 0 && newX < gridCell.GetLength(0) &&
                                newY >= 0 && newY < gridCell.GetLength(1))
                            {
                                Cell neighbor = gridCell[newX, newY].GetComponent<Cell>();
                                if (neighbor.isBomb)
                                {
                                    bombCount++;
                                }
                            }
                        }
                    }

                    cellComponent.bombAroundCount = bombCount;
                    TextMeshPro text = cellComponent.GetComponentInChildren<TextMeshPro>();

                    if (cellComponent.isBomb)
                    {
                        // Если это бомба, убираем текст
                        text.text = "";
                    }
                    if (bombCount == 0)
                    {
                        text.text = "";
                    }
                    else if (bombCount > 0)
                    {
                        // Если это не бомба и вокруг есть бомбы, показываем количество
                        text.text = bombCount.ToString();
                    }
                }

            }
        }
    }

    public void RestartScene()
    {
        // Получаем текущую активную сцену и перезагружаем её
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void SetActiveMenuPause()
    {
        menuPause.SetActive(true);
        isPaused = true;
    }

    public void SetInActiveMenuPause()
    {
        menuPause.SetActive(false);
        isPaused = false;
    }

    public void SetActiveMenuPrompt()
    {
        menuPrompt.SetActive(true);
        // isPaused = true;
    }

    public void SetInActiveMenuPrompt()
    {
        menuPrompt.SetActive(false);
        // isPaused = false;
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnDestroy()
    {
        Animations.OnCellChecked -= CheckCell;
    }
}
