using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CurrentPlaying
{
    public readonly Chart chart;

    private class Indicator
    {
        public readonly Sequence sequence;

        public int index { get; private set; } = 0;

        public GameEvent firstEvent => sequence.events.FirstOrDefault();
        public GameEvent lastEvent => sequence.events.LastOrDefault();

        public GameEvent currentEvent => sequence.events[index];

        public Indicator(Sequence sequence)
        {
            this.sequence = sequence;
        }

        public void Reset()
        {
            index = 0;
        }

        public bool HasCurrent()
        {
            return 0 <= index && index < sequence.events.Count;
        }

        public void MoveNext()
        {
            index++;
        }
    }

    private List<Indicator> indicators = new List<Indicator>();

    private Sequence bpmChangeSequence = new Sequence();

    public CurrentPlaying(Chart chart)
    {
        this.chart = chart;

        foreach (var sequence in chart.sequences)
        {
            // 각 시퀀스 별로 인덱스를 설정합니다.
            indicators.Add(new Indicator(sequence));

            // 시퀀스의 이벤트 목록에서 BPMChangeEvent만 찾아서 추가합니다.
            foreach (var e in sequence.events)
            {
                if (e is BPMChangeEvent bpmChangeEvent)
                {
                    bpmChangeSequence.events.Add(bpmChangeEvent);
                }
            }
        }
    }

    public double ConvertBeatToTime(double beat)
    {
        double elapsedTime = 0.0;
        double previousBeat = 0.0;
        double currentBPM = chart.initialBPM;

        foreach (BPMChangeEvent e in bpmChangeSequence.events)
        {
            if (e.beat >= beat) break;

            elapsedTime += (e.beat - previousBeat) * (60.0 / currentBPM);
            previousBeat = e.beat;
            currentBPM = e.bpm;
        }

        elapsedTime += (beat - previousBeat) * (60.0 / currentBPM);

        return elapsedTime;
    }

    /// <summary>
    /// 지정된 시간보다 뒤에 있는 가장 첫번째 이벤트로 인덱스를 이동합니다. 
    /// </summary>
    /// <param name="time">기준이 되는 시간입니다.</param>
    public void MoveTo(double beat)
    {
        foreach (var indicator in indicators)
        {
            MoveTo(indicator, beat);
        }
    }

    /// <summary>
    /// 지정된 시간보다 뒤에 있는 가장 첫번째 이벤트로 인덱스를 이동합니다.
    /// </summary>
    /// <param name="SQ">시퀀스의 인덱스입니다.</param>
    /// <param name="time">기준이 되는 시간입니다.</param>
    private void MoveTo(Indicator indicator, double time)
    { 
        double GetCurrentEventTime()
        {
            return ConvertBeatToTime(indicator.HasCurrent() ? indicator.currentEvent.beat : (indicator.lastEvent != null ? indicator.lastEvent.beat : 0.0));
        }

        if (GetCurrentEventTime() != time)
        {
            // 현재 이벤트보다 앞의 시간이라면 인덱스를 초기화합니다.
            if (GetCurrentEventTime() > time)
            {
                indicator.Reset();
            }

            while (indicator.HasCurrent())
            {
                if (GetCurrentEventTime() <= time) return;

                indicator.MoveNext();
            }
        }
    }

    /// <summary>
    /// 현재 인덱스보다 뒤에 있으면서 지정된 타입에 해당하는 가장 첫번째 이벤트를 반환합니다.
    /// </summary>
    /// <typeparam name="T">기준이 되는 타입입니다./typeparam>
    /// <param name="SQ">시퀀스의 인덱스입니다.</param>
    /// <returns></returns>
    private T GetEvent<T>(Indicator indicator) where T : GameEvent
    { 
        while (indicator.HasCurrent())
        {
            if (indicator.currentEvent is T currentEvent) return currentEvent;

            indicator.MoveNext();
        }

        return null;
    }
}
