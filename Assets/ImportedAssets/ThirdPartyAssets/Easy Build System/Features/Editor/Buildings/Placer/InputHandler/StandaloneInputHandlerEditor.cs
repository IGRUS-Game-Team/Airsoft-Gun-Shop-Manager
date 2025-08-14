/// <summary>
/// Project : Easy Build System
/// Class : StandaloneInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

namespace EasyBuildSystem.Features.Editor.Buildings.Placer.InputHandler
{
    [CustomEditor(typeof(StandaloneInputHandler), true)]
    public class StandaloneInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();

#if EBS_REWIRED
            EditorGUILayout.HelpBox("The integration with Rewired is enabled on this project.\n" +
                "You can now directly edit the inputs in the Rewired Input Manager within the scene.", MessageType.Info);
            EditorGUILayout.Separator();
#endif

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_BlockWhenCursorOverUI"),
                new GUIContent("Block When Pointer Over UI", "Prevents action keys from being used when the cursor is over a UI element."));

#if ENABLE_INPUT_SYSTEM
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ValidateInputReference"),
                new GUIContent("Validate Input Reference", "Input reference used to validate the current action."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CancelInputReference"),
                new GUIContent("Cancel Input Reference", "Input reference used to cancel the current action."));
#elif EBS_REWIRED
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ValidateActionName"),
                new GUIContent("Validate Action Name", "The action key used to validate the current action."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CancelActionName"),
                new GUIContent("Cancel Action Name", "The action key used to cancel the current action."));
#else
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ValidateActionKey"),
                new GUIContent("Validate Action Key", "The action key used to validate the current action."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CancelActionKey"),
                new GUIContent("Cancel Action Key", "The action key used to cancel the current action."));
#endif

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanRotateBuildingPart"),
                new GUIContent("Can Rotate Buildings", "Enables rotation of the preview using the mouse wheel."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanRotateBuildingPart").boolValue)
            {
#if ENABLE_INPUT_SYSTEM
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_RotateModeInputReference"),
                    new GUIContent("Rotate Input Reference", "Input reference used to rotate the preview."));
                EditorGUI.indentLevel--;
#elif EBS_REWIRED
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_RotateActionName"),
                    new GUIContent("Rotate Action Name", "Input reference used to rotate the preview."));
                EditorGUI.indentLevel--;
#else
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_RotateActionKey"),
                    new GUIContent("Rotate Key Shortcut", "The action key for rotate the preview."));
                EditorGUI.indentLevel--;
#endif
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanSelectBuildingPart"),
                new GUIContent("Can Select Buildings", "Enables the selection of building parts."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanSelectBuildingPart").boolValue)
            {
#if ENABLE_INPUT_SYSTEM
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_SelectInputReference"),
                    new GUIContent("Select Input Reference", "Input reference used to select the next building parts."));
                EditorGUI.indentLevel--;
#elif EBS_REWIRED
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_SelectActionName"),
                    new GUIContent("Select Action Name", "The action key for rotate the preview."));
                EditorGUI.indentLevel--;
#endif
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanSelectBuildingPartEvenPlacement"),
                new GUIContent("Can Select Buildings Even Placement", "Enables the selection of building parts even in placement mode."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UsePlacingModeShortcut"),
                new GUIContent("Use Place Mode Shortcut", "Uses an action key to select the Placing build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UsePlacingModeShortcut").boolValue)
            {
                EditorGUI.indentLevel++;
#if ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_PlacingModeInputReference"),
                    new GUIContent("Place Mode Input Reference", "Input reference used to active the placing mode."));
#elif EBS_REWIRED
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_PlaceModeActionName"),
                    new GUIContent("Place Mode Action Name", "The action key for placing the preview."));
#else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_PlacingModeKey"),
                    new GUIContent("Place Mode Key Shortcut", "The action key for placing the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterPlacing"),
                    new GUIContent("Reset Mode After Placing", "Resets the build mode to NONE after placing."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseEditingModeShortcut"),
                new GUIContent("Use Edit Mode Shortcut", "Uses an action key to select the Editing build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseEditingModeShortcut").boolValue)
            {
                EditorGUI.indentLevel++;
#if ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_EditingModeInputReference"),
                    new GUIContent("Edit Mode Input Reference", "Input reference used to active the editing mode."));
#elif EBS_REWIRED
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_EditModeActionName"),
                    new GUIContent("Edit Mode Action Name", "The action key for editing the preview."));
#else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_EditingModeKey"),
                    new GUIContent("Edit Mode Key Shortcut", "The action key for editing the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterEditing"),
                    new GUIContent("Reset Mode After Editing", "Resets the build mode to NONE after editing."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseDestroyingModeShortcut"),
                new GUIContent("Use Destruction Mode Shortcut", "Uses an action key to select the Destroy build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseDestroyingModeShortcut").boolValue)
            {
                EditorGUI.indentLevel++;
#if ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_DestroyingModeInputReference"),
                    new GUIContent("Destruction Mode Input Reference", "Input reference used to active the destruction mode."));
#elif EBS_REWIRED
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_DestructionModeActionName"),
                    new GUIContent("Destruction Mode Action Name", "The action key for destroying the preview."));
#else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_DestroyingModeKey"),
                    new GUIContent("Destruction Mode Key Shortcut", "The action key for destroying the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterDestroying"),
                    new GUIContent("Reset Mode After Destruction", "Resets the build mode to NONE after destroying."));
                EditorGUI.indentLevel--;
            }


            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}