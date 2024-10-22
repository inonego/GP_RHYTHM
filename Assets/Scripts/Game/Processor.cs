using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Processor : MonoSingleton<Processor>
{
    #region �ʵ� ����

    public Chart Chart;

    public double ProcessBeatDuration = 16.0;

    [SerializeField, HideInInspector] private List<ProcessLine> lineList = new List<ProcessLine>();
    public IReadOnlyList<ProcessLine> LineList => lineList;

    public double CurrentPlayTime => AudioManager.Instance.GetCurrentPlayTime(0);

    public bool isPlaying { get; private set; } = false;

    private int currentIndex = 0;

    #endregion

    #region �ʱ�ȭ �޼���

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

    #region ����Ƽ �̺�Ʈ �޼���

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

    #region ��� �� ����

    public void Play()
    {
        Stop();

        if (Chart != null)
        {
            NoteManager.Instance.SetInputType(Chart.InputType);

            // ���� ���� ó��
            CreateLineList(Chart.InputType);

            // ����� ���� ó��
            AudioManager.Instance.Load(SoundType.Music, Chart.Music, mode: FMOD.MODE.ACCURATETIME);
            AudioManager.Instance.Play(channel: 0, SoundType.Music);

            // �÷��� ����
            isPlaying = true;
        }
    }

    public void Stop()
    {
        // ���� ���� ó��
        ClearLineList();

        // ����� ���� ó��
        AudioManager.Instance.Stop(channel: 0);
        AudioManager.Instance.Release(SoundType.Music);

        // �÷��� ����
        isPlaying = false;
    }

    #endregion

    #region ä�� ó�� �޼���

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
        // ��Ʈ�� ���� / �� �ð��Դϴ�.
        double noteTime = Chart.ConvertBeatToTime(noteEvent.Beat);
        double noteEndTime = Chart.ConvertBeatToTime(noteEvent.Beat + noteEvent.Duration);

        double length = noteEndTime - noteTime;

        // ��Ʈ�� �����մϴ�.
        return NoteManager.Instance.Spawn(currentIndex, noteTime, noteEvent.isLong ? length : 0);
    }

    private void Process()
    {
        NoteManager.Instance.SetCurrentPlayTime(CurrentPlayTime);

        // ���� ��� �ð��� ��Ʈ�� ��ȯ�մϴ�.
        double currentBeat = Chart.ConvertTimeToBeat(CurrentPlayTime);

        for (int index = 0; index < (int)Chart.InputType; index++)
        {
            currentIndex = index;

            lineList[index].Process(currentBeat + ProcessBeatDuration, CurrentPlayTime);
        }
    }

    #endregion
}
