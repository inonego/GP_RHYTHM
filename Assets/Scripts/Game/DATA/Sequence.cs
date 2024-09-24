using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 이벤트를 목록으로 관리하고 처리할 수 있도록 하는 시퀀스입니다.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Sequence<T> where T : GameEvent, new()
{
    /// <summary>
    /// 전체 이벤트의 목록입니다.
    /// </summary>
    [field: SerializeField]
    public SortedList<T> Events { get; private set; } = new SortedList<T>();

    /// <summary>
    /// 이벤트를 찾을때 사용하는 임시 변수입니다.
    /// </summary>
    private readonly T searchEvent = new T();

    /// <summary>
    /// 지정된 비트 이후로 나오는 첫번째 이벤트의 인덱스를 구합니다.
    /// </summary>
    /// <param name="beat">기준이 되는 비트입니다.</param>
    public int Search(double beat)
    {
        searchEvent.Beat = beat;

        return Events.BinarySearch(searchEvent);
    }
}
