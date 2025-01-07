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

public class InputManager : MonoSingleton<InputManager>
{
    [field: SerializeField]
    public SerializedDictionary<InputBindingType, InputBinding> InputBindingList { get; private set; } = new SerializedDictionary<InputBindingType, InputBinding>();
}
