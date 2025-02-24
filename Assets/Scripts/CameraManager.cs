using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    int rows;
    int cols;
    int levelIndex;
    float zoomFollowSpeed = 5f;
    bool isZoomed;
    Vector3 terrainCenter;
    [SerializeField] public Terrain terrain;
    [SerializeField] public Camera mainCamera;
    [SerializeField] public Transform player;
    GameData gameData;
    private Vector3 cameraOriginalPosition;
    Utility utility;
    void GetLevelInfo()
    {
        LevelData levelData = gameData.GetLevelData(levelIndex);

        if (levelData != null)
        {
            rows = levelData.fieldWidth;
            cols = levelData.fieldHeight;
            levelIndex = levelData.levelIndex;
        }
        else
        {
            Debug.LogError("Уровень не найден!");
        }

    }


    void Start()
    {
        utility = new Utility();
        gameData = LevelDataLoader.LoadGameData();
        levelIndex = LevelManager.SelectedLevelIndex;
        terrainCenter = utility.GetTerrainCenter(terrain);
        GetLevelInfo();
        PositionCamera(terrainCenter, rows, cols);
        cameraOriginalPosition = mainCamera.transform.position;
    }

    void PositionCamera(Vector3 terrainCenter, int gridWidth, int gridHeight)
    {
        float cellSize = 0.5f;

        float halfWidth = gridWidth * cellSize / 2f;
        float halfHeight = gridHeight * cellSize / 2f;

        float centerX = terrainCenter.x + halfWidth - cellSize / 2f;
        float centerZ = terrainCenter.z + halfHeight - cellSize / 2f;
        Vector3 centralCell = new Vector3(centerX, terrainCenter.y, centerZ);

        // Позиционирование камеры на определённой высоте и смещении по оси Z
        float distance = Mathf.Max(gridWidth, gridHeight) * 0.7f;  // Дистанция зависит от размера поля
        Camera.main.transform.position = new Vector3(centralCell.x, distance, centralCell.z - distance);

        // Установка угла наклона камеры
        Camera.main.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        // Направляем камеру на центральную клетку
        // Camera.main.transform.LookAt(centralCell);
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

    private void LateUpdate()
    {
        if (isZoomed)
        {
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y + 2, player.position.z - 2);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, zoomFollowSpeed * Time.deltaTime);
        }
    }

}
