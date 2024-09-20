using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chart", menuName = "���� ����/ä��")]
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
                // �� ������ ���� �ε����� �����մϴ�.
                _iNoteList.Add(new Sequence.Indicator(sequence));
            }

            iBPMChange = new Sequence.Indicator(chart.bpmChangeSequence);
            iAutoPlay = new Sequence.Indicator(chart.autoPlaySequence);
        }

        /// <summary>
        /// ������ ���ں��� �ڿ� �ִ� ���� ù��° �̺�Ʈ�� �ε����� �̵��մϴ�. 
        /// </summary>
        /// <param name="beat">������ �Ǵ� �����Դϴ�.</param>
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
        /// �ð��� ���ڷ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="time">��ȯ�� �ð��Դϴ�.</param>
        /// <returns>��ȯ�� �����Դϴ�.</returns>
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
        /// ���ڸ� �ð����� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="beat">��ȯ�� �����Դϴ�.</param>
        /// <returns>��ȯ�� �ð��Դϴ�.</returns>
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