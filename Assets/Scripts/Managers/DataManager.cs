using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private FirebaseDatabase database;
    private void Awake()
    {
        database = FirebaseDatabase.DefaultInstance;
      
    }
    public void SaveData<T>(string reference, T data) where T : class
    {
        DatabaseReference databaseRef = database.GetReference(reference);

        string jsonData = JsonUtility.ToJson(data);

        databaseRef.SetRawJsonValueAsync(jsonData);
    }
    public async void SearchUserData<T>(string reference, Action<TaskStatus> action = null)
    {
        string uid = GameManager.auth.User.UserId;
        DatabaseReference databaseRef = database.GetReference($"users/{uid}/{reference}");

        await databaseRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log(snapshot.GetValue(true));
            }
            action?.Invoke(task.Status);

        });




    }

}
public class UserData
{
    public string nicname;

}