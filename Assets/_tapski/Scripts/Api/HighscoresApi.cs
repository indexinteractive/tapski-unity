using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class HighscoresApi : Singleton<HighscoresApi>
{
    #region Api Fields
    private const string BASE_URL = "!BASE_URL!";
    private const string API_KEY = "!API_KEY!";
    #endregion

    #region Properties
    private string _deviceId => AuthUser.Instance.UserId;
    #endregion

    #region Public Methods
    public int checksum(int score)
    {
        return /*CHECK*/0/*SUM*/;
    }

    public async Task<int> SetHighscoreAsync(int score)
    {
        string url = $"{BASE_URL}/rpc/set_score";
        var data = JsonUtility.ToJson(new ScoreData(_deviceId, score, checksum(score)));
        Debug.Log($"[HighscoresApi] Sending highscore data {data}");

        UnityWebRequest request = ApiPost(url, data);

        var result = request.SendWebRequest();
        while (!result.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            Debug.Log("Unable to set highscore");
            return -1;
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Highscore set: " + responseText);
            return int.Parse(responseText);
        }
    }

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

    private UnityWebRequest ApiPost(string url, string data)
    {
        UnityWebRequest request = UnityWebRequest.Post(url, data);
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
        request.SetRequestHeader("apikey", API_KEY);
        request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }
    #endregion
}
