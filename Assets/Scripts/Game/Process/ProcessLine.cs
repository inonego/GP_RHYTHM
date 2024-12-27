using System;

using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ProcessLine : ProcessLineBase
{
    public delegate void InputProcessEvent(InputDATA inputDATA);
    public delegate void NoteProcessEvent(NoteDATA noteDATA);
    public delegate Note SpawnFunc(NoteEvent noteEvent);

    public event InputProcessEvent OnInputProcess;
    public event NoteProcessEvent OnNoteProcess;
    public SpawnFunc NoteSpawnFunc;

    public ProcessLine(Sequence<NoteEvent> sequence) : base(sequence) { }

    protected override void OnKeyPressed(InputDATA inputDATA)
    {
        base.OnKeyPressed(inputDATA);

        OnInputProcess(inputDATA);
    }

    protected override void OnKeyReleased(InputDATA inputDATA)
    {
        base.OnKeyReleased(inputDATA);

        OnInputProcess(inputDATA);
    }

    protected override void Pass()
    {
        if (currentNoteDATA != null)
        {
            currentNoteDATA.Value.Note.SetWorking(false);
        }

        base.Pass();
    }

    protected override void OnNoteClear(Note note)
    {
        NoteDATA noteDATA;

        noteDATA.Note    = note;

        var judgeDATA = new NoteDATA.NoteJudgeDATA();

        judgeDATA.Type    = JudgeType.Clear;
        judgeDATA.Time    = keyPressedInputDATA.Value.Time;
        judgeDATA.Score   = Judger.Instance.CalculateScore(keyPressedInputDATA.Value.Time, note);

        noteDATA.JudgeDATA = judgeDATA;

        OnNoteProcess(noteDATA);

        base.OnNoteClear(note);
    }

    protected override void OnNoteBreak(Note note)
    {
        NoteDATA noteDATA;

        noteDATA.Note    = note;

        var judgeDATA = new NoteDATA.NoteJudgeDATA();

        judgeDATA.Type    = JudgeType.Break;
        judgeDATA.Time    = Judger.Instance.GetMaxBreakCriteriaTime(note);
        judgeDATA.Score   = 0.0f;

        noteDATA.JudgeDATA = judgeDATA;

        OnNoteProcess(noteDATA);

        base.OnNoteBreak(note);
    }

    protected override void OnNoteMiss(Note note)
    {
        NoteDATA noteDATA;

        noteDATA.Note    = note;

        var judgeDATA = new NoteDATA.NoteJudgeDATA();

        judgeDATA.Type    = JudgeType.Miss;
        judgeDATA.Time    = Judger.Instance.GetMissCriteriaTime(note);
        judgeDATA.Score   = 0.0f;

        noteDATA.JudgeDATA = judgeDATA;

        OnNoteProcess(noteDATA);
        
        base.OnNoteMiss(note);
    }

    protected override NoteDATA? Spawn(NoteEvent noteEvent)
    {
        Note note = NoteSpawnFunc(noteEvent);

        return new NoteDATA { Note = note, JudgeDATA = null };
    }
}
