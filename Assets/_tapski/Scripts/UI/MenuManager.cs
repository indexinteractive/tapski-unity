using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public enum MenuView
{
    None,
    HighScores,
    MainMenu,
    CharacterSelect,
    GameOver
}

public class MenuManager : MonoBehaviour
{
    #region Public Properties
    [Header("UI Documents")]
    public UIDocument MainMenu;
    public UIDocument Scoreboard;
    public UIDocument GameHud;
    public TutorialOverlay Tutorial;

    [Header("Scene References")]
    public WorldGenerator GameWorld;
    public MusicManager Music;
    public CharacterSelect CharacterSelect;
    public GameOver GameOverScreen;
    public GameState State;

    [Header("Menu Input Commands")]
    public InputAction InputPrevMenu;
    public InputAction InputNextMenu;
    public InputAction InputPrevCharacter;
    public InputAction InputNextCharacter;

    [Header("Main Menu Selectors")]
    public string BtnPlaySelector = "btn-play";
    public string BtnScoreboardSelector = "btn-scoreboard";
    public string BtnAudioSelector = "btn-sound";

    [Header("Character Menu Selectors")]
    public string BtnMenuBackSelector = "btn-back";
    public string BtnStartGameSelector = "btn-start";

    [Header("Animation Parameters")]
    public float SlideDurationSec = 1;
    #endregion

    #region Private Fields
    public VisualElement Root { get; private set; }
    private VisualElement _scoreboard;
    private VisualElement _characterSelect;
    private float _offset;

    /// <summary>
    /// Tracks the current view that is shown to the player. Used only for
    /// determining what a keyboard shortcut should do at any given time
    /// </summary>
    private MenuView _currentView;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(MainMenu, "[MenuManager] MainMenu is unassigned");
        Assert.IsNotNull(Scoreboard, "[MenuManager] Scoreboard is unassigned");
        Assert.IsNotNull(CharacterSelect, "[MenuManager] CharacterSelect is unassigned");
        Assert.IsNotNull(GameHud, "[MenuManager] GameHud is unassigned");
        Assert.IsNotNull(Tutorial, "[MenuManager] Tutorial is unassigned");
        Assert.IsNotNull(GameWorld, "[MenuManager] GameWorld is unassigned");
        Assert.IsNotNull(Music, "[MenuManager] MusicManager is unassigned");
        Assert.IsNotNull(GameOverScreen, "[MenuManager] GameOverScreen is unassigned");
        Assert.IsNotNull(State, "[MenuManager] Game State is unassigned");

        Root = MainMenu.rootVisualElement.Children().First();

        // Activate CharacterSelect and get the offset (menu width) after the
        // first frame has been drawn, otherwise it is 'NaN'
        Root.RegisterCallback<GeometryChangedEvent>(GetResolvedMenus);

        InputPrevMenu.performed += OnPreviousMenuPressed;
        InputNextMenu.performed += OnNextMenuPressed;

        InputPrevCharacter.performed += OnPreviousCharacterPressed;
        InputNextCharacter.performed += OnNextCharacterPressed;

        Activate();
    }

    private void OnDisable()
    {
        Debug.Log("!!! Disable all inputs!!");
        InputPrevMenu.performed -= OnPreviousMenuPressed;
        InputNextMenu.performed -= OnNextMenuPressed;
        DisableAllInputs();
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Called externally by <see cref="GameOver.OnBtnBackClicked" /> to activate
    /// keyboard / controller inputs again
    /// </summary>
    public void Activate()
    {
        _currentView = MenuView.MainMenu;
        EnableNavigationInputs();
    }

    private void GetResolvedMenus(GeometryChangedEvent e)
    {
        Root.UnregisterCallback<GeometryChangedEvent>(GetResolvedMenus);

        CharacterSelect.gameObject.SetActive(true);

        _characterSelect = CharacterSelect.Root.Children().First();
        _offset = Root.resolvedStyle.width;

        OffsetUIDocument.Slide(_characterSelect, 0, 0, _offset);

        Scoreboard.gameObject.SetActive(true);
        _scoreboard = Scoreboard.rootVisualElement.Children().First();
        OffsetUIDocument.Slide(_scoreboard, 0, 0, -_offset);

        SetButtonListeners();
    }

    private void SetAudioImage(Button btnAudio, bool enabled)
    {
        btnAudio.ClearClassList();
        btnAudio.AddToClassList($"audio-{State.AudioIsEnabled}");
    }

    private void SetButtonListeners()
    {
        var btnPlay = Root.Q<Button>(BtnPlaySelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Play button was not found");

        btnPlay.clicked += OnPlayClick;

        var btnScoreboard = Root.Q<Button>(BtnScoreboardSelector);
        Assert.IsNotNull(btnScoreboard, "[MenuManager] Scoreboard button was not found");

        btnScoreboard.clicked += OnScoreboardClick;

        var btnAudio = Root.Q<Button>(BtnAudioSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Audio button was not found");
        btnAudio.clicked += () => OnAudioBtnClick(btnAudio);
        SetAudioImage(btnAudio, State.AudioIsEnabled);

        var btnMenuBack = CharacterSelect.Root.Q<Button>(BtnMenuBackSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Back button was not found");
        btnMenuBack.clicked += OnMenuBackClick;

        var btnScoreboardBack = Scoreboard.rootVisualElement.Q<Button>(BtnMenuBackSelector);
        Assert.IsNotNull(btnScoreboardBack, "[MenuManager] Scoreboard Back button was not found");
        btnScoreboardBack.clicked += OnScoreboardBackClick;

        var btnStartGame = CharacterSelect.Root.Q<Button>(BtnStartGameSelector);
        Assert.IsNotNull(btnStartGame, "[MenuManager] Start button was not found");
        btnStartGame.clicked += OnStartGameClick;
    }

    private void DisableCharacterSelectInputs()
    {
        InputNextCharacter.Disable();
        InputPrevCharacter.Disable();
    }

    private void EnableCharacterSelectInputs()
    {
        InputNextCharacter.Enable();
        InputPrevCharacter.Enable();
    }

    private void EnableNavigationInputs()
    {
        InputPrevMenu.Enable();
        InputNextMenu.Enable();
    }

    private void DisableAllInputs()
    {
        InputPrevMenu.Disable();
        InputNextMenu.Disable();
        DisableCharacterSelectInputs();
    }
    #endregion

    #region Input Events
    private void OnPreviousMenuPressed(InputAction.CallbackContext context)
    {
        switch (_currentView)
        {
            case MenuView.MainMenu:
                OnScoreboardClick();
                break;
            case MenuView.CharacterSelect:
                DisableCharacterSelectInputs();
                OnMenuBackClick();
                break;
            case MenuView.HighScores:
                OnScoreboardBackClick();
                break;
        }
    }
    private void OnNextMenuPressed(InputAction.CallbackContext context)
    {
        switch (_currentView)
        {
            case MenuView.MainMenu:
                EnableCharacterSelectInputs();
                OnPlayClick();
                break;
            case MenuView.CharacterSelect:
                DisableAllInputs();
                OnStartGameClick();
                break;
            case MenuView.HighScores:
                OnScoreboardBackClick();
                break;
        }
    }

    private void OnNextCharacterPressed(InputAction.CallbackContext context)
    {
        CharacterSelect.OnNextClick();
    }

    private void OnPreviousCharacterPressed(InputAction.CallbackContext context)
    {
        CharacterSelect.OnPreviousClick();
    }
    #endregion

    #region Button Events
    private void OnPlayClick()
    {
        OffsetUIDocument.Slide(Root, SlideDurationSec, 0, -_offset);
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, _offset, 0);
        _currentView = MenuView.CharacterSelect;
    }

    private void OnScoreboardClick()
    {
        var script = Scoreboard.gameObject.GetComponent<Scoreboard>();
        script.enabled = true;

        // TODO: Rename OffsetUIDocument to UITransition
        OffsetUIDocument.Slide(Root, SlideDurationSec, 0, _offset);
        OffsetUIDocument.Slide(_scoreboard, SlideDurationSec, -_offset, 0);
        _currentView = MenuView.HighScores;
    }

    private void OnScoreboardBackClick()
    {
        var script = Scoreboard.gameObject.GetComponent<Scoreboard>();
        script.enabled = false;

        OffsetUIDocument.Slide(Root, SlideDurationSec, _offset, 0);
        OffsetUIDocument.Slide(_scoreboard, SlideDurationSec, 0, -_offset);
        _currentView = MenuView.MainMenu;
    }

    private void OnMenuBackClick()
    {
        OffsetUIDocument.Slide(Root, SlideDurationSec, -_offset, 0);
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, 0, _offset);
        _currentView = MenuView.MainMenu;
    }

    private void OnAudioBtnClick(Button btnAudio)
    {
        State.AudioIsEnabled = !State.AudioIsEnabled;
        SetAudioImage(btnAudio, State.AudioIsEnabled);
    }

    private async void OnStartGameClick()
    {
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, 0, -_offset);
        await Task.Delay(TimeSpan.FromSeconds(SlideDurationSec));

        if (State.WillShowTutorial)
        {
            Debug.Log("Game will show tutorial");
            Tutorial.gameObject.SetActive(true);
            await Tutorial.WaitForSkipAsync();
            Debug.Log("Tutorial dismissed");
            Tutorial.gameObject.SetActive(false);
        }

        GameWorld.StartNewGame(GameOverScreen.OnPlayerDead);
        Music.SwitchToGame();
        GameHud.gameObject.SetActive(true);
        _currentView = MenuView.None;
    }
    #endregion
}
