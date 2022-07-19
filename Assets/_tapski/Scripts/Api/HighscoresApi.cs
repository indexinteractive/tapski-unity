using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

struct ScoreData
{
    public string device_id;
    public string display_name;
    public int score;
}

public class HighscoresApi : Singleton<HighscoresApi>
{
    #region Api Fields
    private const string BASE_URL = "https://xqmaglxoqhbjsrivmtcn.supabase.co/rest/v1";
    private const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhxbWFnbHhvcWhianNyaXZtdGNuIiwicm9sZSI6ImFub24iLCJpYXQiOjE2NTc2MzY3MTIsImV4cCI6MTk3MzIxMjcxMn0.h__LfU9zMxMit2f3qNwA3lv3-Mm5kF9NqtbzVGaEiS4";
    #endregion

    #region Properties
    private string _deviceId => AuthUser.Instance.UserId;
    #endregion

    #region Public Methods
    public async Task<PlayerRank[]> GetPlayerScoreViewAsync(int padding)
    {
        string url = $"{BASE_URL}/rpc/player_rank?device_id={_deviceId}&lim={padding}";
        UnityWebRequest request = ApiGet(url);

        var result = request.SendWebRequest();
        while (!result.isDone)
        {
            await Task.Yield();
        }

        if (result.webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(result.webRequest.error);
            Debug.Log("Unable to get player score view");
            throw new System.Exception("Unable to get player score view");
        }
        else
        {
            Debug.Log("Player score view retrieved " + request.downloadHandler.text);
            var data = JsonUtility.FromJson<RankingView>("{\"PlayerRanks\":" + request.downloadHandler.text + "}");
            return data.PlayerRanks;
        }
    }
    #endregion

    #region Helpers
    private UnityWebRequest ApiGet(string url, bool singleValue = false)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", API_KEY);
        request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=representation");

        if (singleValue)
        {
            request.SetRequestHeader("Accept", "application/vnd.pgrst.object+json");
        }

        return request;
    }
    #endregion
}
