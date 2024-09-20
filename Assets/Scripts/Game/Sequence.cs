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
        /// 인덱스를 초기화합니다.
        /// </summary>
        private void Reset()
        {
            index = 0;
        }

        /// <summary>
        /// 현재 인덱스가 가리키는 이벤트가 있는지 반환합니다.
        /// </summary>
        /// <returns></returns>
        private bool HasCurrent()
        {
            return 0 <= index && index < sequence.events.Count;
        }

        /// <summary>
        /// 인덱스를 증가시킵니다.
        /// </summary>
        private void MoveNext()
        {
            index++;
        }

        /// <summary>
        /// 지정된 박자보다 뒤에 있는 가장 첫번째 이벤트로 인덱스를 이동합니다.
        /// </summary>
        /// <param name="beat">기준이 되는 박자입니다.</param>
        public void MoveTo(double beat)
        {
            double GetCurrentEventBeat()
            {
                return HasCurrent() ? currentEvent.beat : (lastEvent != null ? lastEvent.beat : 0.0);
            }

            if (GetCurrentEventBeat() != beat)
            {
                // 현재 이벤트보다 앞의 시간이라면 인덱스를 초기화합니다.
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
        /// 현재 인덱스와 같거나 그 뒤에 있으면서 지정된 타입에 해당하는 가장 첫번째 이벤트를 반환합니다.
        /// </summary>
        /// <typeparam name="T">기준이 되는 타입입니다./typeparam>
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
