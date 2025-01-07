using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public delegate void ProcessEvent();

public delegate void InputProcessEvent(int index, InputDATA inputDATA);
public delegate void NoteProcessEvent(int index, NoteDATA noteDATA);
public delegate void NoteJudgeEvent(int index, NoteJudgeDATA noteJudgeDATA);
public delegate Note SpawnFunc(int index, double time, double length, float position, float size);

[Serializable]
public class CurrentProcess
{
    #region 필드 변수

    public readonly Chart Chart;

    public bool IsPlaying               { get; private set; } = false;

    public float Rate { get; private set; } = 0f;

    public int CurrentCombo             { get; private set; } = 0;
    public float CurrentProcessScore    { get; private set; } = 0f;

    public float  CurrentPosition { get; private set; }
    public double CurrentPlayBeat { get; private set; }
    public double CurrentPlayTime => AudioManager.Instance.GetMusicCurrentPlayTime();

    private float scoreSum = 0f;
    private int processedCount = 0;

    private double lastestProcessTime = 0.0;

    private int currentIndex = 0;

    private List<ProcessLine> lineList = new List<ProcessLine>();

    private FMOD.Sound music;

    #endregion

    #region 이벤트 및 델리게이트

    public event ProcessEvent OnProcessEnded;

    public event InputProcessEvent OnInputProcess;
    public event NoteProcessEvent OnNoteProcess;
    public event NoteJudgeEvent OnNoteJudge;
    public SpawnFunc NoteSpawnFunc;

    #endregion

    #region 초기화 메서드

    public CurrentProcess(Chart chart)
    {
        Chart = chart;

        // 라인 관련 처리
        InputBinding inputBinding = InputManager.Instance.InputBindingList[Chart.InputType];

        for (int i = 0; i < inputBinding.InputActionList.Count; i++)
        {
            ProcessLine processLine = new ProcessLine(Chart.SQListNote[i]);

            processLine.OnInputProcess  += _OnInputProcess;
            processLine.OnNoteProcess   += _OnNoteProcess;
            processLine.OnNoteJudge     += _OnNoteJudge;
            processLine.NoteSpawnFunc   += _NoteSpawnFunc;
            
            processLine.Bind(inputBinding.InputActionList[i].action);

            lineList.Add(processLine);
        }

        // 오디오 관련 처리
        music = AudioManager.Instance.LoadSound(Chart.Music, mode: FMOD.MODE.ACCURATETIME);

        AudioManager.Instance.OnMusicEnded += OnMusicEnded;

        // 플래그 설정
        IsPlaying  = false;
    }

    public void Release()
    {
        Chart.ClearCache();

        // 라인 관련 처리
        foreach (var line in lineList)
        {
            line.Release();
        }

        lineList.Clear();

        // 오디오 관련 처리
        music.release();

        AudioManager.Instance.OnMusicEnded -= OnMusicEnded;

        // 플래그 설정
        IsPlaying = false;
    }

    #endregion

    #region 재생 및 중지

    public void Play()
    {
        Stop();

        Chart.MakeCache();

        // 오디오 관련 처리
        AudioManager.Instance.LoadMusic(music);
        AudioManager.Instance.PlayMusic();

        // 플래그 설정
        IsPlaying = true;
    }

    public void Stop()
    {
        // 오디오 관련 처리
        AudioManager.Instance.StopMusic();

        // 플래그 설정
        IsPlaying = false;
    }

    #endregion

    #region 채보 처리 메서드

    /// <summary>
    /// 입력이 처리될때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    private void _OnInputProcess(InputDATA inputDATA)
    {
        OnInputProcess(currentIndex, inputDATA);
    }

    /// <summary>
    /// 노트가 처리될때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    private void _OnNoteProcess(NoteDATA noteDATA)
    {
        OnNoteProcess(currentIndex, noteDATA);
    }

    /// <summary>
    /// 노트가 판정될때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteJudgeDATA"></param>
    private void _OnNoteJudge(NoteJudgeDATA noteJudgeDATA)
    {
        scoreSum += noteJudgeDATA.Score;

        processedCount += 1;

        // 현재 레이트 계산
        Rate = scoreSum / processedCount;

        // 현재 콤보 계산
        if (noteJudgeDATA.Score != 0f)
        {
            CurrentCombo++;
        }
        else
        {
            CurrentCombo = 0;
        }

        // 현재 노트 점수 계산
        if (lastestProcessTime < noteJudgeDATA.Time)
        {
            lastestProcessTime = noteJudgeDATA.Time;

            CurrentProcessScore = noteJudgeDATA.Score;
        }

        OnNoteJudge(currentIndex, noteJudgeDATA);
    }

    /// <summary>
    /// 노트를 스폰할때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="noteEvent"></param>
    private Note _NoteSpawnFunc(NoteEvent noteEvent)
    {
        // 노트의 시작 / 끝 시간입니다.
        double noteTime      = noteEvent.CachedTime;
        double noteEndTime   = noteEvent.CachedTime + noteEvent.CachedLength;

        double length = noteEndTime - noteTime;

        // 노트의 시작 / 끝 위치입니다.
        float notePosition       = Chart.GetPosition(noteTime);
        float noteEndPosition    = Chart.GetPosition(noteEndTime);

        float size = noteEndPosition - notePosition;

        // 노트를 스폰합니다.
        return NoteSpawnFunc(currentIndex, noteTime, noteEvent.isLong ? length : 0.0, notePosition, noteEvent.isLong ? size : 0f);
    }

    private void OnMusicEnded()
    {
        OnProcessEnded();
    }

    public void Process(double ProcessBeatDuration)
    {
        if (IsPlaying)
        { 
            // 현재 재생 시간을 비트로 변환합니다.
            CurrentPlayBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);
            // 현재 재생 비트를 위치로 변환합니다.
            CurrentPosition = Chart.GetPosition(CurrentPlayTime);

            for (int index = 0; index < (int)Chart.InputType; index++)
            {
                // 현재 인덱스를 설정합니다.
                currentIndex = index;

                ProcessLine processLine = lineList[index];

                processLine.SetCurrentPlayTime(CurrentPlayTime);

                processLine.Spawn(CurrentPlayBeat + ProcessBeatDuration);
                processLine.Judge();
            }
        }
    }

    #endregion
}