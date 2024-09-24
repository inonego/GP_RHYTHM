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

    // 현재 노트를 처리(판정)하고 넘깁니다.
    private void Pass(Note note, double time)
    {
        note.SetWorking(false);

        OnNotePassed(note, time, CurrentProcessScore);

        NoteQueue.Dequeue();

        ResetCurrent();
    }

    // 키를 눌렀을때
    private void OnKeyPressed(double inputTime, Note note)
    {
        if (Judger.Instance.CheckInputTimeValid(inputTime, note))
        {
            CurrentProcessScore = Judger.Instance.CalculateScore(inputTime, note);

            // 롱 노트인 경우 판정을 미루고 플래그만 참으로 표시합니다.
            if (note.IsLong)
            {
                IsLongOnProcess = true;
            }
            // 단 노트인 경우 바로 판정을 내립니다.
            else
            {
                Pass(note, inputTime);
            }
        }
    }

    // 키를 떼었을때
    private void OnKeyReleased(double inputTime, Note note)
    {
        // 현재 롱 노트를 처리하고 있다면 판정을 내립니다.
        if (IsLongOnProcess)
        {
            CurrentProcessScore = 0f;

            Pass(note, inputTime);
        }
    }

    // 롱 노트 Clear가 되었을때
    private void OnLongCleared(Note note)
    {
        Pass(note, Judger.Instance.GetLongClearedCriteriaTime(note));
    }

    // 노트 Miss가 되었을때
    private void OnMissed(Note note)
    {
        CurrentProcessScore = 0f;

        Pass(note, Judger.Instance.GetMissedCriteriaTime(note));
    }

    public void Process(double spawnTargetBeat, double currentPlayTime)
    {
        void Spawn(double spawnTargetBeat)
        {
            // 노트 스폰 처리입니다.
            while (NoteEventIndex < Sequence.Events.Count)
            {
                // 현재 노트 인덱스에 해당하는 노트 인덱스를 가지고 옵니다.
                NoteEvent noteEvent = Sequence.Events[NoteEventIndex];

                // targetBeat를 넘어서는 노트 이벤트는 처리하지 않습니다.
                if (noteEvent.Beat > spawnTargetBeat)
                {
                    break;
                }

                // 스폰된 노트를 노트 큐 목록에 추가합니다.
                NoteQueue.Enqueue(NoteSpawnFunc(noteEvent));

                // 인덱스를 업데이트합니다.
                NoteEventIndex++;
            }
        }

        void Judge(double currentPlayTime)
        {
            while (NoteQueue.TryPeek(out Note note))
            {
                // 입력이 있다면 입력을 처리합니다.
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
                    // 현재 롱 노트를 처리하고 있는지 체크합니다.
                    if (IsLongOnProcess)
                    {
                        // 롱 노트가 판정 선을 지나면 처리를 중지하고 판정을 내립니다.
                        if (Judger.Instance.CheckLongCleared(currentPlayTime, note))
                        {
                            OnLongCleared(note);
                        }
                        // 현재 노트가 아직 처리되지 않았다면 루프를 탈출합니다.
                        else break;
                    }
                    else
                    {
                        if (Judger.Instance.CheckMissed(currentPlayTime, note))
                        {
                            OnMissed(note);
                        }
                        // 현재 노트가 Miss되지 않았다면 더 이상 처리할 노트가 없으므로 루프를 탈출합니다.
                        else break;
                    }
                }
            }
        }

        Spawn(spawnTargetBeat);
        Judge(currentPlayTime);
    }
}
