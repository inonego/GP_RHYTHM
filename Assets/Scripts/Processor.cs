using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Processor : MonoBehaviour
{
    public Chart chart;

    private enum InputType
    {
        KeyDown, KeyUp
    }

    [Serializable]
    private class InputQueue : Queue<(InputType type, double time)>
    {
        public readonly InputActionReference inputAction;

        public InputQueue(InputActionReference inputAction)
        {
            this.inputAction = inputAction;
        }

        internal void SetEnabled(bool value)
        {
            Clear();

            if (value)
            {
                inputAction.action.performed += OnKeyDown;
                inputAction.action.canceled += OnKeyUp;
            }
            else
            {
                inputAction.action.performed -= OnKeyDown;
                inputAction.action.canceled -= OnKeyUp;
            }
        }

        private void OnKeyDown(InputAction.CallbackContext context)
        {
            Enqueue((InputType.KeyDown, AudioManager.Instance.GetCurrentPlayTime(0)));
        }

        private void OnKeyUp(InputAction.CallbackContext context)
        {
            Enqueue((InputType.KeyUp, AudioManager.Instance.GetCurrentPlayTime(0)));
        }
    }

    private Chart.Indicator indicator;

    private List<InputQueue> inputQueues = new List<InputQueue>();

    public delegate void OnNoteProcessed(double delta);

    public OnNoteProcessed onNoteProcessed;
    
    private void Update()
    {
        foreach (var inputQueue in inputQueues)
        {
            while (inputQueue.Count > 0)
            {
                inputQueue.Dequeue();
            }
        }
    }

    private bool isPlaying = false;

    public void Play()
    {
        Stop();

        // Ãß°¡

        foreach (var inputQueue in inputQueues)
        {
            inputQueue.SetEnabled(true);
        }
    }
    public void Stop()
    {
        if (isPlaying)
        {
            foreach (var inputQueue in inputQueues)
            {
                inputQueue.SetEnabled(false);
            }
        }
    }
}
