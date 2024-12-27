using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Processor : MonoSingleton<Processor>
{
    #region 필드 변수

    public CurrentProcess Current { get; private set; }

    public double ProcessBeatDuration = 16.0;

    public ProcessEvent OnProcessEnded;

    public event InputProcessEvent OnInputProcess;
    public event NoteProcessEvent OnNoteProcess;
    public SpawnFunc NoteSpawnFunc;

    #endregion

    #region 유니티 이벤트 메서드

    private void Update()
    {
        Process();
    }

    #endregion

    #region 재생 및 중지

    public void Play(Chart chart)
    {
        Stop();

        Release();

        Current = new CurrentProcess(chart);

        Current.OnProcessEnded  += OnProcessEnded;

        Current.OnInputProcess   += OnInputProcess;
        Current.OnNoteProcess    += OnNoteProcess;
        Current.NoteSpawnFunc     = NoteSpawnFunc;

        Current.Play();

        NoteManager.Instance.SetInputType(chart.InputType);
    }

    public void Stop()
    {
        if (Current != null)
        {
            Current.Stop();
        }
    }

    public void Release()
    {
        if (Current != null)
        {
            Current.Release();
        }

        Current = null;

        NoteManager.Instance.DespawnAll();
    }

    #endregion

    #region 채보 처리 메서드

    private void Process()
    {
        if (Current != null)
        {
            Current.Process(ProcessBeatDuration);

            NoteManager.Instance.SetCurrentPlayTime(Current.CurrentPlayTime);
            NoteManager.Instance.SetCurrentPosition(Current.CurrentPosition);
        }
    }

    #endregion
}
