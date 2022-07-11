using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class AuthUser : Singleton<AuthUser>
{
    #region Properties
    public FirebaseAuth Auth => Firebase.Auth.FirebaseAuth.DefaultInstance;

    public FirebaseUser User
    {
        get
        {
            if (Auth.CurrentUser == null)
            {
                throw new System.Exception("User is not logged in");
            }
            return Auth.CurrentUser;
        }
    }

    public string UserId => User.UserId;
    public string Username => User.DisplayName;
    #endregion

    #region User Handling
    public async Task WaitForUserAsync()
    {
        await RunAndroidChecksAsync();

        bool finished = false;
        Auth.StateChanged += async (object sender, EventArgs e) =>
        {
            if (Auth.CurrentUser != null)
            {
                Debug.LogFormat("User is already logged in: {0} ({1})", User.DisplayName, User.UserId);
            }
            else
            {
                await SignInAsync();
            }

            finished = true;
        };

        while (!finished)
        {
            await Task.Yield();
        }
    }

    public async Task SignInAsync()
    {
        Debug.Log("Logging in anonymous user");

        await Auth.SignInAnonymouslyAsync();

        if (User != null)
        {
            if (string.IsNullOrWhiteSpace(User.DisplayName))
            {
                await SetDisplayName();
            }

            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.UserId);
        }
        else
        {
            Debug.Log("Unable to sign in");
        }
    }

    private async Task SetDisplayName()
    {
        Debug.Log("calling SetDisplayName on user " + User.DisplayName);

        var generator = new UsernameGenerator();
        await User.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
        {
            DisplayName = generator.Generate()
        })
        .ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UpdateUserProfileAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("User profile updated successfully.");
        });
    }
    #endregion

    #region Helpers
    private Task RunAndroidChecksAsync()
    {
        try
        {
            return FirebaseApp.CheckAndFixDependenciesAsync();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return Task.CompletedTask;
        }
    }
    #endregion
}
