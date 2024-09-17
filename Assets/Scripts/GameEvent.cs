using System;

[Serializable]
public abstract class GameEvent : IComparable<GameEvent>
{
    public double beat, duration;

    public GameEvent() : this(0d, 0d) { }

    public GameEvent(double beat, double duration)
    {
        this.beat = beat;
        this.duration = duration;
    }

    public int CompareTo(GameEvent other)
    {
        return beat.CompareTo(other.beat);
    }
}

[Serializable]
public class NoteEvent : GameEvent { }

[Serializable]
public class BPMChangeEvent : GameEvent
{
    public double bpm;
}

[Serializable]
public class AutoPlayEvent : GameEvent { }