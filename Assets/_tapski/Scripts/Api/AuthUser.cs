using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AuthUser : Singleton<AuthUser>
{
    #region Api Fields
    private const string BASE_URL = "!BASE_URL!";
    private const string API_KEY = "!API_KEY!";
    #endregion

    #region Properties
    public string UserId => SystemInfo.deviceUniqueIdentifier;
    #endregion

    #region Public Methods
    public async UniTask GetUser(GameState state)
    {
        if (string.IsNullOrWhiteSpace(state.Username))
        {
            string url = $"{BASE_URL}/rpc/create_user";

            string username = await UsernameGenerator.Instance.Generate();
            var user = new DbUserData(UserId, username);
            string data = JsonUtility.ToJson(user);
            Debug.Log($"[AuthUser] Sending user data {data}");

            UnityWebRequest request = ApiPost(url, data);

            var result = request.SendWebRequest();
            while (!result.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log("Unable to create user");
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("User created: " + responseText);
                state.Username = username;
            }
        }
        else
        {
            Debug.Log("Found username in game state: " + state.Username);
        }
    }
    #endregion

    #region Helpers
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
