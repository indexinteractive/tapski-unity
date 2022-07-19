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