using UnityEngine;

public class PathStep : MonoBehaviour
{
    #region Public Properties
    public float Width
    {
        get { return transform.localScale.x; }
        set
        {
            transform.localScale = new Vector3(value, transform.localScale.y, transform.localScale.z);
        }
    }

    public float LeftEdge
    {
        get { return transform.position.x - (Width / 2); }
    }

    public float RightEdge
    {
        get { return transform.position.x + (Width / 2); }
    }
    #endregion

    #region Public Methods
    public bool IsVisibleInCamera(Camera viewCamera, float padding)
    {
        Vector3 left = viewCamera.WorldToViewportPoint(new Vector3(LeftEdge, 0, 0));
        Vector3 right = viewCamera.WorldToViewportPoint(new Vector3(RightEdge, 0, 0));
        return !(right.x < 0 - padding || left.x > 1 + padding);
    }
    #endregion

    #region Static Initialization
    public static PathStep Create(GameObject prefab, Vector2 position, GameObject parent, float width)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.parent = parent.transform;

        var path = instance.AddComponent<PathStep>();
        path.Width = width;

        return path;
    }
    #endregion
}
