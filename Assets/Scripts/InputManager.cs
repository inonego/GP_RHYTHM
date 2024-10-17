using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputBindingType
{
    KEY4 = 4
}

public enum InputType
{
    KeyUp, KeyDown
}

public struct InputDATA
{
    public InputAction InputAction;
    public InputType Type;
    public double Time;

    public InputDATA(InputAction inputAction, InputType type, double time)
    {
        InputAction = inputAction;
        Type = type;
        Time = time;
    }
}

[Serializable]
public class InputQueue : Queue<InputDATA>
{
    private List<InputAction> inputActionList = new List<InputAction>();
    public IReadOnlyList<InputAction> InputActionList => inputActionList;

    public void Bind(InputAction inputAction)
    {
        inputActionList.Add(inputAction);

        inputAction.performed += OnKeyDown;
        inputAction.canceled += OnKeyUp;
    }

    internal void ReleaseAll()
    {
        foreach (var inputAction in inputActionList)
        {
            inputAction.performed -= OnKeyDown;
            inputAction.canceled -= OnKeyUp;
        }

        inputActionList.Clear();
    }

    private void OnKeyDown(InputAction.CallbackContext context)
    {
        InputDATA inputDATA = new InputDATA(context.action, InputType.KeyDown, AudioManager.Instance.GetCurrentPlayTime(channel: 0));

        Enqueue(inputDATA);
    }

    private void OnKeyUp(InputAction.CallbackContext context)
    {
        InputDATA inputDATA = new InputDATA(context.action, InputType.KeyUp, AudioManager.Instance.GetCurrentPlayTime(channel: 0));

        Enqueue(inputDATA);
    }
}

[Serializable]
public class InputBinding {

    [field: SerializeField]
    public List<InputActionReference> InputActionList { get; private set; } = new List<InputActionReference>();
}

public class InputManager : MonoSingleton<InputManager>
{
    /// <summary>
    /// 키 입력 수에 따라 다른 입력 바인딩을 지정합니다.
    /// </summary>
    [field: SerializeField]
    public SerializedDictionary<InputBindingType, InputBinding> InputBindingList { get; private set; } = new SerializedDictionary<InputBindingType, InputBinding>();
}
