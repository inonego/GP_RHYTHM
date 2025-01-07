using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine.InputSystem;

public enum KeyState
{
    Pressed, Released
}

[Serializable]
public struct InputDATA
{
    public KeyState State;
    public double Time;
}

[Serializable]
public class InputBinding
{
    public List<InputActionReference> InputActionList = new List<InputActionReference>();
}

[Serializable]
public class InputDATAQueue : Queue<InputDATA?>
{
    public InputAction InputAction { get; private set; } = null;

    public void Bind(InputAction inputAction)
    {
        InputAction = inputAction;

        InputAction.performed += OnKeyPressed;
        InputAction.canceled  += OnKeyReleased;
    }

    public void Release()
    {
        InputAction = null;

        InputAction.performed -= OnKeyPressed;
        InputAction.canceled  -= OnKeyReleased;
    }

    private void OnKeyPressed(InputAction.CallbackContext context)
    {
        InputDATA inputDATA = new InputDATA { State = KeyState.Pressed, Time = AudioManager.Instance.GetMusicCurrentPlayTime() };

        Enqueue(inputDATA);
    }

    private void OnKeyReleased(InputAction.CallbackContext context)
    {
        InputDATA inputDATA = new InputDATA { State = KeyState.Released, Time = AudioManager.Instance.GetMusicCurrentPlayTime() };

        Enqueue(inputDATA);
    }
}