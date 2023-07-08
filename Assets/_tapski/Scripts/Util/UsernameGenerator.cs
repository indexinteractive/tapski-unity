using System.Globalization;
using System.IO;
using UnityEngine;

public class UsernameGenerator : Singleton<UsernameGenerator>
{
    private class Dict
    {
        public string[] Words { get; private set; }

        public Dict(string filepath)
        {
            Load(filepath);
        }

        private void Load(string filepath)
        {
            Words = File.ReadAllLines(filepath);
        }
    }

    public string Generate()
    {
        string adjectivesPath = Path.Combine(Application.streamingAssetsPath, "text/adjectives.txt");
        string nounsPath = Path.Combine(Application.streamingAssetsPath, "text/nouns.txt");

        Dict adjectives = new Dict(adjectivesPath);
        Dict nouns = new Dict(nounsPath);

        string adjective = adjectives.Words[Random.Range(0, adjectives.Words.Length)];
        string noun = nouns.Words[Random.Range(0, nouns.Words.Length)];

        string username = string.Format("{0}_{1}", adjective, noun);

        TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
        return txtInfo.ToTitleCase(username).Replace("_", string.Empty);
    }
}