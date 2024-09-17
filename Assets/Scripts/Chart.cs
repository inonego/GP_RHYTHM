using System.Collections.Generic;
using UnityEngine;

public class Chart
{
    public List<Sequence> sequences = new List<Sequence>();

    public double initialBPM = 120.0;

    public Indicator GetIndicator()
    {
        return new Indicator(this);
    }

    public BeatToTimeConverter GetBeatToTimeConverter()
    {
        return new BeatToTimeConverter(this);
    }

    public class BeatToTimeConverter
    {
        public readonly Chart chart;

        private SortedList<BPMChangeEvent> events = new SortedList<BPMChangeEvent>();

        public BeatToTimeConverter(Chart chart)
        {
            this.chart = chart;

            foreach (var sequence in chart.sequences)
            {
                // 시퀀스의 이벤트 목록에서 BPMChangeEvent만 찾아서 추가합니다.
                foreach (var e in sequence.events)
                {
                    if (e is BPMChangeEvent bpmChangeEvent)
                    {
                        events.Add(bpmChangeEvent);
                    }
                }
            }
        }

        public double Convert(double beat)
        {
            double elapsedTime = 0.0;
            double previousBeat = 0.0;
            double currentBPM = chart.initialBPM;

            foreach (BPMChangeEvent e in events)
            {
                if (e.beat >= beat) break;

                elapsedTime += (e.beat - previousBeat) * (60.0 / currentBPM);
                previousBeat = e.beat;
                currentBPM = e.bpm;
            }

            elapsedTime += (beat - previousBeat) * (60.0 / currentBPM);

            return elapsedTime;
        }
    }

    public class Indicator
    {
        public readonly Chart chart;

        private List<Sequence.Indicator> pIndicators = new List<Sequence.Indicator>();

        public IReadOnlyList<Sequence.Indicator> indicators => pIndicators;

        public Indicator(Chart chart)
        {
            this.chart = chart;

            foreach (var sequence in chart.sequences)
            {
                pIndicators.Add(sequence.GetIndicator());
            }
        }

        public void ResetIndex()
        {
            foreach (var indicator in indicators)
            {
                indicator.ResetIndex();
            }
        }

        public void MoveIndex(double beat)
        {
            foreach (var indicator in indicators)
            {
                indicator.MoveIndex(beat);
            }
        }
    }
}