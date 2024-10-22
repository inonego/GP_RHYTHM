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
    #region �ʵ� ����

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

    #region �ʱ�ȭ �޼���

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

    #region ���� ó�� �޼���

    public void ProcessSpawn(double spawnTargetBeat)
    {
        // ��Ʈ ���� ó���Դϴ�.
        while (noteEventIndex < Sequence.Events.Count)
        {
            // ���� ��Ʈ �ε����� �ش��ϴ� ��Ʈ �ε����� ������ �ɴϴ�.
            NoteEvent noteEvent = Sequence.Events[noteEventIndex];

            // targetBeat�� �Ѿ�� ��Ʈ �̺�Ʈ�� ó������ �ʽ��ϴ�.
            if (noteEvent.Beat > spawnTargetBeat)
            {
                break;
            }

            // ������ ��Ʈ�� ��Ʈ ť ��Ͽ� �߰��մϴ�.
            noteQueue.Enqueue(NoteSpawnFunc(noteEvent));

            // �ε����� ������Ʈ�մϴ�.
            noteEventIndex++;
        }
    }

    #endregion

    #region ���� ó�� �޼���

    public void ProcessJudge(double currentPlayTime)
    {
        while (noteQueue.TryPeek(out Note note))
        {
            // �Է��� �ִٸ� �Է��� ó���մϴ�.
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
                // ���� �� ��Ʈ�� ó���ϰ� �ִ��� üũ�մϴ�.
                if (IsLongOnProcess)
                {
                    // �� ��Ʈ�� ���� ���� ������ ó���� �����ϰ� ������ �����ϴ�.
                    if (Judger.Instance.CheckLongCleared(currentPlayTime, note))
                    {
                        OnLongCleared();
                    }
                    // ���� ��Ʈ�� ���� ó������ �ʾҴٸ� ������ Ż���մϴ�.
                    else break;
                }
                else
                {
                    if (Judger.Instance.CheckMissed(currentPlayTime, note))
                    {
                        OnMissed();
                    }
                    // ���� ��Ʈ�� Miss���� �ʾҴٸ� �� �̻� ó���� ��Ʈ�� �����Ƿ� ������ Ż���մϴ�.
                    else break;
                }
            }
        }
    }

    // ���� ��Ʈ�� ó��(����)�ϰ� �ѱ�ϴ�.
    private void Pass()
    {
        Note note = noteQueue.Dequeue();

        note.SetWorking(false);

        OnNoteProcess(currentNoteProcessDATA);

        ResetCurrent();
    }

    // Ű�� ��������
    private void OnKeyPressed(double inputTime)
    {
        Note note = noteQueue.Peek();

        if (Judger.Instance.CheckInputTimeValid(inputTime, note))
        {
            currentNoteProcessDATA.Note    = note;
            currentNoteProcessDATA.Type    = NoteProcessType.Judge;
            currentNoteProcessDATA.Time    = inputTime;
            currentNoteProcessDATA.Score   = Judger.Instance.CalculateScore(inputTime, note);

            // �� ��Ʈ�� �ƴ� ��� ������ ��� ó���մϴ�.
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

    // Ű�� ��������
    private void OnKeyReleased(double inputTime)
    {
        Note note = noteQueue.Peek();

        // ���� �� ��Ʈ�� ó���ϰ� �ִٸ� Break ������ �����ϴ�.
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

    // �� ��Ʈ Clear�� �Ǿ�����
    private void OnLongCleared()
    {
        Note note = noteQueue.Peek();

        Pass();

        OnLongProcess(new LongProcessDATA(note, LongProcessType.Clear));
    }

    // ��Ʈ Break�� �Ǿ�����
    private void OnBreaked()
    {
        Note note = noteQueue.Peek();

        currentNoteProcessDATA.Note    = note;
        currentNoteProcessDATA.Type    = NoteProcessType.Break;
        currentNoteProcessDATA.Time    = Judger.Instance.GetMaxBreakedCriteriaTime(note);
        currentNoteProcessDATA.Score   = 0.0f;

        Pass();
    }

    // ��Ʈ Miss�� �Ǿ�����
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
