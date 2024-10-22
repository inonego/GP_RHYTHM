using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class NoteQueue : Queue<Note> { }

[RequireComponent(typeof(Pool))]
public class NoteManager : MonoSingleton<NoteManager>
{
    #region �ʵ� ����

    public InputBindingType InputType { get; private set; }

    public Vector3 Offset;
    public float Space = 1f;


    public double DespawnTime = 1f;

    public double CurrentPlayTime { get; private set; }

    private List<Note> spawned = new List<Note>();

    [Header("Speed")]
    public float UnitPerSecond = 1f;

    public float Speed = 1f;

    [SerializeField] private InputActionReference speedUInputAction;
    [SerializeField] private InputActionReference speedDInputAction;

    private Pool pool;

    #endregion

    #region ����Ƽ �̺�Ʈ �޼���

    protected override void Awake()
    {
        pool = GetComponent<Pool>();
    }

    private void Start()
    {
        Processor.Instance.NoteSpawnFunc = Spawn;
    }

    private void Update()
    {
        if (speedUInputAction.action.WasPressedThisFrame())
        {
            Speed = Mathf.Clamp(Speed + 0.1f, 1f, 10f);
        }

        if (speedDInputAction.action.WasPressedThisFrame())
        {
            Speed = Mathf.Clamp(Speed - 0.1f, 1f, 10f);
        }

        AutoDespawn();
    }

    #endregion

    #region ��Ʈ ó�� �޼���

    public Note Spawn(int index, double time, double length)
    {
        GameObject GO = pool.Spawn();

        // ������ ��Ʈ�� ���� ������Ʈ���� ������Ʈ�� �����ɴϴ�.
        Note note = GO.GetComponent<Note>();

        // ��Ʈ�� �ε����� �ð��� �����մϴ�.
        note.Init(index, time, length);

        spawned.Add(note);

        return note;
    }

    private void AutoDespawn()
    {
        Action remove = null;

        foreach (var note in spawned)
        {
            if (note.Time + note.Length + DespawnTime <= CurrentPlayTime)
            {
                note.gameObject.Despawn();

                remove += () => spawned.Remove(note);
            }
        }

        if (remove != null) remove.Invoke();
    }

    public Vector3 GetPosition(int index, double time)
    {
        return new Vector3((index - ((int)InputType - 1) * 0.5f) * Space, (float)((time - CurrentPlayTime) * UnitPerSecond * Speed), 0f) + Offset;
    }

    /// <summary>
    /// �Է� Ÿ���� �����մϴ�.
    /// </summary>
    /// <param name="inputType">������ �Է� Ÿ���Դϴ�.</param>
    public void SetInputType(InputBindingType inputType)
    {
        InputType = inputType;
    }

    /// <summary>
    /// ���� ��� �ð� ��ġ�� �����մϴ�.
    /// </summary>
    /// <param name="currentPlayTime">������ ���� ��� �ð� ��ġ�Դϴ�.</param>
    public void SetCurrentPlayTime(double currentPlayTime)
    {
        CurrentPlayTime = currentPlayTime;
    }

    #endregion
}