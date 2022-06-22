using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class ScoreKeeper : MonoBehaviour
{
    #region Public Properties
    [Header("References")]
    public GameState State;
    public UIDocument HUD;

    [Header("Selectors")]
    public string ScoreSelector = "text-score";
    #endregion

    #region Private Fields
    private Label _score;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(State, "[ScoreKeeper] Game State is unassigned");
        Assert.IsNotNull(HUD, "[ScoreKeeper] UI HUD is unassigned");

        _score = HUD.rootVisualElement.Q<Label>(ScoreSelector);
        Assert.IsNotNull(_score, "[ScoreKeeper] Could not find a score label");
    }

    private void Update()
    {
        if (_score.text != State.SessionScore.ToString())
        {
            _score.text = State.SessionScore.ToString();
        }
    }
    #endregion
}
