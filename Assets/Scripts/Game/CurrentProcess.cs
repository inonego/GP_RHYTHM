using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void KeyEvent(int index, InputDATA inputDATA);
public delegate void NoteProcessEvent(int index, NoteProcessDATA noteProcessDATA);
public delegate void LongProcessEvent(int index, LongProcessDATA longProcessDATA);
public delegate Note NoteSpawnDelegate(int index, double time, double length);

[Serializable]
public class CurrentProcess
{
    #region 필드 변수

    public readonly Chart Chart;

    public float Rate { get; private set; } = 0f;

    public int CurrentCombo             { get; private set; }
    public float CurrentProcessScore    { get; private set; }

    public bool IsPlaying               { get; private set; } = false;

    public double CurrentPlayTime => AudioManager.Instance.GetCurrentPlayTime(0) - playDelayCounter.GetTimeLeft();

    private TimeCounter playDelayCounter = new TimeCounter();

    private float scoreSum = 0f;
    private int processedCount = 0;

    private double lastestProcessTime = 0.0;

    private int currentIndex = 0;

    private List<ProcessLine> lineList = new List<ProcessLine>();

    public KeyEvent OnKeyEvent;
    public NoteProcessEvent OnNoteProcess;
    public LongProcessEvent OnLongProcess;
    public NoteSpawnDelegate NoteSpawnFunc;

    #endregion

    #region 초기화 메서드

    public CurrentProcess(Chart chart)
    {
        Chart = chart;

        InputBinding inputBinding = InputManager.Instance.InputBindingList[Chart.InputType];

        for (int i = 0; i < inputBinding.InputActionList.Count; i++)
        {
            ProcessLine processLine = new ProcessLine(Chart.SQListNote[i]);

            processLine.OnKeyEvent      += _OnKeyEvent;
            processLine.OnNoteProcess   += _OnNoteProcess;
            processLine.OnLongProcess   += _OnLongProcess;
            processLine.NoteSpawnFunc    = _NoteSpawnFunc;

            processLine.Bind(inputBinding.InputActionList[i].action);

            lineList.Add(processLine);
        }

        // 임시로 랜덤 노트 생성
        {
            Chart.SQListNote[0].Events.Clear();
            Chart.SQListNote[1].Events.Clear();
            Chart.SQListNote[2].Events.Clear();
            Chart.SQListNote[3].Events.Clear();

            for (int i = 0; i < 1024; i++)
            {
                NoteEvent noteEvent = new NoteEvent();

                noteEvent.Beat = i * 0.5f;

                noteEvent.Duration = UnityEngine.Random.Range(0f, 1f) < 0.8f ? 0f : 0.25f;

                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[0].Events.Add(noteEvent);
                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[1].Events.Add(noteEvent);
                if (UnityEngine.Random.Range(0f, 1f) > 0.7f) Chart.SQListNote[2].Events.Add(noteEvent);
            }

            for (int i = 0; i < 256; i++)
            {
                NoteEvent noteEvent = new NoteEvent();

                noteEvent.Beat = i * 8f;
                noteEvent.Duration = 4f;

                Chart.SQListNote[3].Events.Add(noteEvent);
            }
        }
    }

    public void Release()
    {
        foreach (var line in lineList)
        {
            line.Release();
        }

        lineList.Clear();
    }

    #endregion

    #region 재생 및 중지

    public void Play(float playDelayTime)
    {
        Stop();

        // 오디오 관련 처리
        AudioManager.Instance.Load(SoundType.Music, Chart.Music, mode: FMOD.MODE.ACCURATETIME);

        // 노트 관련 처리
        playDelayCounter.Start(playDelayTime);

        // 플래그 설정
        IsPlaying = true;
    }

    private void Play()
    {
        AudioManager.Instance.Play(channel: 0, SoundType.Music);
    }

    public void Stop()
    {
        // 오디오 관련 처리
        AudioManager.Instance.Stop(channel: 0);
        AudioManager.Instance.Release(SoundType.Music);

        playDelayCounter.Stop();

        // 플래그 설정
        IsPlaying = false;
    }

    #endregion

    #region 채보 처리 메서드

    private void _OnKeyEvent(InputDATA inputDATA)
    {
        OnKeyEvent(currentIndex, inputDATA);
    }

    private void _OnNoteProcess(NoteProcessDATA noteProcessDATA)
    {
        scoreSum += noteProcessDATA.Score;

        processedCount += 1;

        // 현재 레이트 계산
        Rate = scoreSum / processedCount;

        // 현재 콤보 계산
        if (noteProcessDATA.Score != 0f)
        {
            CurrentCombo++;
        }
        else
        {
            CurrentCombo = 0;
        }

        // 현재 노트 점수 계산
        if (lastestProcessTime < noteProcessDATA.Time)
        {
            lastestProcessTime = noteProcessDATA.Time;

            CurrentProcessScore = noteProcessDATA.Score;
        }

        OnNoteProcess(currentIndex, noteProcessDATA);
    }

    private void _OnLongProcess(LongProcessDATA longProcessDATA)
    {
        OnLongProcess(currentIndex, longProcessDATA);
    }

    private Note _NoteSpawnFunc(NoteEvent noteEvent)
    {
        // 노트의 시작 / 끝 시간입니다.
        double noteTime      = Chart.ConvertBeatToTime(noteEvent.Beat);
        double noteEndTime   = Chart.ConvertBeatToTime(noteEvent.Beat + noteEvent.Duration);

        double length = noteEndTime - noteTime;

        // 노트를 스폰합니다.
        return NoteSpawnFunc(currentIndex, noteTime, noteEvent.isLong ? length : 0.0);
    }

    public void Process(double ProcessBeatDuration)
    {
        playDelayCounter.Update();

        if (IsPlaying)
        {
            if (playDelayCounter.WasEndedThisFrame())
            {
                Play();
            }

            // 현재 재생 시간을 비트로 변환합니다.
            double currentBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);

            for (int index = 0; index < (int)Chart.InputType; index++)
            {
                // 현재 인덱스를 설정합니다.
                currentIndex = index;

                lineList[index].ProcessSpawn(currentBeat + ProcessBeatDuration);
                lineList[index].ProcessJudge(CurrentPlayTime);
            }
        }
    }

    #endregion
}