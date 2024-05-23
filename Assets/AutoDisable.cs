using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    [SerializeField] private float Activetimer = 5f;
    private float currentTimer;
    public void OnEnable()
    {
        currentTimer = Activetimer;
    }
    private void Update()
    {
        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0f)
            GameManager.Pool.Release(this.gameObject);
    }
}
