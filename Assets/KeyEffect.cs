using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyEffect : MonoBehaviour
{
    public Color Color = Color.white;

    public float Time;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play()
    {
        spriteRenderer.color = Color;

        spriteRenderer.DOColor(Color.clear, Time);
    }
}
