using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ProcessEvent();

public delegate void KeyEvent(int index, InputDATA inputDATA);
public delegate void NoteProcessEvent(int index, NoteProcessDATA noteProcessDATA);
public delegate void LongProcessEvent(int index, LongProcessDATA longProcessDATA);
public delegate Note NoteSpawnDelegate(int index, double time, double length, float position, float size);

[Serializable]
public class CurrentProcess
{
    #region �ʵ� ����

    public readonly Chart Chart;

    public float Rate { get; private set; } = 0f;

    public int CurrentCombo             { get; private set; }
    public float CurrentProcessScore    { get; private set; }

    public bool IsPlaying               { get; private set; }

    public float  CurrentPosition { get; private set; }
    public double CurrentPlayBeat { get; private set; }
    public double CurrentPlayTime => AudioManager.Instance.GetMusicCurrentPlayTime();

    private float scoreSum = 0f;
    private int processedCount = 0;

    private double lastestProcessTime = 0.0;

    private int currentIndex = 0;

    private List<ProcessLine> lineList = new List<ProcessLine>();

    public ProcessEvent OnProcessEnded;

    public KeyEvent OnKeyEvent;
    public NoteProcessEvent OnNoteProcess;
    public LongProcessEvent OnLongProcess;
    public NoteSpawnDelegate NoteSpawnFunc;

    private FMOD.Sound music;

    private bool isReleased;

    #endregion

    #region �ʱ�ȭ �޼���

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

        // ����� ���� ó��
        music = AudioManager.Instance.LoadSound(Chart.Music, mode: FMOD.MODE.ACCURATETIME);

        // �÷��� ����
        IsPlaying  = false;
        isReleased = false;
    }

    public void Release()
    {
        Chart.ClearCache();

        foreach (var line in lineList)
        {
            line.Release();
        }

        lineList.Clear();

        // ����� ���� ó��
        music.release();

        AudioManager.Instance.OnMusicEnded -= OnMusicEnded;

        // �÷��� ����
        IsPlaying = false;
        isReleased = true;
    }

    #endregion

    #region ��� �� ����

    public void Play()
    {
        Stop();

        Chart.MakeCache();

        // ����� ���� ó��
        AudioManager.Instance.LoadMusic(music);
        AudioManager.Instance.PlayMusic();

        AudioManager.Instance.OnMusicEnded += OnMusicEnded;

        // �÷��� ����
        IsPlaying = true;
    }

    public void Stop()
    {
        // ����� ���� ó��
        AudioManager.Instance.StopMusic();

        // �÷��� ����
        IsPlaying = false;
    }

    #endregion

    #region ä�� ó�� �޼���

    private void _OnKeyEvent(InputDATA inputDATA)
    {
        OnKeyEvent(currentIndex, inputDATA);
    }

    private void _OnNoteProcess(NoteProcessDATA noteProcessDATA)
    {
        scoreSum += noteProcessDATA.Score;

        processedCount += 1;

        // ���� ����Ʈ ���
        Rate = scoreSum / processedCount;

        // ���� �޺� ���
        if (noteProcessDATA.Score != 0f)
        {
            CurrentCombo++;
        }
        else
        {
            CurrentCombo = 0;
        }

        // ���� ��Ʈ ���� ���
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
        // ��Ʈ�� ���� / �� �ð��Դϴ�.
        double noteTime      = noteEvent.CachedTime;
        double noteEndTime   = noteEvent.CachedTime + noteEvent.CachedLength;

        double length = noteEndTime - noteTime;

        // ��Ʈ�� ���� / �� ��ġ�Դϴ�.
        float notePosition       = Chart.GetPosition(noteTime);
        float noteEndPosition    = Chart.GetPosition(noteEndTime);

        float size = noteEndPosition - notePosition;

        // ��Ʈ�� �����մϴ�.
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
            // ���� ��� �ð��� ��Ʈ�� ��ȯ�մϴ�.
            CurrentPlayBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);
            // ���� ��� ��Ʈ�� ��ġ�� ��ȯ�մϴ�.
            CurrentPosition = Chart.GetPosition(CurrentPlayTime);

            for (int index = 0; index < (int)Chart.InputType; index++)
            {
                // ���� �ε����� �����մϴ�.
                currentIndex = index;

                lineList[index].ProcessSpawn(CurrentPlayBeat + ProcessBeatDuration);
                lineList[index].ProcessJudge(CurrentPlayTime);
            }
        }
    }

    #endregion
}