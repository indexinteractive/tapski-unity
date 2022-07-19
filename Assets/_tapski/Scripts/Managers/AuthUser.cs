using UnityEngine;

public class AuthUser : Singleton<AuthUser>
{
    #region Properties
    public string UserId => SystemInfo.deviceUniqueIdentifier;
    #endregion
}
