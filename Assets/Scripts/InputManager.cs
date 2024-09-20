using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType
{
    KEY4 = 4
}

[Serializable]
public class InputQueue : Queue<double>
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
        Enqueue(+AudioManager.Instance.GetCurrentPlayTime(0));
    }

    private void OnKeyUp(InputAction.CallbackContext context)
    {
        Enqueue(-AudioManager.Instance.GetCurrentPlayTime(0));
    }
}

[Serializable]
public class InputBinding : List<InputActionReference> { }

public class InputManager : PersistentMonoSingleton<InputManager>
{
    /// <summary>
    /// Ű �Է� ���� ���� �ٸ� �Է� ���ε��� �����մϴ�.
    /// </summary>
    [field: SerializeField]
    public SerializedDictionary<InputType, InputBinding> inputBindingList { get; private set; } = new SerializedDictionary<InputType, InputBinding>();
}
