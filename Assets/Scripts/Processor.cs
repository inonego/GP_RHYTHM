using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Processor : MonoBehaviour
{
    private enum InputType
    {
        KeyDown, KeyUp
    }

    [Serializable]
    private class InputQueue
    {
        public InputActionReference inputAction;

        public Queue<(InputType type, double time)> queue = new Queue<(InputType, double)>();

        internal void OnEnable()
        {
            inputAction.action.performed += OnKeyDown;
            inputAction.action.canceled += OnKeyUp;
        }
        internal void OnDisable()
        {
            inputAction.action.performed -= OnKeyDown;
            inputAction.action.canceled -= OnKeyUp;
        }

        private void OnKeyDown(InputAction.CallbackContext context)
        {

        }

        private void OnKeyUp(InputAction.CallbackContext context)
        {

        }
    }
}