using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chart", menuName = "리듬 게임/채보")]
public class Chart : ScriptableObject
{
    public new string name;

    public string author, genre, description;

    public AudioClip music;

    public double initialBPM = 120.0;

    public InputType inputType { get; private set; }

    public Chart(InputType inputType)
    {
        this.inputType = inputType;

        SetNoteSequenceListCount((int)inputType);
    }

    [SerializeField, HideInInspector]
    private List<Sequence> _noteSequenceList = new List<Sequence>();
    public IReadOnlyList<Sequence> noteSequenceList => _noteSequenceList;

    [HideInInspector] public Sequence bpmChangeSequence = new Sequence();
    [HideInInspector] public Sequence autoPlaySequence = new Sequence();

    private void SetNoteSequenceListCount(int count)
    {
        _noteSequenceList.Clear();

        for (int i = 0; i < count; i++)
        {
            _noteSequenceList.Add(new Sequence());
        }
    }

    public Indicator GetIndicator()
    {
        return new Indicator(this);
    }

    public class Indicator
    {
        public readonly Chart chart;

        [SerializeField, HideInInspector]
        private List<Sequence.Indicator> _iNoteList = new List<Sequence.Indicator> ();
        public IReadOnlyList<Sequence.Indicator> iNoteList => _iNoteList;

        public Sequence.Indicator iBPMChange { get; private set; }
        public Sequence.Indicator iAutoPlay { get; private set; }

        public Indicator(Chart chart)
        {
            this.chart = chart;

            foreach (var sequence in chart._noteSequenceList)
            {
                // 각 시퀀스 별로 인덱스를 설정합니다.
                _iNoteList.Add(new Sequence.Indicator(sequence));
            }

            iBPMChange = new Sequence.Indicator(chart.bpmChangeSequence);
            iAutoPlay = new Sequence.Indicator(chart.autoPlaySequence);
        }

        /// <summary>
        /// 지정된 박자보다 뒤에 있는 가장 첫번째 이벤트로 인덱스를 이동합니다. 
        /// </summary>
        /// <param name="beat">기준이 되는 박자입니다.</param>
        public void MoveTo(double beat)
        {
            foreach (var indicator in _iNoteList)
            {
                indicator.MoveTo(beat);
            }

            iBPMChange.MoveTo(beat);
            iAutoPlay.MoveTo(beat);
        }

        /// <summary>
        /// 시간을 박자로 변환합니다.
        /// </summary>
        /// <param name="time">변환할 시간입니다.</param>
        /// <returns>변환된 박자입니다.</returns>
        public double ConvertTimeToBeat(double time)
        {
            double elapsedTime = 0.0;
            double previousBeat = 0.0;
            double currentBPM = chart.initialBPM;

            foreach (BPMChangeEvent e in chart.bpmChangeSequence.events)
            {
                double nextTime = elapsedTime + (e.beat - previousBeat) * (60.0 / currentBPM);

                if (nextTime >= time) break;

                elapsedTime = nextTime;
                previousBeat = e.beat;
                currentBPM = e.bpm;
            }

            return previousBeat + (time - elapsedTime) * (currentBPM / 60.0f);
        }

        /// <summary>
        /// 박자를 시간으로 변환합니다.
        /// </summary>
        /// <param name="beat">변환할 박자입니다.</param>
        /// <returns>변환된 시간입니다.</returns>
        public double ConvertBeatToTime(double beat)
        {
            double elapsedTime = 0.0;
            double previousBeat = 0.0;
            double currentBPM = chart.initialBPM;

            foreach (BPMChangeEvent e in chart.bpmChangeSequence.events)
            {
                double nextTime = elapsedTime + (e.beat - previousBeat) * (60.0 / currentBPM);

                if (e.beat >= beat) break;

                elapsedTime = nextTime;
                previousBeat = e.beat;
                currentBPM = e.bpm;
            }

            return elapsedTime + (beat - previousBeat) * (60.0 / currentBPM);
        }
    }
}