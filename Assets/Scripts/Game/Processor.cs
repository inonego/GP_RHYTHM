using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Processor : MonoSingleton<Processor>
{
    #region �ʵ� ����

    public CurrentProcess Current { get; private set; }

    public double ProcessBeatDuration = 16.0;

    public ProcessEvent OnProcessEnded;

    public KeyEvent OnKeyEvent;
    public NoteProcessEvent OnNoteProcess;
    public LongProcessEvent OnLongProcess;
    public NoteSpawnDelegate NoteSpawnFunc;

    #endregion

    #region ����Ƽ �̺�Ʈ �޼���

    private void Update()
    {
        Process();
    }

    #endregion

    #region ��� �� ����

    public void Play(Chart chart)
    {
        Stop();

        Release();

        Current = new CurrentProcess(chart);

        Current.OnProcessEnded  += OnProcessEnded;

        Current.OnKeyEvent      += OnKeyEvent;
        Current.OnNoteProcess   += OnNoteProcess;
        Current.OnLongProcess   += OnLongProcess;
        Current.NoteSpawnFunc    = NoteSpawnFunc;

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

    #region ä�� ó�� �޼���

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
