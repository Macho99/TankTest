using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    public event Action onStartDrawWeapon;
    public event Action onEndDrawWeapon;
    public void OnStartDrawWeapon()
    {
        // equipment;

        onStartDrawWeapon?.Invoke();
    }
    public void OnEndDrawWeapon()
    {
        onEndDrawWeapon?.Invoke();

        onStartDrawWeapon = null;
        onEndDrawWeapon = null;
    }
}
