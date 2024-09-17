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
                // ���� �̺�Ʈ���� ���� �ð��̶�� �ε����� �ʱ�ȭ�մϴ�.
                if (currentEvent.beat > beat)
                {
                    ResetIndex();
                }

                // �ð��� ���� ������ �̺�Ʈ�� ã���ϴ�.
                while (index < sequence.events.Count)
                {
                    if (currentEvent.beat <= beat) break;

                    index++;
                }
            }
        }
    }
}
