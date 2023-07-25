using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Assertions;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// Handles firebase data for the scoreboard.
/// NOTE: this script must start disabled when assigned in the editor, or it will
/// attempt to communicate with firebase before initialization
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class Scoreboard : MonoBehaviour
{
    #region Public Properties
    [Header("Item Template")]
    public VisualTreeAsset ScoreItemTemplate;
    [Range(8, 15)]
    public float ItemHeight = 12;

    /// <summary>
    /// The number of items that can be displayed in the scoreboard container after styles are calculated
    /// </summary>
    public int ItemSlotsCount => Mathf.FloorToInt(_container.resolvedStyle.height / ItemHeight);

    [Header("Selectors")]
    public string ScoresContainerSelector = "item-container";
    public string SpinnerSelector = "spinner";
    public string HighlightClassname = "highlight";

    [Header("Set Username UI")]
    public GameState State;
    public UIDocument SetUsernameUI;
    public string TextInputSelector = "username-input";
    public string BtnDoneSelector = "btn-done";
    #endregion

    #region Private Fields
    private VisualElement _spinner;
    private VisualElement _container;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(ScoreItemTemplate, "[Scoreboard] Item template is unassigned");
        Assert.IsNotNull(SetUsernameUI, "[Scoreboard] Set Username UI is unassigned");
        Assert.IsNotNull(State, "[Scoreboard] Game State is unassigned");

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

        await FetchHighScoresAsync();

        _spinner.style.opacity = 0;
        _container.style.opacity = 1;
    }

    private void OnDisable()
    {
        if (_container != null)
        {
            for (int i = _container.childCount - 1; i >= 0; i--)
            {
                _container.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Database Methods
    private async UniTask FetchHighScoresAsync()
    {
        // We know the max number of items we want to display, so use that number as the above+below padding in our
        // query. This satisfies the cases where the player is in either first or last place, and we can
        // simply adjust a sliding window offset below to display the user as close to the middle as possible
        PlayerRank[] rankedPlayers = await HighscoresApi.Instance.GetPlayerScoreViewAsync(ItemSlotsCount);

        int playerIndex = -1;
        for (int i = 0; i < rankedPlayers.Length; i++)
        {
            var player = rankedPlayers[i];
            if (player.device_id == AuthUser.Instance.UserId)
            {
                playerIndex = i;

                if (State.Username != player.display_name)
                {
                    Debug.Log("Local username does not match remote! Updating local to " + player.display_name);
                    State.Username = player.display_name;
                }

                break;
            }
        }

        int idealSplit = Mathf.FloorToInt(ItemSlotsCount / 2f);
        int offset = Mathf.Clamp(playerIndex - idealSplit - 1, 0, rankedPlayers.Length - ItemSlotsCount);
        foreach (var i in rankedPlayers.Skip(offset))
        {
            bool highlight = i.device_id == AuthUser.Instance.UserId;
            AddPlayers(i.rank.ToString(), i.display_name, i.score.ToString(), highlight);
        }
    }
    #endregion

    #region Events
    public void OnScoreboardItemClicked(MouseUpEvent evt)
    {
        SetUsernameUI.gameObject.SetActive(true);
        SetUsernameUI.rootVisualElement.RegisterCallback<GeometryChangedEvent>(changedEvent =>
        {
            var input = SetUsernameUI.rootVisualElement.Q<TextField>(TextInputSelector);
            input.value = State.Username;

            SetUsernameUI.rootVisualElement.Q<Button>(BtnDoneSelector).clicked += async () =>
            {
                string inputText = SetUsernameUI.rootVisualElement.Q<TextField>(TextInputSelector).value;

                if (!string.IsNullOrEmpty(inputText) && inputText != State.Username)
                {
                    Debug.Log("Setting username to " + inputText);
                    State.Username = inputText;

                    await HighscoresApi.Instance.UpdateUsername(State.Username);
                    OnDisable();
                    await FetchHighScoresAsync();
                }

                SetUsernameUI.gameObject.SetActive(false);
            };
        });
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
            item.RegisterCallback<MouseUpEvent>(OnScoreboardItemClicked);
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