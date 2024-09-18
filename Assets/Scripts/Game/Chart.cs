using System.Collections.Generic;
using UnityEngine;

public class Chart : ScriptableObject
{
    public new string name;
    
    public string author, genre, description;

    public AudioClip music;

    public double initialBPM = 120.0;

    [HideInInspector] public List<Sequence> noteSequences = new List<Sequence>();
    [HideInInspector] public Sequence bpmChangeSequence = new Sequence();
    [HideInInspector] public Sequence autoPlaySequence = new Sequence();

    public Indicator GetIndicator()
    {
        return new Indicator(this);
    }

    public class Indicator
    {
        public readonly Chart chart;

        public IReadOnlyList<Sequence.Indicator> iNoteList { get; private set; } = new List<Sequence.Indicator>();
        public Sequence.Indicator iBPMChange { get; private set; }
        public Sequence.Indicator iAutoPlay { get; private set; }

        public Indicator(Chart chart)
        {
            this.chart = chart;

            List<Sequence.Indicator> iNoteList = this.iNoteList as List<Sequence.Indicator>;

            foreach (var sequence in chart.noteSequences)
            {
                // �� ������ ���� �ε����� �����մϴ�.
                iNoteList.Add(new Sequence.Indicator(sequence));
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
            foreach (var indicator in iNoteList)
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