using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("General")]
    [SerializeField] internal GameObject timeGO;
    [SerializeField] internal GameObject longGO;

    [Header("Color")]
    public Color EnabledColor = Color.white;
    public Color DisabledColor = Color.white;
    public Color LongEnabledColor = Color.white;
    public Color LongDisabledColor = Color.white;

    public int Index { get; private set; } = 0;
    public double Time { get; private set; } = 0.0;
    public double Length { get; private set; } = 0.0;
    public float Position { get; private set; } = 0f;
    public float Size { get; private set; } = 0f;

    public bool IsLong => Length > 0.0;

    public bool IsWorking { get; private set; }

    private SpriteRenderer timeSpriteRenderer;
    private SpriteRenderer longSpriteRenderer;

    private void Awake()
    {
        timeSpriteRenderer = timeGO.GetComponent<SpriteRenderer>();
        longSpriteRenderer = longGO.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetWorking(true);
    }

    private void Update()
    {
        NoteManager.Instance.SetTransform(this);
    }

    public void Init(int index, double time, double length, float position, float size)
    {
        Index = index; Time = time; Length = length; Position = position; Size = size;

        Update();
    }

    public void SetWorking(bool value)
    {
        IsWorking = value;

        timeSpriteRenderer.color = value ? EnabledColor : DisabledColor;
        longSpriteRenderer.color = value ? LongEnabledColor : LongDisabledColor;
    }
}
