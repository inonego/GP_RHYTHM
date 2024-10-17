using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum NoteProcessType
{
    Judge, Miss, Break
}

public enum LongProcessType
{
    Press, Break, Clear
}

public struct NoteProcessDATA
{
    public Note Note;
    public NoteProcessType Type;
    public double Time;
    public float Score;
}

public struct LongProcessDATA
{
    public Note Note;
    public LongProcessType Type;

    public LongProcessDATA(Note note, LongProcessType type)
    {
        Note = note;
        Type = type;
    }
}

[Serializable]
public class ProcessLine
{
    #region 필드 변수

    public readonly Sequence<NoteEvent> Sequence;

    private InputQueue inputQueue;
    private NoteQueue noteQueue;
    private int noteEventIndex;

    private NoteProcessDATA currentNoteProcessDATA;

    public bool IsLongOnProcess => currentNoteProcessDATA.Note != null && currentNoteProcessDATA.Note.IsLong;

    public delegate void KeyEvent(InputDATA inputDATA);
    public delegate void NoteProcessEvent(NoteProcessDATA noteProcessDATA);
    public delegate void LongProcessEvent(LongProcessDATA longProcessDATA);
    public delegate Note NoteSpawnDelegate(NoteEvent noteEvent);

    public KeyEvent OnKeyEvent;
    public NoteProcessEvent OnNoteProcess;
    public LongProcessEvent OnLongProcess;
    public NoteSpawnDelegate NoteSpawnFunc;

    #endregion

    #region 초기화 메서드

    public ProcessLine(Sequence<NoteEvent> sequence)
    {
        Sequence = sequence;

        inputQueue = new InputQueue();
        noteQueue = new NoteQueue();
        noteEventIndex = 0;

        ResetCurrent();
    }

    public void Bind(InputAction inputAction)
    {
        inputQueue.Bind(inputAction);
    }

    public void ResetCurrent()
    {
        currentNoteProcessDATA = new NoteProcessDATA();
    }

    internal void Release()
    {
        inputQueue.ReleaseAll();
    }

    #endregion

    #region 스폰 처리 메서드

    public void ProcessSpawn(double spawnTargetBeat)
    {
        // 노트 스폰 처리입니다.
        while (noteEventIndex < Sequence.Events.Count)
        {
            // 현재 노트 인덱스에 해당하는 노트 인덱스를 가지고 옵니다.
            NoteEvent noteEvent = Sequence.Events[noteEventIndex];

            // targetBeat를 넘어서는 노트 이벤트는 처리하지 않습니다.
            if (noteEvent.Beat > spawnTargetBeat)
            {
                break;
            }

            // 스폰된 노트를 노트 큐 목록에 추가합니다.
            noteQueue.Enqueue(NoteSpawnFunc(noteEvent));

            // 인덱스를 업데이트합니다.
            noteEventIndex++;
        }
    }

    #endregion

    #region 판정 처리 메서드

    public void ProcessJudge(double currentPlayTime)
    {
        while (noteQueue.TryPeek(out Note note))
        {
            // 입력이 있다면 입력을 처리합니다.
            if (inputQueue.TryDequeue(out InputDATA inputDATA))
            {
                OnKeyEvent(inputDATA);

                bool isKeyPressed = inputDATA.Type == InputType.KeyDown;

                if (isKeyPressed)
                {
                    OnKeyPressed(inputDATA.Time);
                }
                else
                {
                    OnKeyReleased(inputDATA.Time);
                }
            }
            else
            {
                // 현재 롱 노트를 처리하고 있는지 체크합니다.
                if (IsLongOnProcess)
                {
                    // 롱 노트가 판정 선을 지나면 처리를 중지하고 판정을 내립니다.
                    if (Judger.Instance.CheckLongCleared(currentPlayTime, note))
                    {
                        OnLongCleared();
                    }
                    // 현재 노트가 아직 처리되지 않았다면 루프를 탈출합니다.
                    else break;
                }
                else
                {
                    if (Judger.Instance.CheckMissed(currentPlayTime, note))
                    {
                        OnMissed();
                    }
                    // 현재 노트가 Miss되지 않았다면 더 이상 처리할 노트가 없으므로 루프를 탈출합니다.
                    else break;
                }
            }
        }
    }

    // 현재 노트를 처리(판정)하고 넘깁니다.
    private void Pass()
    {
        Note note = noteQueue.Dequeue();

        note.SetWorking(false);

        OnNoteProcess(currentNoteProcessDATA);

        ResetCurrent();
    }

    // 키를 눌렀을때
    private void OnKeyPressed(double inputTime)
    {
        Note note = noteQueue.Peek();

        if (Judger.Instance.CheckInputTimeValid(inputTime, note))
        {
            currentNoteProcessDATA.Note    = note;
            currentNoteProcessDATA.Type    = NoteProcessType.Judge;
            currentNoteProcessDATA.Time    = inputTime;
            currentNoteProcessDATA.Score   = Judger.Instance.CalculateScore(inputTime, note);

            // 롱 노트가 아닌 경우 판정을 즉시 처리합니다.
            if (IsLongOnProcess)
            {
                OnLongProcess(new LongProcessDATA(note, LongProcessType.Press));
            }
            else
            {
                Pass();
            }
        }
        else
        {
            if (Judger.Instance.CheckBreaked(inputTime, note))
            {
                OnBreaked();
            }
        }
    }

    // 키를 떼었을때
    private void OnKeyReleased(double inputTime)
    {
        Note note = noteQueue.Peek();

        // 현재 롱 노트를 처리하고 있다면 Break 판정을 내립니다.
        if (IsLongOnProcess)
        {
            currentNoteProcessDATA.Note    = note;
            currentNoteProcessDATA.Type    = NoteProcessType.Break;
            currentNoteProcessDATA.Time    = inputTime;
            currentNoteProcessDATA.Score   = 0.0f;

            Pass();

            OnLongProcess(new LongProcessDATA(note, LongProcessType.Break));
        }
    }

    // 롱 노트 Clear가 되었을때
    private void OnLongCleared()
    {
        Note note = noteQueue.Peek();

        Pass();

        OnLongProcess(new LongProcessDATA(note, LongProcessType.Clear));
    }

    // 노트 Break가 되었을때
    private void OnBreaked()
    {
        Note note = noteQueue.Peek();

        currentNoteProcessDATA.Note    = note;
        currentNoteProcessDATA.Type    = NoteProcessType.Break;
        currentNoteProcessDATA.Time    = Judger.Instance.GetMaxBreakedCriteriaTime(note);
        currentNoteProcessDATA.Score   = 0.0f;

        Pass();
    }

    // 노트 Miss가 되었을때
    private void OnMissed()
    {
        Note note = noteQueue.Peek();

        currentNoteProcessDATA.Note     = note;
        currentNoteProcessDATA.Type     = NoteProcessType.Miss;
        currentNoteProcessDATA.Time     = Judger.Instance.GetMissedCriteriaTime(note);
        currentNoteProcessDATA.Score    = 0.0f;

        Pass();
    }

    #endregion
}
