using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject timeGO;
    [SerializeField] private GameObject longGO;

    [Header("Color")]
    public Color EnabledColor = Color.white;
    public Color DisabledColor = Color.white;
    public Color LongEnabledColor = Color.white;
    public Color LongDisabledColor = Color.white;

    public int Index { get; private set; } = 0;
    public double Time { get; private set; } = 0.0;
    public double Length { get; private set; } = 0.0;

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
        NoteManager manager = NoteManager.Instance;

        transform.position = manager.GetPosition(Index, Time);

        longGO.transform.localPosition  = new Vector3(0f, (float)Length * manager.UnitPerSecond * manager.Speed * 0.5f, 0f);
        longGO.transform.localScale     = new Vector3(1f, (float)Length * manager.UnitPerSecond * manager.Speed, 1f);
    }

    public void Init(int index, double time, double length)
    {
        Index = index;
        Time = time;
        Length = length;

        Update();
    }

    public void SetWorking(bool value)
    {
        IsWorking = value;

        timeSpriteRenderer.color = value ? EnabledColor : DisabledColor;
        longSpriteRenderer.color = value ? LongEnabledColor : LongDisabledColor;
    }
}
