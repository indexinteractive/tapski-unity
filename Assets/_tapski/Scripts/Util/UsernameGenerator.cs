using System.Globalization;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UsernameGenerator : Singleton<UsernameGenerator>
{
    private class Dict
    {
        public string[] Words { get; private set; }

        public static async UniTask<Dict> Load(string filepath)
        {
            var dict = new Dict();
#if UNITY_WEBGL && !UNITY_EDITOR
            string text = await GetWebFileData(filepath);
            dict.Words = text.Split('\n');
#else
            await UniTask.Yield();
            dict.Words = File.ReadAllLines(filepath);
#endif
            return dict;
        }

        private static async UniTask<string> GetWebFileData(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
            string text = request.downloadHandler.text;
            return text;
        }
    }

    public async UniTask<string> Generate()
    {
        string adjectivesPath = Path.Combine(Application.streamingAssetsPath, "text/adjectives.txt");
        string nounsPath = Path.Combine(Application.streamingAssetsPath, "text/nouns.txt");

        Dict adjectives = await Dict.Load(adjectivesPath);
        Dict nouns = await Dict.Load(nounsPath);

        string adjective = adjectives.Words[Random.Range(0, adjectives.Words.Length)];
        string noun = nouns.Words[Random.Range(0, nouns.Words.Length)];

        string username = string.Format("{0}_{1}", adjective, noun);

        TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
        return txtInfo.ToTitleCase(username).Replace("_", string.Empty);
    }
}