using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public List<LocationData> locations;

    public LevelData GetLevelData(int levelIndex)
    {
        foreach (var location in locations)
        {
            foreach (var level in location.levels)
            {
                if (level.levelIndex == levelIndex)
                {
                    return level; // Возвращаем найденный уровень
                }
            }
        }
        return null; // Если уровень не найден
    }
}


[System.Serializable]
public class LocationData
{
    public string name;
    public List<LevelData> levels;
}



[System.Serializable]
public class LevelData
{
    public string nameLevel;
    public int levelIndex;
    public int difficulty;
    public int bombCount;
    public int rewardPoints;
    public int fieldWidth;
    public int fieldHeight;

}



