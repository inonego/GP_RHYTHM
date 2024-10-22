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
    #region �ʵ� ����

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

    #region ��� �� ����

    public void Play()
    {
        Stop();

        // ����� ���� ó��
        FMOD.Sound music = AudioManager.Instance.LoadSound(Chart.Music, mode: FMOD.MODE.ACCURATETIME);
        AudioManager.Instance.LoadMusic(music);
        AudioManager.Instance.PlayMusic();

        // �÷��� ����
        IsPlaying = true;
    }

    public void Stop()
    {
        // ����� ���� ó��
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.ReleaseMusic();

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
        double noteTime      = Chart.ConvertBeatToTime(noteEvent.Beat);
        double noteEndTime   = Chart.ConvertBeatToTime(noteEvent.Beat + noteEvent.Duration);

        double duration = noteEndTime - noteTime;

        // ��Ʈ�� ���� / �� ��ġ�Դϴ�.
        float notePosition       = Chart.GetPosition(noteEvent.Beat);
        float noteEndPosition    = Chart.GetPosition(noteEvent.Beat + noteEvent.Duration);

        float length = noteEndPosition - notePosition;

        // ��Ʈ�� �����մϴ�.
        return NoteSpawnFunc(currentIndex, noteTime, noteEvent.isLong ? duration : 0.0, notePosition, noteEvent.isLong ? length : 0f);
    }

    public void Process(double ProcessBeatDuration)
    {
        if (IsPlaying)
        { 
            // ���� ��� �ð��� ��Ʈ�� ��ȯ�մϴ�.
            CurrentPlayBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);
            // ���� ��� ��Ʈ�� ��ġ�� ��ȯ�մϴ�.
            CurrentPosition = Chart.GetPosition(CurrentPlayBeat);

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