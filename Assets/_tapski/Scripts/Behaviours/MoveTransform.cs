using UnityEngine;

public class MoveTransform : MonoBehaviour
{
    #region Public Properties
    public Vector3 Speed;
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        transform.position = new Vector3(
            transform.position.x + Speed.x * Time.deltaTime,
            transform.position.y + Speed.y * Time.deltaTime,
            transform.position.z + Speed.z * Time.deltaTime
        );
    }
    #endregion
}
