using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

public enum MenuView
{
    None,
    HighScores,
    MainMenu,
    CharacterSelect,
    Gameplay,
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

    private void SlideIn(VisualElement element, bool slideLeft)
    {
        UITransition.Slide(element, SlideDurationSec, (slideLeft ? 1 : -1) * _offset, 0);
    }

    private void SlideOut(VisualElement element, bool slideLeft)
    {
        UITransition.Slide(element, SlideDurationSec, 0, (slideLeft ? -1 : 1) * _offset);
    }
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
        ChangeMenuState(MenuView.MainMenu);
    }

    private void GetResolvedMenus(GeometryChangedEvent e)
    {
        Root.UnregisterCallback<GeometryChangedEvent>(GetResolvedMenus);

        CharacterSelect.gameObject.SetActive(true);

        _characterSelect = CharacterSelect.Root.Children().First();
        _offset = Root.resolvedStyle.width;

        UITransition.Slide(_characterSelect, 0, 0, _offset);

        Scoreboard.gameObject.SetActive(true);
        _scoreboard = Scoreboard.rootVisualElement.Children().First();
        UITransition.Slide(_scoreboard, 0, 0, -_offset);

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

        btnPlay.clicked += OnNextMenuPressed;

        var btnScoreboard = Root.Q<Button>(BtnScoreboardSelector);
        Assert.IsNotNull(btnScoreboard, "[MenuManager] Scoreboard button was not found");

        btnScoreboard.clicked += OnPreviousMenuPressed;

        var btnAudio = Root.Q<Button>(BtnAudioSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Audio button was not found");
        btnAudio.clicked += () => OnAudioBtnClick(btnAudio);
        SetAudioImage(btnAudio, State.AudioIsEnabled);

        var btnMenuBack = CharacterSelect.Root.Q<Button>(BtnMenuBackSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Back button was not found");
        btnMenuBack.clicked += OnPreviousMenuPressed;

        var btnScoreboardBack = Scoreboard.rootVisualElement.Q<Button>(BtnMenuBackSelector);
        Assert.IsNotNull(btnScoreboardBack, "[MenuManager] Scoreboard Back button was not found");
        btnScoreboardBack.clicked += OnPreviousMenuPressed;

        var btnStartGame = CharacterSelect.Root.Q<Button>(BtnStartGameSelector);
        Assert.IsNotNull(btnStartGame, "[MenuManager] Start button was not found");
        btnStartGame.clicked += OnNextMenuPressed;
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
            case MenuView.MainMenu: ChangeMenuState(MenuView.HighScores); break;
            case MenuView.CharacterSelect: ChangeMenuState(MenuView.MainMenu); break;
            case MenuView.HighScores: ChangeMenuState(MenuView.MainMenu); break;
        }
    }

    private void OnNextMenuPressed(InputAction.CallbackContext context)
    {
        switch (_currentView)
        {
            case MenuView.MainMenu: ChangeMenuState(MenuView.CharacterSelect); break;
            case MenuView.CharacterSelect: ChangeMenuState(MenuView.Gameplay); break;
            case MenuView.HighScores: ChangeMenuState(MenuView.MainMenu); break;
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
    /// <summary>
    /// Exists only to allow button handlers such as btnPlay.clicked in <see cref="SetButtonListeners" /> to pass
    /// their logic onto <see cref="OnNextMenuPressed" /> which handles the actual logic.
    /// </summary>
    private void OnNextMenuPressed()
    {
        OnNextMenuPressed(default(InputAction.CallbackContext));
    }

    private void OnPreviousMenuPressed()
    {
        OnPreviousMenuPressed(default(InputAction.CallbackContext));
    }

    private void OnAudioBtnClick(Button btnAudio)
    {
        State.AudioIsEnabled = !State.AudioIsEnabled;
        SetAudioImage(btnAudio, State.AudioIsEnabled);
    }

    public async void ChangeMenuState(MenuView value)
    {
        Debug.Log($"Switching from {_currentView} to {value}");
        switch (value)
        {
            case MenuView.CharacterSelect:
                EnableCharacterSelectInputs();
                SlideOut(Root, slideLeft: true);
                SlideIn(_characterSelect, slideLeft: true);
                break;
            case MenuView.GameOver:
                break;
            case MenuView.HighScores:
                Scoreboard.gameObject.GetComponent<Scoreboard>().enabled = true;
                SlideOut(Root, slideLeft: false);
                SlideIn(_scoreboard, slideLeft: false);
                break;
            case MenuView.MainMenu:
                if (_currentView == MenuView.CharacterSelect)
                {
                    DisableCharacterSelectInputs();
                    SlideOut(_characterSelect, slideLeft: false);
                    SlideIn(Root, slideLeft: false);
                }
                else if (_currentView == MenuView.HighScores)
                {
                    Scoreboard.gameObject.GetComponent<Scoreboard>().enabled = false;
                    SlideOut(_scoreboard, slideLeft: true);
                    SlideIn(Root, slideLeft: true);
                }

                EnableNavigationInputs();
                break;
            case MenuView.Gameplay:
                DisableAllInputs();

                if (_currentView == MenuView.CharacterSelect)
                {
                    SlideOut(_characterSelect, slideLeft: true);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(SlideDurationSec));

                if (State.WillShowTutorial)
                {
                    Debug.Log("Game will show tutorial");
                    Tutorial.gameObject.SetActive(true);
                    await Tutorial.WaitForSkipAsync();
                    Debug.Log("Tutorial dismissed");
                    Tutorial.gameObject.SetActive(false);
                }

                Music.SwitchToGame();
                GameHud.gameObject.SetActive(true);

                GameWorld.StartNewGame(GameOverScreen.OnPlayerDead);
                break;
            case MenuView.None:
                break;
        }

        _currentView = value;
    }
    #endregion
}
