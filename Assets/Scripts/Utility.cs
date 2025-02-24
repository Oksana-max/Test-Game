using UnityEngine;

public class Utility
{
    public  Vector3 GetTerrainCenter(Terrain myTerrain)
    {
        // Получаем размеры Terrain
        float width = myTerrain.terrainData.size.x;
        float length = myTerrain.terrainData.size.z;

        // Находим центр
        float centerX = myTerrain.transform.position.x + width / 2;
        float centerZ = myTerrain.transform.position.z + length / 3.5f;

        // Получаем высоту на позиции центра Terrain
        float centerY = myTerrain.SampleHeight(new Vector3(centerX, 0, centerZ));

        return new Vector3(centerX, centerY, centerZ);
    }
}
