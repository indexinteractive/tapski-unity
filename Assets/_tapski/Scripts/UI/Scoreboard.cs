using Firebase.Database;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

/// <summary>
/// Handles firebase data for the scoreboard.
/// NOTE: this script must start disabled when assigned in the editor, or it will
/// attempt to communicate with firebase before initialization
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class Scoreboard : MonoBehaviour
{
    #region Constants
    private const string HIGHSCORE_KEY = "highscores";
    #endregion

    #region Public Properties
    [Header("Item Template")]
    public VisualTreeAsset ScoreItemTemplate;
    [Range(8, 15)]
    public float ItemHeight = 12;

    [Header("Selectors")]
    public string ScoresContainerSelector = "item-container";
    public string SpinnerSelector = "spinner";
    public string HighlightClassname = "highlight";
    #endregion

    #region Firebase Properties
    public DatabaseReference HighScoreRef
    {
        get { return FirebaseDatabase.DefaultInstance.RootReference.Child(HIGHSCORE_KEY); }
    }

    public int ItemSlotsCount
    {
        get { return Mathf.FloorToInt(_container.resolvedStyle.height / ItemHeight); }
    }
    #endregion

    #region Private Fields
    private VisualElement _spinner;
    private VisualElement _container;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(ScoreItemTemplate, "[Scoreboard] Item template is unassigned");

        var root = GetComponent<UIDocument>().rootVisualElement;
        _container = root.Q(ScoresContainerSelector);
        _spinner = root.Q(SpinnerSelector);

        Assert.IsNotNull(_container, "[Scoreboard] Scores container was not found");
        Assert.IsNotNull(_spinner, "[Scoreboard] Spinner was not found");
    }

    private async void OnEnable()
    {
        _spinner.style.opacity = 1;
        _container.style.opacity = 0;

        await FetchFirebaseScores();

        _spinner.style.opacity = 0;
        _container.style.opacity = 1;
    }

    private void OnDisable()
    {
        for (int i = _container.childCount - 1; i >= 0; i--)
        {
            _container.RemoveAt(i);
        }
    }
    #endregion

    #region Firebase Methods
    private async Task FetchFirebaseScores()
    {
        // We would like to keep the user score roughly around the middle of the container:
        // The ideal scenario means there is an equivalent number of scores above and below the user score.
        int beforePadding = ItemSlotsCount / 2;

        int userScore = await GetUserScoreAsync();

        DataSnapshot before = await HighScoreRef.OrderByValue().EndAt(userScore).LimitToLast(beforePadding).GetValueAsync();

        foreach (var item in before.Children)
        {
            AddPlayers("0", item.Key, item.Value.ToString());
        }

        AddPlayers("0", AuthUser.Instance.Username, userScore.ToString(), true);

        // In the event that there are few scores above the player score (i.e., player is ranked #1),
        // we should populate more items below the player score, in order to fill in the list
        int beforeCount = (int)before.ChildrenCount;
        int afterPadding = ItemSlotsCount - beforeCount - 1;

        DataSnapshot after = await HighScoreRef.OrderByValue().StartAt(userScore).LimitToFirst(afterPadding).GetValueAsync();

        foreach (var item in after.Children)
        {
            AddPlayers("0", item.Key, item.Value.ToString());
        }
    }

    private async Task<int> GetUserScoreAsync()
    {
        DataSnapshot snap = await HighScoreRef.Child(AuthUser.Instance.UserId).GetValueAsync();
        if (snap.Exists)
        {
            return int.Parse(snap.Value.ToString());
        }
        else
        {
            return -1;
        }
    }
    #endregion

    #region Helpers
    private void AddPlayers(string rank, string username, string score, bool highlight = false)
    {
        var item = ScoreItemTemplate.CloneTree();

        item.Q<Label>("text-rank").text = SanitizeRank(rank);
        item.Q<Label>("text-username").text = SanitizeUsername(username);
        item.Q<Label>("text-score").text = SanitizeScore(score);

        if (highlight)
        {
            item.AddToClassList(HighlightClassname);
        }

        _container.Add(item);
    }

    private string SanitizeRank(string rank) => $"#{rank}";
    private string SanitizeScore(string score) => string.Format("{0:n0}", score);

    private string SanitizeUsername(string username, int characterLimit = 15)
    {
        return (username.Length <= characterLimit)
            ? username
            : username.Substring(0, characterLimit) + "…";
    }
    #endregion
}