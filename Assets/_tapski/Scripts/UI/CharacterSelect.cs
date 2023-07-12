using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
[RequireComponent(typeof(AudioSource))]
public class CharacterSelect : MonoBehaviour
{
    #region Public Properties
    [Header("Characters")]
    public GameObject[] Characters;
    [Tooltip("Scriptable object that stores the current game state")]
    public GameState State;

    public VisualElement Root { get; private set; }

    [Header("Selectors")]
    public string BtnPreviousSelector = "btn-previous";
    public string BtnNextSelector = "btn-next";
    public string CharacterPreviewSelector = "img-character";
    #endregion

    #region Private Fields
    private VisualElement _characterPreview;
    private AudioSource _btnAudio;
    private int _characterIndex = 0;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsTrue(Characters.Length > 0, "[CharacterSelect] No characters were assigned");
        Assert.IsNotNull(State, "[CharacterSelect] GameState asset is unassigned!");

        Root = GetComponent<UIDocument>().rootVisualElement;
        SetButtonHandlers(Root);

        _characterPreview = Root.Q(CharacterPreviewSelector);
        Assert.IsNotNull(_characterPreview, "[CharacterSelect] cannot find the character preview element");

        _btnAudio = GetComponent<AudioSource>();
        Assert.IsNotNull(_btnAudio, "[CharacterSelect] cannot find button AudioSource");

        CheckSavedCharacter();
        ChangeCharacter(true);
    }
    #endregion

    #region Helpers
    private void SetButtonHandlers(VisualElement root)
    {
        root.Q<Button>(BtnPreviousSelector).clicked += OnPreviousClick;
        root.Q<Button>(BtnNextSelector).clicked += OnNextClick;
    }
    #endregion

    #region Button Events
    public void OnPreviousClick()
    {
        _characterIndex--;
        if (_characterIndex < 0)
        {
            _characterIndex = Characters.Length - 1;
        }

        ChangeCharacter();
    }

    public void OnNextClick()
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

    private void ChangeCharacter(bool skipAudio = false)
    {
        var character = Characters[_characterIndex];
        State.SelectedCharacter = character;

        _characterPreview.ClearClassList();
        _characterPreview.AddToClassList(character.name);

        if (!skipAudio && State.AudioIsEnabled)
        {
            _btnAudio.Play();
        }
    }
    #endregion
}
