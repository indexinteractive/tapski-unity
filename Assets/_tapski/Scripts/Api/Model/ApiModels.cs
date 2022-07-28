using System;

/// <summary>
/// Represents a highscore entry in the database
/// </summary>
[Serializable]
public class PlayerRank
{
    public int score;
    public string device_id;
    public string display_name;
    public int rank;
}

[Serializable]
public class RankingView
{
    public PlayerRank[] PlayerRanks;
}

/// <summary>
/// The data sent to the server when setting a highscore
/// </summary>
[Serializable]
public struct ScoreData
{
    public string id;
    public int value;
    public int checksum;

    public ScoreData(string id, int value, int check)
    {
        this.id = id;
        this.value = value;
        checksum = check;
    }
}

/// <summary>
/// The user data sent to the server when registering/update a new user
/// </summary>
[Serializable]
public struct DbUserData
{
    public string id;
    public string name;

    public DbUserData(string device_id, string displayName)
    {
        name = displayName;
        id = device_id;
    }
}

[Serializable]
public struct UsernameUpdateData
{
    public string display_name;
}