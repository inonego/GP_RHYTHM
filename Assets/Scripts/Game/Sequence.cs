using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Sequence
{
    [field: SerializeField]
    public SortedList<GameEvent> events { get; private set; } = new SortedList<GameEvent>();

    public Indicator GetIndicator()
    {
        return new Indicator(this);
    }

    public class Indicator
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

        /// <summary>
        /// �ε����� �ʱ�ȭ�մϴ�.
        /// </summary>
        private void Reset()
        {
            index = 0;
        }

        /// <summary>
        /// ���� �ε����� ����Ű�� �̺�Ʈ�� �ִ��� ��ȯ�մϴ�.
        /// </summary>
        /// <returns></returns>
        private bool HasCurrent()
        {
            return 0 <= index && index < sequence.events.Count;
        }

        /// <summary>
        /// �ε����� ������ŵ�ϴ�.
        /// </summary>
        private void MoveNext()
        {
            index++;
        }

        /// <summary>
        /// ������ ���ں��� �ڿ� �ִ� ���� ù��° �̺�Ʈ�� �ε����� �̵��մϴ�.
        /// </summary>
        /// <param name="beat">������ �Ǵ� �����Դϴ�.</param>
        public void MoveTo(double beat)
        {
            double GetCurrentEventBeat()
            {
                return HasCurrent() ? currentEvent.beat : (lastEvent != null ? lastEvent.beat : 0.0);
            }

            if (GetCurrentEventBeat() != beat)
            {
                // ���� �̺�Ʈ���� ���� �ð��̶�� �ε����� �ʱ�ȭ�մϴ�.
                if (GetCurrentEventBeat() > beat)
                {
                    Reset();
                }

                while (HasCurrent())
                {
                    if (GetCurrentEventBeat() <= beat) return;

                    MoveNext();
                }
            }
        }

        /// <summary>
        /// ���� �ε����� ���ų� �� �ڿ� �����鼭 ������ Ÿ�Կ� �ش��ϴ� ���� ù��° �̺�Ʈ�� ��ȯ�մϴ�.
        /// </summary>
        /// <typeparam name="T">������ �Ǵ� Ÿ���Դϴ�./typeparam>
        public T GetEvent<T>() where T : GameEvent
        {
            while (HasCurrent())
            {
                if (currentEvent is T) return currentEvent as T;

                MoveNext();
            }

            return null;
        }
    }
}
