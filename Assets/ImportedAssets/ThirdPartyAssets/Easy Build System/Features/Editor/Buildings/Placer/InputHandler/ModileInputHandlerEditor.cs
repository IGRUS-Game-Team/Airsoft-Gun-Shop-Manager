/// <summary>
/// Project : Easy Build System
/// Class : ModileInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

namespace EasyBuildSystem.Features.Editor.Buildings.Placer.InputHandler
{
    [CustomEditor(typeof(MobileInputHandler), true)]
    public class ModileInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}