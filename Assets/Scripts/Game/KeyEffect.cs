using DG.Tweening;
using DG.Tweening.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyEffect : MonoBehaviour
{
    public Color Color = Color.white;
    public Color Clear = Color.clear;

    public float Time;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private TweenerCore<Color, Color, DG.Tweening.Plugins.Options.ColorOptions> core;

    public void Play()
    {
        spriteRenderer.color = Color;

        if (core != null)
        {
            core.Kill();
        }

        core = spriteRenderer.DOColor(Clear, Time);
    }
}
