using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class Processor : PersistentMonoSingleton<Processor>
{
    public Chart chart;

    private Chart.Indicator indicator;

    [SerializeField, HideInInspector]
    private List<InputQueue> _inputQueueList = new List<InputQueue>();
    public IReadOnlyList<InputQueue> inputQueueList => inputQueueList;

    private void CreateInputQueueList(InputType inputType)
    {
        InputBinding inputBinding = InputManager.Instance.inputBindingList[inputType];

        foreach (var inputAction in inputBinding)
        {
            _inputQueueList.Add(new InputQueue(inputAction));
        }

        foreach (var inputQueue in _inputQueueList)
        {
            inputQueue.SetEnabled(true);
        }
    }

    private void ClearInputQueueList()
    {
        foreach (var inputQueue in _inputQueueList)
        {
            inputQueue.SetEnabled(false);
        }

        _inputQueueList.Clear();
    }

    public delegate void OnNoteProcessed(double delta);

    public OnNoteProcessed onNoteProcessed;

    private void Start()
    {
        Play();
    }

    private void Update()
    {
        if (isPlaying)
        {
            foreach (var inputQueue in _inputQueueList)
            {
                while (inputQueue.Count > 0)
                {
                    double time = inputQueue.Dequeue();
                }
            }
        }
    }

    private bool isPlaying = false;

    public void Play()
    {
        Stop();

        if (chart != null)
        {
            indicator = chart.GetIndicator();

            //CreateInputQueueList(chart.inputType);

            AudioManager.Instance.Load(0, chart.music);

            AudioManager.Instance.Play(channel: 0, sound: 0);

            isPlaying = true;
        }
    }

    public void Stop()
    {
        ClearInputQueueList();

        AudioManager.Instance.Stop(channel: 0);

        AudioManager.Instance.Release(0);

        isPlaying = false;
    }
}
