using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

/// <summary>
/// Makes up the score part of the in-game HUD
/// </summary>
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
    private void OnEnable()
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
