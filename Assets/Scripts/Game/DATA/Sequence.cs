using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// �̺�Ʈ�� ������� �����ϰ� ó���� �� �ֵ��� �ϴ� �������Դϴ�.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Sequence<T> where T : GameEvent, new()
{
    /// <summary>
    /// ��ü �̺�Ʈ�� ����Դϴ�.
    /// </summary>
    [field: SerializeField]
    public SortedList<T> Events { get; private set; } = new SortedList<T>();

    /// <summary>
    /// �̺�Ʈ�� ã���� ����ϴ� �ӽ� �����Դϴ�.
    /// </summary>
    private readonly T searchEvent = new T();

    /// <summary>
    /// ������ ��Ʈ ���ķ� ������ ù��° �̺�Ʈ�� �ε����� ���մϴ�.
    /// </summary>
    /// <param name="beat">������ �Ǵ� ��Ʈ�Դϴ�.</param>
    public int Search(double beat)
    {
        searchEvent.Beat = beat;

        return Events.BinarySearch(searchEvent);
    }
}
