using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Chart : ScriptableObject
{
    #region 정적 메서드

    /// <summary>
    /// 채보를 생성합니다.
    /// </summary>
    /// <param name="inputType">생성할 채보의 입력 타입입니다.</param>
    public static Chart CreateChart(InputBindingType inputType)
    {
        var chart = CreateInstance<Chart>();

        chart.Init(inputType);

        AssetDatabase.CreateAsset(chart, "Assets/채보.asset");

        return chart;
    }

    /// <summary>
    /// 4키 채보를 생성합니다.
    /// </summary>
    [MenuItem("리듬게임/채보(4키) 생성")]
    static void CreateChartKEY4()
    {
        CreateChart(InputBindingType.KEY4);
    }

    #endregion

    #region 필드 변수

    /// <summary>
    /// 채보 노래의 이름입니다.
    /// </summary>
    public string Name;

    /// <summary>
    /// 채보 노래의 제작자입니다.
    /// </summary>
    public string Author;

    /// <summary>
    /// 채보 노래의 장르입니다.
    /// </summary>
    public string Genre;

    /// <summary>
    /// 채보 노래에 대한 설명입니다.
    /// </summary>
    public string Description;

    /// <summary>
    /// 채보 노래입니다.
    /// </summary>
    public AudioClip Music;

    /// <summary>
    /// 채보 노래 재생 시 노래가 시작되는 시간입니다.
    /// </summary>
    public double StartTime = 0.0;

    /// <summary>
    /// 채보 노래의 시작 BPM입니다.
    /// </summary>
    public double InitialBPM = 120.0;

    /// <summary>
    /// 채보의 입력 타입입니다.
    /// </summary>
    [field: SerializeField, HideInInspector]
    public InputBindingType InputType { get; private set; }

    [SerializeField, HideInInspector] private List<Sequence<NoteEvent>> _SQListNote = new List<Sequence<NoteEvent>>();
    [SerializeField, HideInInspector] private Sequence<BPMChangeEvent> _SQBPMChange = new Sequence<BPMChangeEvent>();
    [SerializeField, HideInInspector] private Sequence<SpeedChangeEvent> _SQSpeedChange = new Sequence<SpeedChangeEvent>();

    /// <summary>
    /// 노트 이벤트에 대한 시퀀스 목록입니다.
    /// </summary>
    public IReadOnlyList<Sequence<NoteEvent>> SQListNote => _SQListNote;
    /// <summary>
    /// BPM 변경 이벤트에 대한 시퀀스입니다.
    /// </summary>
    public Sequence<BPMChangeEvent> SQBPMChange => _SQBPMChange;
    /// <summary>
    /// 속도 변경 이벤트에 대한 시퀀스입니다.
    /// </summary>
    public Sequence<SpeedChangeEvent> SQSpeedChange => _SQSpeedChange;

    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 생성자는 사용되지 않습니다.
    /// CreateChart를 이용해주세요.
    /// </summary>
    private Chart() { }

    /// <summary>
    /// 채보를 초기화합니다.
    /// </summary>
    /// <param name="inputType">채보의 입력 타입입니다.</param>
    private void Init(InputBindingType inputType)
    {
        InputType = inputType;

        SetSQListNoteCount((int)inputType);
    }

    /// <summary>
    /// 노트 이벤트 시퀀스 목록의 개수를 정합니다.
    /// </summary>
    /// <param name="count">설정할 개수입니다.</param>
    private void SetSQListNoteCount(int count)
    {
        _SQListNote.Clear();

        for (int i = 0; i < count; i++)
        {
            _SQListNote.Add(new Sequence<NoteEvent>());
        }
    }

    #endregion

    #region 유틸 메서드

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
    /// 시간을 비트로 변환합니다.
    /// </summary>
    /// <param name="time">변환할 시간입니다.</param>
    /// <returns>변환된 비트입니다.</returns>
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
    /// 비트를 시간으로 변환합니다.
    /// </summary>
    /// <param name="beat">변환할 비트입니다.</param>
    /// <returns>변환된 시간입니다.</returns>
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