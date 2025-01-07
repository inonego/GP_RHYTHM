using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct NoteDATA
{
    public Note Note;
}

[Serializable]
public struct NoteJudgeDATA
{
    public Note Note;

    public JudgeType Type;
    public float Score;
    public double Time;
}

[Serializable]
public class ProcessLine
{
    public readonly Sequence<NoteEvent> Sequence;

#region 변수

    private int index = 0;
    
    protected InputDATAQueue inputDATAQueue = new InputDATAQueue();
    protected NoteDATAQueue noteDATAQueue   = new NoteDATAQueue();
    
    public InputDATA? keyPressedInputDATA;
    public InputDATA? keyReleasedInputDATA;

    private bool isKeyPressed   => keyPressedInputDATA  != null;
    private bool isKeyReleased  => keyReleasedInputDATA != null;

    protected NoteDATA? currentNoteDATA = null;

    public double CurrentPlayTime { get; private set; } = 0.0;

    /// <summary>
    /// 현재 플레이 시간을 설정합니다.
    /// </summary>
    /// <param name="currentPlayTime"></param>
    public void SetCurrentPlayTime(double currentPlayTime) => CurrentPlayTime = currentPlayTime;

    #endregion

#region 이벤트 및 델리게이트

    // 입력 처리에 대한 이벤트입니다.
    public delegate void InputProcessEvent(InputDATA inputDATA);
    public event InputProcessEvent OnInputProcess;

    // 노트 처리에 대한 이벤트입니다.
    public delegate void NoteProcessEvent(NoteDATA noteDATA);
    public event NoteProcessEvent OnNoteProcess;
    
    // 노트 판정에 대한 이벤트입니다.
    public delegate void NoteJudgeEvent(NoteJudgeDATA noteJudgeDATA);
    public event NoteJudgeEvent OnNoteJudge;

    // 노트 생성에 대한 델리게이트입니다.
    public delegate Note SpawnFunc(NoteEvent noteEvent);
    public SpawnFunc NoteSpawnFunc;

    #endregion

#region 초기화 메서드

    public ProcessLine(Sequence<NoteEvent> sequence)
    {
        Sequence = sequence;
    }

    public void Bind(InputAction inputAction)
    {
        inputDATAQueue.Bind(inputAction);
    }

    public void Release()
    {
        inputDATAQueue.Release();
    }

#endregion

#region 스폰 및 판정

    /// <summary>
    /// 지정된 박자까지의 노트를 스폰합니다.
    /// </summary>
    /// <param name="spawnTargetBeat"></param>
    public void Spawn(double spawnTargetBeat)
    { 
        // 노트 스폰 처리입니다.
        while (index < Sequence.Events.Count)
        {
            // 현재 노트 인덱스에 해당하는 노트 인덱스를 가지고 옵니다.
            NoteEvent noteEvent = Sequence.Events[index];

            // spawnTargetBeat를 넘어서는 노트 이벤트는 처리하지 않습니다.
            if (noteEvent.Beat > spawnTargetBeat)
            {
                break;
            }

            // 스폰된 노트를 노트 큐 목록에 추가합니다.
            noteDATAQueue.Enqueue(Spawn(noteEvent));

            // 인덱스를 업데이트합니다.
            index++;
        }
    }

    protected NoteDATA Spawn(NoteEvent noteEvent)
    {
        Note note = NoteSpawnFunc(noteEvent);

        return new NoteDATA { Note = note };
    }

    public void Judge()
    {
        InputDATA? inputDATA;

        bool HasInputDATA()       =>       inputDATA != null;
        bool HasCurrentNoteDATA() => currentNoteDATA != null;

        do
        {
            // 처리중인 노트가 없다면 노트를 하나 꺼냅니다.
            if (!HasCurrentNoteDATA())
            {   
                bool exists = noteDATAQueue.TryDequeue(out currentNoteDATA);

                // 처리할 노트가 없다면 루프를 탈출합니다.
                if (!exists) break;
            }

            // 입력을 하나 처리한 다음 노트를 하나 처리하는 식으로 진행되어야합니다.

            // 처리할 입력이 있다면 입력을 하나 꺼냅니다.
            if (inputDATAQueue.TryDequeue(out inputDATA))
            {
                // 입력을 처리합니다.
                ProcessInputDATA(inputDATA.Value);
            }
            
            // 노트를 처리합니다.
            ProcessNoteDATA(currentNoteDATA.Value);
        }
        // 모든 입력을 다 처리했는데도 노트의 판정이 완료되지 못하면 루프를 그만 돌리도록 해야합니다.
        while (HasInputDATA() || !HasCurrentNoteDATA());
    }

#endregion

#region 입력 처리

    /// <summary>
    /// 입력을 처리합니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected void ProcessInputDATA(InputDATA inputDATA)
    { 
        // 키가 눌렸다면 KeyPressed 이벤트를 발생시킵니다.
        if (inputDATA.State == KeyState.Pressed)
        {   
            OnKeyPressed(inputDATA);
        }
        // 키가 떼졌다면 KeyReleased 이벤트를 발생시킵니다.
        else
        {
            if (isKeyPressed)
            {
                OnKeyReleased(inputDATA);
            }
        }
    }

    /// <summary>
    /// 키가 눌렸을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected void OnKeyPressed(InputDATA inputDATA)
    {
        keyPressedInputDATA = inputDATA;

        OnInputProcess(inputDATA);
    }

    /// <summary>
    /// 키가 떼졌을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected void OnKeyReleased(InputDATA inputDATA)
    {
        keyReleasedInputDATA = inputDATA;

        OnInputProcess(inputDATA);
    }

#endregion

#region 노트 처리

    /// <summary>
    /// 노트가 눌렸을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void OnNotePressed(NoteDATA noteDATA)
    {
        noteDATA.Note.SetPressed(true);
        
        OnNoteProcess(noteDATA);
    }

    /// <summary>
    /// 노트가 떼졌을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void OnNoteReleased(NoteDATA noteDATA)
    {
        noteDATA.Note.SetPressed(false);

        OnNoteProcess(noteDATA);
    }

    /// <summary>
    /// 노트를 처리합니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void ProcessNoteDATA(NoteDATA noteDATA)
    {
        if (!noteDATA.Note.IsPressed)
        {
            if (isKeyPressed)
            {
                // 키가 눌린 시간이 유효한지 확인합니다.
                if (Judger.Instance.CheckInputTimeValid(keyPressedInputDATA.Value.Time, noteDATA.Note))
                {
                    OnNotePressed(noteDATA);
                }
                else
                {
                    // 노트가 Break되었다면 Break 이벤트를 발생시킵니다.
                    if (Judger.Instance.CheckBreak(keyPressedInputDATA.Value.Time, noteDATA.Note))
                    {
                        OnNoteBreak(noteDATA);
                        OnNoteReleased(noteDATA);

                        return;
                    }

                    // 유효하지 않은 경우 키 입력을 초기화합니다.
                    keyPressedInputDATA = null;
                    keyReleasedInputDATA = null;
                }
            }
        }
        
        if (noteDATA.Note.IsPressed)
        {
            // 성공적으로 판정되는 경우를 처리합니다.
            if (Judger.Instance.CheckClear(CurrentPlayTime, noteDATA.Note))
            { 
                OnNoteClear(noteDATA);
                OnNoteReleased(noteDATA);

                return;
            }
            else
            {
                if (isKeyReleased)
                {
                    // 롱 노트에 대한 처리입니다.
                    if (noteDATA.Note.IsLong)
                    {
                        OnNoteBreak(noteDATA);
                        OnNoteReleased(noteDATA);
                        
                        return;
                    }
                }
            }
        }
        else
        {
            // 노트가 Miss되었다면 Miss 이벤트를 발생시킵니다.
            if (Judger.Instance.CheckMiss(CurrentPlayTime, noteDATA.Note))
            {
                OnNoteMiss(noteDATA);
                OnNoteReleased(noteDATA);
                
                return;
            }
        }
    }

#endregion

#region 판정 처리

    /// <summary>
    /// 현재 처리중인 노트에 대해 처리 완료 표시합니다.
    /// </summary>
    protected void Pass()
    {
        currentNoteDATA = null;

        keyPressedInputDATA = null;
        keyReleasedInputDATA = null;
    }

    /// <summary>
    /// 노트가 Clear되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void OnNoteClear(NoteDATA noteDATA)
    {
        var judgeDATA = new NoteJudgeDATA();

        judgeDATA.Note    = noteDATA.Note;
        judgeDATA.Type    = JudgeType.Clear;
        judgeDATA.Time    = keyPressedInputDATA.Value.Time;
        judgeDATA.Score   = Judger.Instance.CalculateScore(keyPressedInputDATA.Value.Time, noteDATA.Note);

        OnNoteJudge(judgeDATA);

        Pass();
    }

    /// <summary>
    /// 노트가 Break되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void OnNoteBreak(NoteDATA noteDATA)
    {
        var judgeDATA = new NoteJudgeDATA();

        judgeDATA.Note    = noteDATA.Note;
        judgeDATA.Type    = JudgeType.Break;
        judgeDATA.Time    = Judger.Instance.GetMaxBreakCriteriaTime(noteDATA.Note);
        judgeDATA.Score   = 0.0f;

        OnNoteJudge(judgeDATA);

        Pass();
    }

    /// <summary>
    /// 노트가 Miss되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void OnNoteMiss(NoteDATA noteDATA)
    {
        var judgeDATA = new NoteJudgeDATA();

        judgeDATA.Note    = noteDATA.Note;
        judgeDATA.Type    = JudgeType.Miss;
        judgeDATA.Time    = Judger.Instance.GetMissCriteriaTime(noteDATA.Note);
        judgeDATA.Score   = 0.0f;

        OnNoteJudge(judgeDATA);

        Pass();
    }

#endregion

}
