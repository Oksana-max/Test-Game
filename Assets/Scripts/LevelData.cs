

using System.Collections.Generic;

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


[System.Serializable]
public class LocationData
{
    public string name;
    public List<LevelData> levels;
}

[System.Serializable]
public class GameData
{
    public List<LocationData> locations;
}