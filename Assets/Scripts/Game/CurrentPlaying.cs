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
            // �� ������ ���� �ε����� �����մϴ�.
            indicators.Add(new Indicator(sequence));

            // �������� �̺�Ʈ ��Ͽ��� BPMChangeEvent�� ã�Ƽ� �߰��մϴ�.
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
    /// ������ �ð����� �ڿ� �ִ� ���� ù��° �̺�Ʈ�� �ε����� �̵��մϴ�. 
    /// </summary>
    /// <param name="time">������ �Ǵ� �ð��Դϴ�.</param>
    public void MoveTo(double beat)
    {
        foreach (var indicator in indicators)
        {
            MoveTo(indicator, beat);
        }
    }

    /// <summary>
    /// ������ �ð����� �ڿ� �ִ� ���� ù��° �̺�Ʈ�� �ε����� �̵��մϴ�.
    /// </summary>
    /// <param name="SQ">�������� �ε����Դϴ�.</param>
    /// <param name="time">������ �Ǵ� �ð��Դϴ�.</param>
    private void MoveTo(Indicator indicator, double time)
    { 
        double GetCurrentEventTime()
        {
            return ConvertBeatToTime(indicator.HasCurrent() ? indicator.currentEvent.beat : (indicator.lastEvent != null ? indicator.lastEvent.beat : 0.0));
        }

        if (GetCurrentEventTime() != time)
        {
            // ���� �̺�Ʈ���� ���� �ð��̶�� �ε����� �ʱ�ȭ�մϴ�.
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
    /// ���� �ε������� �ڿ� �����鼭 ������ Ÿ�Կ� �ش��ϴ� ���� ù��° �̺�Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <typeparam name="T">������ �Ǵ� Ÿ���Դϴ�./typeparam>
    /// <param name="SQ">�������� �ε����Դϴ�.</param>
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
