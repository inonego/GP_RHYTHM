using System;

[Serializable]
public abstract class GameEvent : IComparable<GameEvent>
{
    /// <summary>
    /// 이벤트가 발생하는 비트의 시점입니다.
    /// </summary>
    public double Beat;
    /// <summary>
    /// 이벤트가 지속되는 비트의 길이입니다.
    /// </summary>
    public double Duration;

    public double CachedTime    { get; private set; }
    public double CachedLength  { get; private set; }

    internal void MakeCache(double time, double length)
    {
        CachedTime = time;
        CachedLength = length;
    }

    internal void ClearCache()
    {
        CachedTime = 0.0;
        CachedLength = 0.0;
    }

    public bool isLong => Duration > 0;

    public GameEvent() : this(0.0, 0.0) { }

    public GameEvent(double beat, double duration)
    {
       Beat = beat;
       Duration = duration;

       ClearCache();
    }

    public int CompareTo(GameEvent other)
    {
        return Beat.CompareTo(other.Beat);
    }
}

/// <summary>
/// 노트에 대한 이벤트입니다.
/// </summary>
[Serializable]
public class NoteEvent : GameEvent { }

/// <summary>
/// BPM 변경에 대한 이벤트입니다.
/// </summary>
[Serializable]
public class BPMChangeEvent : GameEvent
{
    /// <summary>
    /// 이벤트가 처리되도록 하는 BPM를 설정하는 값입니다.
    /// </summary>
    public double BPM;
}

/// <summary>
/// 속도 변경에 대한 이벤트입니다.
/// </summary>
[Serializable]
public class SpeedChangeEvent : GameEvent
{
    /// <summary>
    /// 이벤트가 처리되도록 하는 속도를 설정하는 값입니다.
    /// </summary>
    public float Speed;
}