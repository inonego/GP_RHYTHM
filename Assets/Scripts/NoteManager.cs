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
    #region 필드 변수

    public InputBindingType InputType { get; private set; }

    public Vector3 Offset;
    public float Space = 1f;

    public double DespawnTime = 1f;

    public double CurrentPlayTime { get; private set; }
    public float  CurrentPosition { get; private set; }

    private List<Note> spawned = new List<Note>();

    [Header("Speed")]
    public float UnitPerSecond = 1f;
    public float UserSpeed = 1f;

    [SerializeField] private InputActionReference speedUInputAction;
    [SerializeField] private InputActionReference speedDInputAction;

    private Pool pool;

    #endregion

    #region 유니티 이벤트 메서드

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
            UserSpeed = Mathf.Clamp(UserSpeed + 0.1f, 1f, 10f);
        }

        if (speedDInputAction.action.WasPressedThisFrame())
        {
            UserSpeed = Mathf.Clamp(UserSpeed - 0.1f, 1f, 10f);
        }

        AutoDespawn();
    }

    #endregion

    #region 노트 처리 메서드

    public Note Spawn(int index, double time, double length, float position, float size)
    {
        GameObject GO = pool.Spawn();

        // 스폰된 노트의 게임 오브젝트에서 컴포넌트를 가져옵니다.
        Note note = GO.GetComponent<Note>();

        // 노트의 인덱스와 시간을 설정합니다.
        note.Init(index, time, length, position, size);

        spawned.Add(note);

        return note;
    }

    public void DespawnAll()
    {
        pool.DespawnAll();
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

    public void SetTransform(Note note)
    {
        note.transform.position = new Vector3((note.Index - ((int)InputType - 1) * 0.5f) * Space, (note.Position - CurrentPosition) * UnitPerSecond * UserSpeed, 0f) + Offset;

        note.longGO.transform.localPosition = new Vector3(0f, (float)note.Size * UnitPerSecond * UserSpeed * 0.5f, 0f);
        note.longGO.transform.localScale    = new Vector3(1f, (float)note.Size * UnitPerSecond * UserSpeed, 1f);
    }

    /// <summary>
    /// 입력 타입을 설정합니다.
    /// </summary>
    /// <param name="inputType">설정할 입력 타입입니다.</param>
    public void SetInputType(InputBindingType inputType)
    {
        InputType = inputType;
    }

    /// <summary>
    /// 현재 재생 시간을 설정합니다.
    /// </summary>
    /// <param name="currentPlayTime">설정할 현재 재생 시간입니다.</param>
    public void SetCurrentPlayTime(double currentPlayTime)
    {
        CurrentPlayTime = currentPlayTime;
    }

    /// <summary>
    /// 현재 위치를 설정합니다.
    /// </summary>
    /// <param name="currentPosition">설정할 현재 위치입니다.</param>
    public void SetCurrentPosition(float currentPosition)
    {
        CurrentPosition = currentPosition;
    }

    #endregion
}