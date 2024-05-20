using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class AuthManager : MonoBehaviour
{

    private FirebaseAuth auth;
    private FirebaseUser user;


    public FirebaseAuth Auth { get { return auth; } }
    public FirebaseUser User { get { return user; } }
    private void Awake()
    {
        Init();

    }
    private void Init()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        print(auth);
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }
    public async Task UpdateUserProfile(string nicname)
    {
        try
        {
            UserProfile userProfile = new UserProfile();
            userProfile.DisplayName = nicname;

            await auth.CurrentUser.UpdateUserProfileAsync(userProfile);


        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    public async Task<AuthResult> SignUpWithEmailAndPassword(string email, string password, string nicname)
    {
        if (auth == null)
        {
            Debug.Log($"AuthManager : {auth}");
            return null;
        }

        try
        {
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            if (result.User != null)
            {
                await UpdateUserProfile(nicname);
            }
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }


        return null;

    }
    public async Task<AuthResult> LoginWithEmailAndPassword(string email, string password)
    {
        if (auth == null)
        {
            Debug.Log($"AuthManager : {auth}");
            return null;
        }

        try
        {

            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result.User != null)
            {
                user = result.User;
            }
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }


        return null;

    }
    public async Task<AuthResult> LoginWithGuest()
    {
        if (auth == null)
        {
            Debug.Log($"AuthManager : {auth}");

            return null;
        }

        AuthResult result = await auth.SignInAnonymouslyAsync();

        if (result != null)
        {
            user = result.User;

            return result;
        }

        return null;
    }
    public void SignOut()
    {
        auth.SignOut();
    }
    public async void DeletUser()
    {
        if (user != null)
        {
            await user.DeleteAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("DeleteAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User deleted successfully.");
            });
        }
    }
}
