using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct InputDATA
{
    public InputAction InputAction;
    public InputType Type;
    public double Time;
}

[Serializable]
public struct NoteDATA
{
    [Serializable]
    public struct NoteJudgeDATA
    {
        public JudgeType Type;
        public double Time;
        public float Score;
    }

    public Note Note;
    public NoteJudgeDATA? JudgeDATA;
}

[Serializable]
public abstract class ProcessLineBase
{
    public readonly Sequence<NoteEvent> Sequence;

    public double CurrentPlayTime { get; private set; } = 0.0;

    private int noteEventIndex = 0;

    #region 데이터
    
    protected InputDATAQueue inputDATAQueue = new InputDATAQueue();
    protected NoteDATAQueue noteDATAQueue  = new NoteDATAQueue();

    protected InputDATA? keyPressedInputDATA = null;
    protected InputDATA? keyReleasedInputDATA = null;

    protected NoteDATA? currentNoteDATA = null;

    #endregion

    public ProcessLineBase(Sequence<NoteEvent> sequence)
    {
        Sequence = sequence;
    }

    public void Bind(InputAction inputAction)
    {
        inputDATAQueue.Bind(inputAction);
    }

    public void Release()
    {
        inputDATAQueue.ReleaseAll();
    }

    public void SetCurrentPlayTime(double currentPlayTime)
    {
        CurrentPlayTime = currentPlayTime;
    }

    public void Spawn(double spawnTargetBeat)
    { 
        // 노트 스폰 처리입니다.
        while (noteEventIndex < Sequence.Events.Count)
        {
            // 현재 노트 인덱스에 해당하는 노트 인덱스를 가지고 옵니다.
            NoteEvent noteEvent = Sequence.Events[noteEventIndex];

            // spawnTargetBeat를 넘어서는 노트 이벤트는 처리하지 않습니다.
            if (noteEvent.Beat > spawnTargetBeat)
            {
                break;
            }

            // 스폰된 노트를 노트 큐 목록에 추가합니다.
            noteDATAQueue.Enqueue(Spawn(noteEvent));

            // 인덱스를 업데이트합니다.
            noteEventIndex++;
        }
    }

    protected abstract NoteDATA? Spawn(NoteEvent noteEvent);

    public void Judge()
    {
        InputDATA? inputDATA;

        do
        {
            // 처리중인 노트가 없다면 노트를 하나 꺼냅니다.
            if (currentNoteDATA == null)
            {   
                // 처리할 노트가 없다면 루프를 탈출합니다.
                if (!noteDATAQueue.TryDequeue(out currentNoteDATA)) break;
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
        while (inputDATA != null || currentNoteDATA == null);
    }

    /// <summary>
    /// 입력을 처리합니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected void ProcessInputDATA(InputDATA inputDATA)
    { 
        bool isKeyPressed = inputDATA.Type == InputType.KeyPressed;

        // 키가 눌렸다면 KeyPressed 이벤트를 발생시킵니다.
        if (isKeyPressed)
        {   
            OnKeyPressed(inputDATA);
        }
        // 키가 떼졌다면 KeyReleased 이벤트를 발생시킵니다.
        else
        {
            if (keyPressedInputDATA != null)
            {
                OnKeyReleased(inputDATA);
            }
        }
    }

    /// <summary>
    /// 키가 눌렸을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected virtual void OnKeyPressed(InputDATA inputDATA)
    {
        keyPressedInputDATA = inputDATA;
    }

    /// <summary>
    /// 키가 떼졌을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="inputDATA"></param>
    protected virtual void OnKeyReleased(InputDATA inputDATA)
    {
        keyReleasedInputDATA = inputDATA;
    }

    /// <summary>
    /// 노트를 처리합니다.
    /// </summary>
    /// <param name="noteDATA"></param>
    protected void ProcessNoteDATA(NoteDATA noteDATA)
    {
        // 키가 눌렸다면 판정을 처리합니다.
        if (keyPressedInputDATA != null)
        {   
            // 키가 눌린 시간이 유효한지 확인합니다.
            if (Judger.Instance.CheckInputTimeValid(keyPressedInputDATA.Value.Time, noteDATA.Note))
            {
                // 롱 노트가 처리 중인 경우 판정되기 이전에 키를 떼었을때의 경우를 처리합니다.
                if (noteDATA.Note.IsLong)
                {
                    if (keyReleasedInputDATA != null)
                    {
                        OnNoteBreak(noteDATA.Note);
                        
                        return;
                    }
                }

                // 성공적으로 판정되는 경우를 처리합니다.
                if (Judger.Instance.CheckClear(CurrentPlayTime, noteDATA.Note))
                { 
                    OnNoteClear(noteDATA.Note);

                    return;
                }
            }
            else
            {
                // 노트가 Break되었다면 Break 이벤트를 발생시킵니다.
                if (Judger.Instance.CheckBreak(keyPressedInputDATA.Value.Time, noteDATA.Note))
                {
                    OnNoteBreak(noteDATA.Note);

                    return;
                }

                // 유효하지 않은 경우 키 입력을 초기화합니다.
                keyPressedInputDATA = null;
                keyReleasedInputDATA = null;
            }
        }
        else
        {
            // 노트가 Miss되었다면 Miss 이벤트를 발생시킵니다.
            if (Judger.Instance.CheckMiss(CurrentPlayTime, noteDATA.Note))
            {
                OnNoteMiss(noteDATA.Note);
                
                return;
            }
        }
    }

    /// <summary>
    /// 현재 처리중인 노트에 대해 처리 완료 표시합니다.
    /// </summary>
    protected virtual void Pass()
    {
        currentNoteDATA = null;

        keyPressedInputDATA = null;
        keyReleasedInputDATA = null;
    }

    /// <summary>
    /// 노트가 Clear되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="note"></param>
    protected virtual void OnNoteClear(Note note)
    {
        Pass();
    }

    /// <summary>
    /// 노트가 Break되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="note"></param>
    protected virtual void OnNoteBreak(Note note)
    {
        Pass();
    }

    /// <summary>
    /// 노트가 Miss되었을때 호출되는 이벤트입니다.
    /// </summary>
    /// <param name="note"></param>
    protected virtual void OnNoteMiss(Note note)
    {
        Pass();
    }
}
