using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Chart : ScriptableObject
{
    #region ���� �޼���

    /// <summary>
    /// ä���� �����մϴ�.
    /// </summary>
    /// <param name="inputType">������ ä���� �Է� Ÿ���Դϴ�.</param>
    public static Chart CreateChart(InputBindingType inputType)
    {
        var chart = CreateInstance<Chart>();

        chart.Init(inputType);

        AssetDatabase.CreateAsset(chart, "Assets/ä��.asset");

        return chart;
    }

    /// <summary>
    /// 4Ű ä���� �����մϴ�.
    /// </summary>
    [MenuItem("�������/ä��(4Ű) ����")]
    static void CreateChartKEY4()
    {
        CreateChart(InputBindingType.KEY4);
    }

    #endregion

    #region �ʵ� ����

    /// <summary>
    /// ä�� �뷡�� �̸��Դϴ�.
    /// </summary>
    public string Name;

    /// <summary>
    /// ä�� �뷡�� �������Դϴ�.
    /// </summary>
    public string Author;

    /// <summary>
    /// ä�� �뷡�� �帣�Դϴ�.
    /// </summary>
    public string Genre;

    /// <summary>
    /// ä�� �뷡�� ���� �����Դϴ�.
    /// </summary>
    public string Description;

    /// <summary>
    /// ä�� �뷡�Դϴ�.
    /// </summary>
    public AudioClip Music;

    /// <summary>
    /// ä�� �뷡 ��� �� �뷡�� ���۵Ǵ� �ð��Դϴ�.
    /// </summary>
    public double StartTime = 0.0;

    /// <summary>
    /// ä�� �뷡�� ���� BPM�Դϴ�.
    /// </summary>
    public double InitialBPM = 120.0;

    /// <summary>
    /// ä���� �Է� Ÿ���Դϴ�.
    /// </summary>
    [field: SerializeField, HideInInspector]
    public InputBindingType InputType { get; private set; }

    [SerializeField, HideInInspector] private List<Sequence<NoteEvent>> _SQListNote = new List<Sequence<NoteEvent>>();
    [SerializeField, HideInInspector] private Sequence<BPMChangeEvent> _SQBPMChange = new Sequence<BPMChangeEvent>();
    [SerializeField, HideInInspector] private Sequence<SpeedChangeEvent> _SQSpeedChange = new Sequence<SpeedChangeEvent>();

    /// <summary>
    /// ��Ʈ �̺�Ʈ�� ���� ������ ����Դϴ�.
    /// </summary>
    public IReadOnlyList<Sequence<NoteEvent>> SQListNote => _SQListNote;
    /// <summary>
    /// BPM ���� �̺�Ʈ�� ���� �������Դϴ�.
    /// </summary>
    public Sequence<BPMChangeEvent> SQBPMChange => _SQBPMChange;
    /// <summary>
    /// �ӵ� ���� �̺�Ʈ�� ���� �������Դϴ�.
    /// </summary>
    public Sequence<SpeedChangeEvent> SQSpeedChange => _SQSpeedChange;

    #endregion

    #region �ʱ�ȭ �޼���

    /// <summary>
    /// �����ڴ� ������ �ʽ��ϴ�.
    /// CreateChart�� �̿����ּ���.
    /// </summary>
    private Chart() { }

    /// <summary>
    /// ä���� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="inputType">ä���� �Է� Ÿ���Դϴ�.</param>
    private void Init(InputBindingType inputType)
    {
        InputType = inputType;

        SetSQListNoteCount((int)inputType);
    }

    /// <summary>
    /// ��Ʈ �̺�Ʈ ������ ����� ������ ���մϴ�.
    /// </summary>
    /// <param name="count">������ �����Դϴ�.</param>
    private void SetSQListNoteCount(int count)
    {
        _SQListNote.Clear();

        for (int i = 0; i < count; i++)
        {
            _SQListNote.Add(new Sequence<NoteEvent>());
        }
    }

    #endregion

    #region ��ƿ �޼���

    public int GetTotalNoteEventCount()
    {
        int sum = 0;

        for (int i = 0; i < SQListNote.Count; i++)
        {
            sum += SQListNote[i].Events.Count;
        }

        return sum;
    }

    public float GetPosition(double beat)
    {
        double position = beat;
        float previousSpeed = 1f;

        foreach (SpeedChangeEvent e in SQSpeedChange.Events)
        {
            double eventBeat = e.Beat;

            if (eventBeat > beat)
            {
                break;
            }

            position += (e.Speed - previousSpeed) * (beat - eventBeat);
            previousSpeed = e.Speed;
        }
        
        return (float)(ConvertBeatToTime(position));
    }

    /// <summary>
    /// �ð��� ��Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="time">��ȯ�� �ð��Դϴ�.</param>
    /// <returns>��ȯ�� ��Ʈ�Դϴ�.</returns>
    public double ConvertTimeToBeat(double time)
    {
        double elapsedTime = StartTime;
        double previousBeat = 0.0;
        double currentBPM = InitialBPM;

        foreach (BPMChangeEvent e in SQBPMChange.Events)
        {
            double nextTime = elapsedTime + (e.Beat - previousBeat) * (60.0 / currentBPM);

            if (nextTime >= time) break;

            elapsedTime = nextTime;
            previousBeat = e.Beat;
            currentBPM = e.BPM;
        }

        return previousBeat + (time - elapsedTime) * (currentBPM / 60.0f);
    }

    /// <summary>
    /// ��Ʈ�� �ð����� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="beat">��ȯ�� ��Ʈ�Դϴ�.</param>
    /// <returns>��ȯ�� �ð��Դϴ�.</returns>
    public double ConvertBeatToTime(double beat)
    {
        double elapsedTime = StartTime;
        double previousBeat = 0.0;
        double currentBPM = InitialBPM;

        foreach (BPMChangeEvent e in SQBPMChange.Events)
        {
            double nextTime = elapsedTime + (e.Beat - previousBeat) * (60.0 / currentBPM);

            if (e.Beat >= beat) break;

            elapsedTime = nextTime;
            previousBeat = e.Beat;
            currentBPM = e.BPM;
        }

        return elapsedTime + (beat - previousBeat) * (60.0 / currentBPM);
    }

    #endregion
}