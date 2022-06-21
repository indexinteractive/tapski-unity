using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    #region Public Properties
    [Header("UI References")]
    public UIDocument MainMenu;
    public UIDocument CharacterSelect;

    [Header("Button Selectors")]
    public string BtnPlaySelector = "btn-play";

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

        _mainMenu = MainMenu.rootVisualElement.Children().First();

        // Activate CharacterSelect and et the offset (menu width) after the
        // first frame has been drawn, otherwise it is 'NaN'
        _mainMenu.RegisterCallback<GeometryChangedEvent>(GetResolvedMenus);

        SetButtonListeners();
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
    }

    private void SetButtonListeners()
    {
        var playBtn = _mainMenu.Q<Button>(BtnPlaySelector);
        Assert.IsNotNull(playBtn, "[MenuManager] Play button was not found");

        playBtn.clicked += OnPlayClick;
    }
    #endregion

    #region Button Events
    private void OnPlayClick()
    {
        OffsetUIDocument.Slide(_mainMenu, SlideDurationSec, 0, -_offset);
        OffsetUIDocument.Slide(_characterSelect, SlideDurationSec, _offset, 0);
    }
    #endregion
}
