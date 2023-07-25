using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class OffsetUIDocument : MonoBehaviour
{
    #region Public Properties
    public float SlideDurationSec = 2;
    #endregion

    #region Private Fields
    private VisualElement _uiRoot;
    private float _screenWidth;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        var _uiDoc = GetComponent<UIDocument>();
        _uiRoot = _uiDoc.rootVisualElement.Children().First();

        Assert.IsTrue(_uiRoot.resolvedStyle.position == Position.Absolute, "[OffsetUIDocument] UI root element must have position:absolute");

        _screenWidth = Screen.width;
    }
    #endregion

    #region Panel Movement
#if UNITY_EDITOR
    [Button]
#endif
    public void SlideRight()
    {
        Slide(SlideDurationSec, 0, _screenWidth);
    }

#if UNITY_EDITOR
    [Button]
#endif
    public void SlideLeft()
    {
        Slide(SlideDurationSec, 0, -_screenWidth);
    }

    public async void Slide(float durationSec, float startValue, float endValue)
    {
        float timeElapsed = 0;
        while (timeElapsed < durationSec)
        {
            _uiRoot.style.left = Mathf.Lerp(startValue, endValue, timeElapsed / durationSec);

            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        _uiRoot.style.left = endValue;
    }
    #endregion

    #region Static Methods
    public static async void Slide(VisualElement root, float durationSec, float startValue, float endValue)
    {
        Assert.IsTrue(root.resolvedStyle.position == Position.Absolute, "[OffsetUIDocument] root element must have position:absolute");

        float timeElapsed = 0;
        while (timeElapsed < durationSec)
        {
            root.style.left = Mathf.Lerp(startValue, endValue, timeElapsed / durationSec);

            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        root.style.left = endValue;
    }
    #endregion
}
