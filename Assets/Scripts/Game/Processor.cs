using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Processor : MonoSingleton<Processor>
{
    #region 필드 변수

    public Chart Chart;

    public double ProcessBeatDuration = 16.0;

    [SerializeField, HideInInspector] private List<ProcessLine> lineList = new List<ProcessLine>();
    public IReadOnlyList<ProcessLine> LineList => lineList;

    public double CurrentPlayTime => AudioManager.Instance.GetCurrentPlayTime(0);

    public bool isPlaying { get; private set; } = false;

    private int currentIndex = 0;

    #endregion

    #region 초기화 메서드

    public void CreateLineList(InputBindingType inputType)
    {
        ClearLineList();

        InputBinding inputBinding = InputManager.Instance.InputBindingList[inputType];

        for (int i = 0; i < inputBinding.InputActionList.Count; i++)
        {
            ProcessLine processLine = new ProcessLine(Chart.SQListNote[i]);

            processLine.OnNotePassed += OnNotePassed;
            processLine.NoteSpawnFunc += Spawn;

            processLine.Bind(inputBinding.InputActionList[i].action);

            lineList.Add(processLine);
        }
    }

    public void ClearLineList()
    {
        foreach (var line in lineList)
        {
            line.Release();
        }

        lineList.Clear();
    }

    #endregion

    #region 유니티 이벤트 메서드

    private void Start()
    {
        Chart.SQListNote[0].Events.Clear();
        Chart.SQListNote[1].Events.Clear();
        Chart.SQListNote[2].Events.Clear();
        Chart.SQListNote[3].Events.Clear();

        for (int i = 0; i < 1024; i++)
        {
            NoteEvent noteEvent = new NoteEvent();

            noteEvent.Beat = i;

            noteEvent.Duration = 0f;

            Chart.SQListNote[0].Events.Add(noteEvent);
            Chart.SQListNote[1].Events.Add(noteEvent);
            Chart.SQListNote[2].Events.Add(noteEvent);
            Chart.SQListNote[3].Events.Add(noteEvent);
        }

        Play();
    }

    private void Update()
    {
        if (isPlaying)
        {
            Process();
        }
    }

    #endregion

    #region 재생 및 중지

    public void Play()
    {
        Stop();

        if (Chart != null)
        {
            NoteManager.Instance.SetInputType(Chart.InputType);

            // 게임 관련 처리
            CreateLineList(Chart.InputType);

            // 오디오 관련 처리
            AudioManager.Instance.Load(SoundType.Music, Chart.Music, mode: FMOD.MODE.ACCURATETIME);
            AudioManager.Instance.Play(channel: 0, SoundType.Music);

            // 플래그 설정
            isPlaying = true;
        }
    }

    public void Stop()
    {
        // 게임 관련 처리
        ClearLineList();

        // 오디오 관련 처리
        AudioManager.Instance.Stop(channel: 0);
        AudioManager.Instance.Release(SoundType.Music);

        // 플래그 설정
        isPlaying = false;
    }

    #endregion

    #region 채보 처리 메서드

    public int CurrentCombo { get; private set; }
    public float CurrentProcessScore { get; private set; }

    private double lastestProcessTime = 0.0;

    private void OnNotePassed(Note note, double time, float score)
    {
        if (score != 0f)
        {
            CurrentCombo++;
        }
        else
        {
            CurrentCombo = 0;
        }

        if (lastestProcessTime < time)
        {
            lastestProcessTime = time;

            CurrentProcessScore = Mathf.CeilToInt(score / 10f) * 10f;
        }
    }

    private Note Spawn(NoteEvent noteEvent)
    {
        // 노트의 시작 / 끝 시간입니다.
        double noteTime = Chart.ConvertBeatToTime(noteEvent.Beat);
        double noteEndTime = Chart.ConvertBeatToTime(noteEvent.Beat + noteEvent.Duration);

        double length = noteEndTime - noteTime;

        // 노트를 스폰합니다.
        return NoteManager.Instance.Spawn(currentIndex, noteTime, noteEvent.isLong ? length : 0);
    }

    private void Process()
    {
        NoteManager.Instance.SetCurrentPlayTime(CurrentPlayTime);

        // 현재 재생 시간을 비트로 변환합니다.
        double currentBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);

        for (int index = 0; index < (int)Chart.InputType; index++)
        {
            currentIndex = index;

            lineList[index].Process(currentBeat + ProcessBeatDuration, CurrentPlayTime);
        }
    }

    #endregion
}
