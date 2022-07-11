using UnityEngine;

/// <summary>
/// Generic Singleton Pattern decorator
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : class, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }
}

/// <summary>
/// Singleton pattern decorator that inherits from MonoBehaviour
/// to create an instance inside of Awake()
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonUnity<T> : MonoBehaviour where T : class, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        _instance = this as T;
    }
    #endregion
}
