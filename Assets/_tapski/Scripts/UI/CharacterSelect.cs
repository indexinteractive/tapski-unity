using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CharacterSelect : MonoBehaviour
{
    #region Public Properties
    [Header("Characters")]
    public GameObject[] Characters;
    [Tooltip("Scriptable object that stores the current game state")]
    public GameState State;

    [Header("Selectors")]
    public string BtnPreviousSelector = "btn-previous";
    public string BtnNextSelector = "btn-next";
    public string BtnStartSelector = "btn-start";
    public string CharacterPreviewSelector = "img-character";
    #endregion

    #region Private Fields
    private VisualElement _uiRoot;
    private VisualElement _characterPreview;
    private int _characterIndex = 0;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsTrue(Characters.Length > 0, "[CharacterSelect] No characters were assigned");
        Assert.IsNotNull(State, "[CharacterSelect] GameState asset is unassigned!");

        _uiRoot = GetComponent<UIDocument>().rootVisualElement;
        _uiRoot.Q<Button>(BtnPreviousSelector).clicked += OnPreviousClick;
        _uiRoot.Q<Button>(BtnNextSelector).clicked += OnNextClick;

        _characterPreview = _uiRoot.Q(CharacterPreviewSelector);
        Assert.IsNotNull(_characterPreview, "[CharacterSelect] cannot find the character preview element");

        CheckSavedCharacter();
        ChangeCharacter();
    }
    #endregion

    #region Button Events
    private void OnPreviousClick()
    {
        _characterIndex--;
        if (_characterIndex < 0)
        {
            _characterIndex = Characters.Length - 1;
        }

        ChangeCharacter();
    }

    private void OnNextClick()
    {
        _characterIndex = ++_characterIndex % Characters.Length;
        ChangeCharacter();
    }
    #endregion

    #region Character Handling
    private void CheckSavedCharacter()
    {
        if (State.SelectedCharacter != null)
        {
            _characterIndex = Array.IndexOf(Characters, State.SelectedCharacter);
        }
    }

    private void ChangeCharacter()
    {
        var character = Characters[_characterIndex];
        State.SelectedCharacter = character;

        _characterPreview.ClearClassList();
        _characterPreview.AddToClassList(character.name);
    }
    #endregion
}
