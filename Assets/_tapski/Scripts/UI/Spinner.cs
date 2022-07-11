using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class Spinner : MonoBehaviour
{
    #region Public Properties
    public string SpinnerSelector = "spinner";
    public int SpinnerUpdateMs = 200;
    #endregion

    #region Private Fields
    private List<VisualElement> _spinnerElements;
    private int _lastElementIndex;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var spinner = root.Q<VisualElement>(SpinnerSelector).Children().First();

        Assert.IsNotNull(spinner, "[Spinner] Spinner is unassigned");

        _spinnerElements = new List<VisualElement>();
        foreach (var element in spinner.Children())
        {
            _spinnerElements.Add(element);
            element.style.opacity = 0f;
        }

        _lastElementIndex = 0;
        spinner.schedule.Execute(UpdateSpinner).Every(SpinnerUpdateMs);
    }

    private void OnDisable()
    {
        _spinnerElements.Clear();
    }
    #endregion

    #region Spinner Events
    private void UpdateSpinner()
    {
        _spinnerElements[((_lastElementIndex - 2) + _spinnerElements.Count) % _spinnerElements.Count].style.opacity = 0f;
        _spinnerElements[((_lastElementIndex - 1) + _spinnerElements.Count) % _spinnerElements.Count].style.opacity = 0.35f;

        _spinnerElements[_lastElementIndex].style.opacity = 0.65f;
        _lastElementIndex = (_lastElementIndex + 1) % _spinnerElements.Count;
        _spinnerElements[_lastElementIndex].style.opacity = 1f;
    }
    #endregion
}
