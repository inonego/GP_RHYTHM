using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void KeyEvent(int index, InputDATA inputDATA);
public delegate void NoteProcessEvent(int index, NoteProcessDATA noteProcessDATA);
public delegate void LongProcessEvent(int index, LongProcessDATA longProcessDATA);
public delegate Note NoteSpawnDelegate(int index, double time, double duration, float position, float length);

[Serializable]
public class CurrentProcess
{
    #region 필드 변수

    public readonly Chart Chart;

    public float Rate { get; private set; } = 0f;

    public int CurrentCombo             { get; private set; }
    public float CurrentProcessScore    { get; private set; }

    public bool IsPlaying               { get; private set; } = false;

    public float  CurrentPosition { get; private set; }
    public double CurrentPlayBeat { get; private set; }
    public double CurrentPlayTime => AudioManager.Instance.GetMusicCurrentPlayTime();

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

    public void Play()
    {
        Stop();

        // 오디오 관련 처리
        FMOD.Sound music = AudioManager.Instance.LoadSound(Chart.Music, mode: FMOD.MODE.ACCURATETIME);
        AudioManager.Instance.LoadMusic(music);
        AudioManager.Instance.PlayMusic();

        // 플래그 설정
        IsPlaying = true;
    }

    public void Stop()
    {
        // 오디오 관련 처리
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.ReleaseMusic();

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

        double duration = noteEndTime - noteTime;

        // 노트의 시작 / 끝 위치입니다.
        float notePosition       = Chart.GetPosition(noteEvent.Beat);
        float noteEndPosition    = Chart.GetPosition(noteEvent.Beat + noteEvent.Duration);

        float length = noteEndPosition - notePosition;

        // 노트를 스폰합니다.
        return NoteSpawnFunc(currentIndex, noteTime, noteEvent.isLong ? duration : 0.0, notePosition, noteEvent.isLong ? length : 0f);
    }

    public void Process(double ProcessBeatDuration)
    {
        if (IsPlaying)
        { 
            // 현재 재생 시간을 비트로 변환합니다.
            CurrentPlayBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);
            // 현재 재생 비트를 위치로 변환합니다.
            CurrentPosition = Chart.GetPosition(CurrentPlayBeat);

            for (int index = 0; index < (int)Chart.InputType; index++)
            {
                // 현재 인덱스를 설정합니다.
                currentIndex = index;

                lineList[index].ProcessSpawn(CurrentPlayBeat + ProcessBeatDuration);
                lineList[index].ProcessJudge(CurrentPlayTime);
            }
        }
    }

    #endregion
}