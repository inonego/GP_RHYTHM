using System;

[Serializable]
public class Sequence
{
    public SortedList<GameEvent> events = new SortedList<GameEvent>();

    public Indicator GetIndicator()
    {
        return new Indicator(this);
    }

    public class Indicator
    {
        public readonly Sequence sequence;

        public int index { get; private set; }

        public GameEvent currentEvent => sequence.events[index];

        public Indicator(Sequence sequence)
        {
            this.sequence = sequence;
        }

        public void ResetIndex()
        {
            index = 0;
        }

        public void MoveIndex(int index)
        {
            this.index = index;
        }

        public void MoveIndex(double beat)
        {
            if (currentEvent.beat != beat)
            {
                // 현재 이벤트보다 앞의 시간이라면 인덱스를 초기화합니다.
                if (currentEvent.beat > beat)
                {
                    ResetIndex();
                }

                // 시간과 가장 근접한 이벤트를 찾습니다.
                while (index < sequence.events.Count)
                {
                    if (currentEvent.beat <= beat) break;

                    index++;
                }
            }
        }
    }
}
