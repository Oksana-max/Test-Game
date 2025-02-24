using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
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
    [SerializeField] public Terrain terrain;
    [SerializeField] public GameObject cell;
    [SerializeField] public GameObject menuLost;
    [SerializeField] public GameObject menuWon;
    [SerializeField] public GameObject menuPause;
    [SerializeField] public GameObject menuPrompt;
    [SerializeField] public Animator animator;
    GameObject instantiatedCell;
    GameObject[,] gridCell;
    [SerializeField] public Button buttonPropmt;
    Vector3 terrainCenter;
    [SerializeField] public Transform player;  // Ссылка на персонажа
    [SerializeField] public Camera mainCamera;  // Ссылка на основную камеру
    [SerializeField] public GameData gameData;
    [SerializeField] public PlayerData playerData;
    Utility utility;


    void GetLevelInfo()
    {
        LevelData levelData = gameData.GetLevelData(levelIndex);

        if (levelData != null)
        {
            rows = levelData.fieldWidth;
            cols = levelData.fieldHeight;
            bombCount = levelData.bombCount;
            totalCoin = levelData.rewardPoints;
            levelIndex = levelData.levelIndex;
        }
        else
        {
            Debug.LogError("Уровень не найден!");
        }

    }


    public void ShowPrompt()//Подсказка
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
                highlighter.HighlightWithGlow();
            }


        }

    }

    // // Функция корректировки клеток под рельеф
    // void AdjustMeshToTerrain(GameObject cell, Terrain terrain)
    // {
    //     if (cell == null || terrain == null) return;

    //     MeshFilter meshFilter = cell.GetComponent<MeshFilter>();
    //     if (meshFilter == null) return;

    //     // Создаем копию меша, чтобы избежать ошибки
    //     Mesh mesh = Instantiate(meshFilter.sharedMesh);
    //     mesh.name = cell.name + "_Deformed";  // Уникальное имя меша

    //     Vector3[] vertices = mesh.vertices;

    //     for (int i = 0; i < vertices.Length; i++)
    //     {
    //         Vector3 worldPos = cell.transform.TransformPoint(vertices[i]); // Перевод в мировые координаты
    //         float terrainHeight = terrain.SampleHeight(worldPos);          // Получаем высоту террайна
    //         worldPos.y = terrainHeight + 0.002f;                                    // Устанавливаем новую высоту
    //         vertices[i] = cell.transform.InverseTransformPoint(worldPos);  // Обратно в локальные координаты
    //     }

    //     // Обновляем меш
    //     mesh.vertices = vertices;
    //     mesh.RecalculateNormals();  // Пересчитываем нормали
    //     mesh.RecalculateBounds();   // Пересчитываем границы

    //     // Устанавливаем новый меш в MeshFilter
    //     meshFilter.mesh = mesh;

    //     // Обновляем MeshCollider, если он есть
    //     MeshCollider meshCollider = cell.GetComponent<MeshCollider>();
    //     if (meshCollider != null)
    //     {
    //         meshCollider.sharedMesh = null;  // Сначала сбрасываем меш
    //         meshCollider.sharedMesh = mesh;  // Затем обновляем
    //     }
    // }


    void Start()
    {
        utility = new Utility();
        buttonPropmt.GetComponentInChildren<TextMeshProUGUI>().text = prompt.ToString();
        gameData = LevelDataLoader.LoadGameData();
        playerData = PlayerDataLoader.LoadPlayerData();
        levelIndex = LevelManager.SelectedLevelIndex;
        GetLevelInfo();
        Debug.Log("Загружен " + levelIndex + "уровень");
        Animations.OnCellChecked += CheckCell;
        gridCell = new GameObject[rows, cols];
        safeCellsCount = rows * cols - bombCount; // Общее количество клеток минус количество бомб
        terrainCenter = utility.GetTerrainCenter(terrain);
        for (int i = 0; i < gridCell.GetLength(0); i++)
        {
            for (int j = 0; j < gridCell.GetLength(1); j++)
            {
                // Вычисляем позицию клетки
                float x = terrainCenter.x + j * 0.5f;
                float z = terrainCenter.z + i * 0.5f;

                // Создаём клетку на вычисленных координатах
                Vector3 position = new Vector3(x, 0.001f, z);
                instantiatedCell = Instantiate(cell, position, Quaternion.Euler(-90, 0, 0));
                gridCell[i, j] = instantiatedCell;
                instantiatedCell.name = $"Cell_{i}_{j}";
                TextMeshPro textNum = instantiatedCell.GetComponentInChildren<TextMeshPro>();
                textNum.enabled = false;
                GameObject imageBomb = instantiatedCell.transform.Find("Bomb")?.gameObject;
                imageBomb.gameObject.SetActive(false);

            }
        }

        PlaceBomb(bombCount, gridCell);  // Расставляем бомбы
        PlaceNumbersAroundBombs(gridCell); // Расставляем числа вокруг бомб
    }

    void Update()
    {
        if (prompt <= 0)
        {
            buttonPropmt.GetComponentInChildren<TextMeshProUGUI>().text = "+";
        }
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
                StartCoroutine(HandleExplosion(cell));
                return; // Выход, чтобы не продолжать выполнение кода
            }
        }

        // Продолжить стандартное открытие клетки
        OpenNeighbor(gridCell, cell);
        cell.GetComponent<Renderer>().material.color = Color.gray;
        TextMeshPro textNum = cell.GetComponentInChildren<TextMeshPro>();
        textNum.enabled = true;
    }

    IEnumerator HandleExplosion(GameObject cell)
    {
        animator.SetTrigger("IsBomb");
        // Ждем один кадр, чтобы анимация и эффекты запустились одновременно
        yield return new WaitForSeconds(0.5f);

        ParticleSystem explosionEffect = cell.transform.Find("ExplosionEffect")?.GetComponent<ParticleSystem>();
        AudioSource audioSource = cell.GetComponentInChildren<AudioSource>();
        // Запускаем звук взрыва
        if (audioSource != null && explosionEffect != null)
        {
            audioSource.Play();  // Запускаем звук
        }
        // Запускаем эффект взрыва
        if (explosionEffect != null && audioSource != null)
        {
            explosionEffect.gameObject.SetActive(true);
            explosionEffect.Play();  // Запускаем воспроизведение эффекта взрыва
        }


        // Меняем цвет материала клетки на черный
        Renderer cellRenderer = cell.GetComponent<Renderer>();
        if (cellRenderer != null)
        {
            // Меняем цвет материала клетки на черный
            cellRenderer.material.color = Color.black;
        }

        // Делаем небольшую задержку перед проигрышем
        yield return new WaitForSeconds(2f);
        // Показываем меню проигрыша
        menuLost.SetActive(true);
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
