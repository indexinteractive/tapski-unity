using System.Globalization;
using System.IO;
using UnityEngine;

public class UsernameGenerator
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
        Dict adjectives = new Dict("Assets/_tapski/Resources/adjectives.txt");
        Dict nouns = new Dict("Assets/_tapski/Resources/nouns.txt");

        string adjective = adjectives.Words[Random.Range(0, adjectives.Words.Length)];
        string noun = nouns.Words[Random.Range(0, nouns.Words.Length)];

        string username = string.Format("{0}_{1}", adjective, noun);

        TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
        return txtInfo.ToTitleCase(username).Replace("_", string.Empty);
    }
}