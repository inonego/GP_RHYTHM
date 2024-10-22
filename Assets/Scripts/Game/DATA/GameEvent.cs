using System;

[Serializable]
public abstract class GameEvent : IComparable<GameEvent>
{
    /// <summary>
    /// �̺�Ʈ�� �߻��ϴ� ��Ʈ�� �����Դϴ�.
    /// </summary>
    public double Beat;
    /// <summary>
    /// �̺�Ʈ�� ���ӵǴ� ��Ʈ�� �����Դϴ�.
    /// </summary>
    public double Duration;

    public bool isLong => Duration > 0;

    public GameEvent() : this(0.0, 0.0) { }

    public GameEvent(double beat, double duration)
    {
       Beat = beat;
       Duration = duration;
    }

    public int CompareTo(GameEvent other)
    {
        return Beat.CompareTo(other.Beat);
    }
}

/// <summary>
/// ��Ʈ�� ���� �̺�Ʈ�Դϴ�.
/// </summary>
[Serializable]
public class NoteEvent : GameEvent { }

/// <summary>
/// BPM ���濡 ���� �̺�Ʈ�Դϴ�.
/// </summary>
[Serializable]
public class BPMChangeEvent : GameEvent
{
    /// <summary>
    /// �̺�Ʈ�� ó���ǵ��� �ϴ� BPM�� �����ϴ� ���Դϴ�.
    /// </summary>
    public double BPM;
}

/// <summary>
/// �ڵ� �÷��̿� ���� �̺�Ʈ�Դϴ�.
/// </summary>
[Serializable]
public class AutoPlayEvent : GameEvent { }