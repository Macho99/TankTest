using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZombieInfoUI : MonoBehaviour
{

    [SerializeField] private ZombieInfoSlot[] slots;
    private bool isOpened;
    private Animator animator;
    public enum ZombieType { Brute, Wretch, Noraml }
    private void Awake()
    {
        isOpened = false;
        animator = GetComponent<Animator>();
    }

    public void Init(ZombieType zombieType, int count)
    {
        slots[(int)zombieType].UpdateSlot(count);
    }
    public ZombieInfoSlot GetSlot(ZombieType zombieType)
    {
        return slots[(int)zombieType];
    }
    public void PressActiveButton()
    {
        isOpened = !isOpened;
        animator.SetBool("Open", isOpened);
        Debug.Log(isOpened);
    }

}

