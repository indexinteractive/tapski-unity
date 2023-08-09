using UnityEngine;
using System.IO;

public class Screenshot : MonoBehaviour
{
    [Header("Screenshot Settings")]
    public string screenshotNamePrefix = "Screenshot";

#if UNITY_EDITOR
    [Button]
#endif
    private void CaptureScreenshot()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string screenshotFileName = $"{screenshotNamePrefix}_{timestamp}.png";
        string screenshotPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), screenshotFileName);

        ScreenCapture.CaptureScreenshot(screenshotPath);

        Debug.Log($"Screenshot saved to: {screenshotPath}");
    }
}
