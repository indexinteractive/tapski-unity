using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    #region Public Properties
    [Header("References")]
    public UIDocument MainMenu;
    public UIDocument CharacterSelect;
    public GameState State;

    [Header("Button Selectors")]
    public string BtnPlaySelector = "btn-play";
    public string BtnMenuBackSelector = "btn-back";
    public string BtnAudioSelector = "btn-sound";

    [Header("Animation Parameters")]
    public float SlideDurationSec = 1;
    #endregion

    #region Private Fields
    private VisualElement _mainMenu;
    private VisualElement _characterSelect;
    private float _offset;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(MainMenu, "[MenuManager] MainMenu is unassigned");
        Assert.IsNotNull(CharacterSelect, "[MenuManager] CharacterSelect is unassigned");
        Assert.IsNotNull(State, "[MenuManager] Game State is unassigned");

        _mainMenu = MainMenu.rootVisualElement.Children().First();

        // Activate CharacterSelect and et the offset (menu width) after the
        // first frame has been drawn, otherwise it is 'NaN'
        _mainMenu.RegisterCallback<GeometryChangedEvent>(GetResolvedMenus);
    }
    #endregion

    #region Helpers
    private void GetResolvedMenus(GeometryChangedEvent evt)
    {
        _mainMenu.UnregisterCallback<GeometryChangedEvent>(GetResolvedMenus);

        CharacterSelect.gameObject.SetActive(true);

        _characterSelect = CharacterSelect.rootVisualElement.Children().First();
        _offset = _mainMenu.resolvedStyle.width;

        OffsetUIDocument.Slide(_characterSelect, 0, 0, _offset);

        SetButtonListeners();
    }

    private void SetAudioImage(Button btnAudio, bool enabled)
    {
        btnAudio.ClearClassList();
        btnAudio.AddToClassList($"audio-{State.AudioIsEnabled}");
    }

    private void SetButtonListeners()
    {
        var btnPlay = _mainMenu.Q<Button>(BtnPlaySelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Play button was not found");

        btnPlay.clicked += OnPlayClick;

        var btnAudio = _mainMenu.Q<Button>(BtnAudioSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Audio button was not found");
        btnAudio.clicked += () => OnAudioBtnClick(btnAudio);
        SetAudioImage(btnAudio, State.AudioIsEnabled);

        var btnMenuBack = CharacterSelect.rootVisualElement.Q<Button>(BtnMenuBackSelector);
        Assert.IsNotNull(btnPlay, "[MenuManager] Back button was not found");
        btnMenuBack.clicked += OnMenuBackClick;
    }
    #endregion

    #region Button Events
    private void OnPlayClick()
    {
        OffsetUIDocument.Slide(_mainMenu, SlideDurationSec, 0, -_offset);
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, _offset, 0);
    }

    private void OnMenuBackClick()
    {
        OffsetUIDocument.Slide(_mainMenu, SlideDurationSec, -_offset, 0);
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, 0, _offset);
    }

    private void OnAudioBtnClick(Button btnAudio)
    {
        State.AudioIsEnabled = !State.AudioIsEnabled;
        SetAudioImage(btnAudio, State.AudioIsEnabled);
    }
    #endregion
}
