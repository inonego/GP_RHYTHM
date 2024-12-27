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
    KeyPressed, KeyReleased
}

[Serializable]
public class InputDATAQueue : Queue<InputDATA?>
{
    [SerializeField, HideInInspector]
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
        InputDATA inputDATA = new InputDATA { InputAction = context.action, Type = InputType.KeyPressed, Time = AudioManager.Instance.GetMusicCurrentPlayTime() };

        Enqueue(inputDATA);
    }

    private void OnKeyUp(InputAction.CallbackContext context)
    {
        InputDATA inputDATA = new InputDATA { InputAction = context.action, Type = InputType.KeyReleased, Time = AudioManager.Instance.GetMusicCurrentPlayTime() };

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
    [field: SerializeField]
    public SerializedDictionary<InputBindingType, InputBinding> InputBindingList { get; private set; } = new SerializedDictionary<InputBindingType, InputBinding>();
}
