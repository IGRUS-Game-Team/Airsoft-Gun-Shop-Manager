#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

public static class InputReferencerFinder
{
    public static InputActionReference FindReference(string actionName)
    {
        InputActionAsset actionFile = Resources.Load<InputActionAsset>("Default Input Actions");

        if (actionFile == null) return null;

        InputAction action = actionFile.FindAction(actionName);

        if (action == null) return null;

        return InputActionReference.Create(action);
    }
}

#endif