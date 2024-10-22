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
    #region �ʵ� ����

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

        // �ӽ÷� ���� ��Ʈ ����
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

    #region ��� �� ����

    public void Play(float playDelayTime)
    {
        Stop();

        // ����� ���� ó��
        AudioManager.Instance.Load(SoundType.Music, Chart.Music, mode: FMOD.MODE.ACCURATETIME);

        // ��Ʈ ���� ó��
        playDelayCounter.Start(playDelayTime);

        // �÷��� ����
        IsPlaying = true;
    }

    private void Play()
    {
        AudioManager.Instance.Play(channel: 0, SoundType.Music);
    }

    public void Stop()
    {
        // ����� ���� ó��
        AudioManager.Instance.Stop(channel: 0);
        AudioManager.Instance.Release(SoundType.Music);

        playDelayCounter.Stop();

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

        double length = noteEndTime - noteTime;

        // ��Ʈ�� �����մϴ�.
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

            // ���� ��� �ð��� ��Ʈ�� ��ȯ�մϴ�.
            double currentBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);

            for (int index = 0; index < (int)Chart.InputType; index++)
            {
                // ���� �ε����� �����մϴ�.
                currentIndex = index;

                lineList[index].ProcessSpawn(currentBeat + ProcessBeatDuration);
                lineList[index].ProcessJudge(CurrentPlayTime);
            }
        }
    }

    #endregion
}