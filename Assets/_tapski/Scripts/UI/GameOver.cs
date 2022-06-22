using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameOver : MonoBehaviour
{
    #region Public Properties
    [Header("UI Documents")]
    public UIDocument MainMenu;
    public UIDocument GameHud;

    [Header("References")]
    public WorldGenerator GameWorld;
    public GameState State;

    [Header("Selectors")]
    public string TextScoreSelector = "text-score";
    public string TextHighScoreSelector = "text-high-score";
    public string TextNewHighScoreSelector = "new-high-score";
    public string BtnBackSelector = "btn-back";
    public string BtnRetrySelector = "btn-retry";

    [Header("Animation Parameters")]
    public float SlideDurationSec = 0.3f;
    #endregion

    #region Private Fields
    private VisualElement _mainMenu;
    private VisualElement _gameOver;
    private float _offset;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        Assert.IsNotNull(GameWorld, "[GameOver] Game World is unassigned");
        Assert.IsNotNull(State, "[GameOver] Game State is unassigned");

        var root = GetComponent<UIDocument>().rootVisualElement;
        SetButtons(root);

        _gameOver = root.Children().First();
        _mainMenu = MainMenu.rootVisualElement.Children().First();
        _offset = _mainMenu.resolvedStyle.width;

        _gameOver.Q<Label>(TextNewHighScoreSelector).style.display = DisplayStyle.None;

        OffsetUIDocument.Slide(_gameOver, 0, 0, _offset);
        OffsetUIDocument.Slide(_gameOver, SlideDurationSec, _offset, 0);
    }
    #endregion

    #region Helpers
    private void SetButtons(VisualElement root)
    {
        var btnBack = root.Q<Button>(BtnBackSelector);
        btnBack.clicked += OnBtnBackClicked;

        var btnRetry = root.Q<Button>(BtnRetrySelector);
        btnRetry.clicked += OnBtnRetryClicked;
    }
    #endregion

    #region Game Events
    public void OnPlayerDead()
    {
        this.gameObject.SetActive(true);
        GameHud.gameObject.SetActive(false);

        var isNewHigh = State.SetHighScore(State.SessionScore);
        if (isNewHigh)
        {
            _gameOver.Q<Label>(TextNewHighScoreSelector).style.display = DisplayStyle.Flex;
        }

        var labelScore = _gameOver.Q<Label>(TextScoreSelector);
        Assert.IsNotNull(labelScore, "[GameOver] Score Label was not found");
        labelScore.text = State.SessionScore.ToString();

        var labelHighScore = _gameOver.Q<Label>(TextHighScoreSelector);
        Assert.IsNotNull(labelHighScore, "[GameOver] High Score Label was not found");
        labelHighScore.text = State.DeviceHighScore.ToString();
    }
    #endregion

    #region Button Events
    private async void OnBtnRetryClicked()
    {
        OffsetUIDocument.Slide(_gameOver, SlideDurationSec, 0, _offset);
        await Task.Delay(TimeSpan.FromSeconds(SlideDurationSec));

        GameWorld.StartNewGame(OnPlayerDead);

        this.gameObject.SetActive(false);
        GameHud.gameObject.SetActive(true);
    }

    private void OnBtnBackClicked()
    {
        OffsetUIDocument.Slide(_gameOver, SlideDurationSec, 0, _offset);
        OffsetUIDocument.Slide(_mainMenu, SlideDurationSec, -_offset, 0);

        GameWorld.EndGame();

        this.gameObject.SetActive(false);
        GameHud.gameObject.SetActive(false);
    }
    #endregion
}
