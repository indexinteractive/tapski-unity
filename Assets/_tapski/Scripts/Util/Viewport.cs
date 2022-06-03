using UnityEngine;

public class ViewportHelper
{
    /// <summary>
    /// Returns a boolean indicating if the object is within the camera's
    /// screen-space boundaries, with a padding to allow for a view
    /// larger than the camera
    /// </summary>
    public static bool IsVisible(Camera camera, Transform t, float padding)
    {
        Vector3 screenPoint = camera.WorldToViewportPoint(t.position);

        float lowerLimit = 0.0f - padding;
        float upperLimit = 1.0f + padding;
        float leftLimit = 0.0f - padding;
        float rightLimit = 1.0f + padding;

        return screenPoint.y > lowerLimit && screenPoint.y < upperLimit
            && screenPoint.x > leftLimit && screenPoint.x < rightLimit;
    }

    /// <summary>
    /// Returns a boolean value indicating if a transform position is above the top
    /// of the camera view
    ///
    ///      x (item outside bounds)
    /// ------------------------------ padding
    ///      x (item within bounds)
    ///    ┌──────┐
    ///    │      │ camera view
    ///    │      │
    ///    └──────┘
    ///
    /// </summary>
    public static bool IsAboveCameraView(Camera camera, Transform t, float padding)
    {
        Vector3 screenPoint = camera.WorldToViewportPoint(t.position);

        float upperLimit = 1.0f + padding;
        return screenPoint.y < upperLimit;
    }
}
