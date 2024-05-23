using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapTarget : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites;
    private Color color;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }
    public enum TargetType { LocalPlayer, OtherPlayer, BasicMonster, BossMonster, Vehicle, Item }

    public void Init(TargetType targetType)
    {
        float scale = 3f;
        if (targetType == TargetType.LocalPlayer)
        {
            scale = 5f;
            color = Color.green;
        }
        else if (targetType == TargetType.OtherPlayer)
        {
            color = Color.yellow;
        }
        else if (targetType == TargetType.BasicMonster || targetType == TargetType.BossMonster)
        {
            color = Color.red;
        }
        else if (targetType == TargetType.Vehicle)
        {
            color = Color.blue;

        }
        else if (targetType == TargetType.Item)
        {
            color = Color.magenta;
        }


        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprites[(int)targetType];
        spriteRenderer.color = color;
        transform.localScale = new Vector3(scale, scale, scale);

    }
}
