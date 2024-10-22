using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

[Serializable]
public class ProcessLine
{
    public readonly Sequence<NoteEvent> Sequence;

    private InputQueue InputQueue;
    private NoteQueue NoteQueue;
    private int NoteEventIndex;

    public bool IsLongOnProcess         { get; private set; }
    public float CurrentProcessScore    { get; private set; }

    public delegate void NotePassedEvent(Note note, double time, float score);

    public NotePassedEvent OnNotePassed;
    public Func<NoteEvent, Note> NoteSpawnFunc;

    public ProcessLine(Sequence<NoteEvent> sequence)
    {
        Sequence = sequence;

        InputQueue = new InputQueue();
        NoteQueue = new NoteQueue();
        NoteEventIndex = 0;

        ResetCurrent();
    }

    public void Bind(InputAction inputAction)
    {
        InputQueue.Bind(inputAction);
    }

    public void ResetCurrent()
    {
        CurrentProcessScore = 0f;
        IsLongOnProcess = false;
    }

    internal void Release()
    {
        InputQueue.ReleaseAll();
    }

    // ���� ��Ʈ�� ó��(����)�ϰ� �ѱ�ϴ�.
    private void Pass(Note note, double time)
    {
        note.SetWorking(false);

        OnNotePassed(note, time, CurrentProcessScore);

        NoteQueue.Dequeue();

        ResetCurrent();
    }

    // Ű�� ��������
    private void OnKeyPressed(double inputTime, Note note)
    {
        if (Judger.Instance.CheckInputTimeValid(inputTime, note))
        {
            CurrentProcessScore = Judger.Instance.CalculateScore(inputTime, note);

            // �� ��Ʈ�� ��� ������ �̷�� �÷��׸� ������ ǥ���մϴ�.
            if (note.IsLong)
            {
                IsLongOnProcess = true;
            }
            // �� ��Ʈ�� ��� �ٷ� ������ �����ϴ�.
            else
            {
                Pass(note, inputTime);
            }
        }
    }

    // Ű�� ��������
    private void OnKeyReleased(double inputTime, Note note)
    {
        // ���� �� ��Ʈ�� ó���ϰ� �ִٸ� ������ �����ϴ�.
        if (IsLongOnProcess)
        {
            CurrentProcessScore = 0f;

            Pass(note, inputTime);
        }
    }

    // �� ��Ʈ Clear�� �Ǿ�����
    private void OnLongCleared(Note note)
    {
        Pass(note, Judger.Instance.GetLongClearedCriteriaTime(note));
    }

    // ��Ʈ Miss�� �Ǿ�����
    private void OnMissed(Note note)
    {
        CurrentProcessScore = 0f;

        Pass(note, Judger.Instance.GetMissedCriteriaTime(note));
    }

    public void Process(double spawnTargetBeat, double currentPlayTime)
    {
        void Spawn(double spawnTargetBeat)
        {
            // ��Ʈ ���� ó���Դϴ�.
            while (NoteEventIndex < Sequence.Events.Count)
            {
                // ���� ��Ʈ �ε����� �ش��ϴ� ��Ʈ �ε����� ������ �ɴϴ�.
                NoteEvent noteEvent = Sequence.Events[NoteEventIndex];

                // targetBeat�� �Ѿ�� ��Ʈ �̺�Ʈ�� ó������ �ʽ��ϴ�.
                if (noteEvent.Beat > spawnTargetBeat)
                {
                    break;
                }

                // ������ ��Ʈ�� ��Ʈ ť ��Ͽ� �߰��մϴ�.
                NoteQueue.Enqueue(NoteSpawnFunc(noteEvent));

                // �ε����� ������Ʈ�մϴ�.
                NoteEventIndex++;
            }
        }

        void Judge(double currentPlayTime)
        {
            while (NoteQueue.TryPeek(out Note note))
            {
                // �Է��� �ִٸ� �Է��� ó���մϴ�.
                if (InputQueue.TryDequeue(out InputDATA inputDATA))
                {
                    bool isKeyPressed = inputDATA.Type == InputType.KeyDown;

                    if (isKeyPressed)
                    {
                        OnKeyPressed(inputDATA.Time, note);
                    }
                    else
                    {
                        OnKeyReleased(inputDATA.Time, note);
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
                            OnLongCleared(note);
                        }
                        // ���� ��Ʈ�� ���� ó������ �ʾҴٸ� ������ Ż���մϴ�.
                        else break;
                    }
                    else
                    {
                        if (Judger.Instance.CheckMissed(currentPlayTime, note))
                        {
                            OnMissed(note);
                        }
                        // ���� ��Ʈ�� Miss���� �ʾҴٸ� �� �̻� ó���� ��Ʈ�� �����Ƿ� ������ Ż���մϴ�.
                        else break;
                    }
                }
            }
        }

        Spawn(spawnTargetBeat);
        Judge(currentPlayTime);
    }
}
