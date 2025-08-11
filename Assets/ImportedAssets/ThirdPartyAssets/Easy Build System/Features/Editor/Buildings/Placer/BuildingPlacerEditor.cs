/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacerEditor.cs
/// Namespace : EasyBuildSystem.Editor.Runtime.Buildings.Placer
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

namespace EasyBuildSystem.Editor.Runtime.Buildings.Placer
{
    [CustomEditor(typeof(BuildingPlacer), true)]
    public class BuildingPlacerEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_InputFoldout;
        bool m_RaycastFoldout;
        bool m_SnappingFoldout;
        bool m_AudioFoldout;

        List<Type> m_InputHandlers = new List<Type>();
        string[] m_Handlers;
        int m_InputHandlerIndex;

        UnityEditor.Editor m_InputHandlerEditor;

        BuildingPlacer Target
        {
            get
            {
                return (BuildingPlacer)target;
            }
        }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            List<Type> types = GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(BaseInputHandler)).ToList();

            m_InputHandlers = types;

            m_Handlers = new string[m_InputHandlers.Count];

            for (int i = 0; i < m_InputHandlers.Count; i++)
            {
                m_Handlers[i] = Regex.Replace(m_InputHandlers[i].Name, "([a-z])([A-Z])", "$1 $2");
            }

            BaseInputHandler[] inputHandlers = Target.GetComponents<BaseInputHandler>();

            if (inputHandlers.Length > 1)
            {
                for (int i = 1; i < inputHandlers.Length; i++)
                {
                    DestroyImmediate(inputHandlers[i]);
                }
            }

            if (Target.GetInputHandler != null)
            {
                m_InputHandlerIndex = m_InputHandlers.IndexOf(Target.GetInputHandler.GetType());
                Target.GetInputHandler.hideFlags = HideFlags.HideInInspector;
            }
            else
            {
                m_InputHandlerIndex = m_InputHandlers.IndexOf(typeof(StandaloneInputHandler));
            }
        }

        public Type[] GetAllDerivedTypes(AppDomain appDomain, Type targetType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = appDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != typeof(BaseInputHandler))
                    {
                        if (type == targetType || type.IsSubclassOf(targetType))
                        {
                            result.Add(type);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Placer",
                "This component handles the inputs and manages the different building modes during the runtime.\n" +
                "You can find more information on the Building Placer component in the documentation.");

            m_InputFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Input Settings"), m_InputFoldout);

            if (m_InputFoldout)
            {
#if !ENABLE_INPUT_SYSTEM
                EditorGUILayout.HelpBox("It is recommended to use the New Input System for more flexibility in handling input changes.\n" +
                    "The new Unity Input System provides additional capabilities and customization options for managing input events.\n" +
                    "You read the documentation to have more information about the New Input System support.", MessageType.Info);
#endif

                EditorGUI.BeginChangeCheck();
                m_InputHandlerIndex = EditorGUILayout.Popup("Current Input Handler", m_InputHandlerIndex, m_Handlers);
                if (EditorGUI.EndChangeCheck())
                {
                    if (Target.GetInputHandler != null)
                    {
                        DestroyImmediate(Target.GetInputHandler);
                    }

                    if (m_InputHandlerEditor != null)
                    {
                        DestroyImmediate(m_InputHandlerEditor);
                        m_InputHandlerEditor = null;
                    }

                    Target.GetInputHandler = (BaseInputHandler)Target.gameObject.AddComponent(m_InputHandlers[m_InputHandlerIndex]);
                    Target.GetInputHandler.hideFlags = HideFlags.HideInInspector;

                    Repaint();

                    m_InputFoldout = true;
                }

                if (m_InputHandlerEditor == null)
                {
                    m_InputHandlerEditor = CreateEditor(Target.GetInputHandler);
                }
                else
                {
                    m_InputHandlerEditor.OnInspectorGUI();
                }

                EditorUtility.SetDirty(target);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_RaycastFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Raycast Settings"), m_RaycastFoldout);

            if (m_RaycastFoldout)
            {
                if (serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Camera").objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("This component requires a camera to function properly. Please make sure you have a camera assigned.\n" +
                        "", MessageType.Warning);

                    GUI.color = Color.yellow;
                }
                else
                {
                    GUI.color = Color.white;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Camera"),
                    new GUIContent("Raycast Camera", "The camera used for raycasting."));

                GUI.color = Color.white;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_ViewType"),
                    new GUIContent("Raycast View Type", "The type of view for raycasting.\n" +
                    "- First Person View: The ray originates from the center of the camera and goes forward.\n" +
                    "- Third Person View: The ray originates from a custom transform and goes forward.\n" +
                    "- Top Down View: The ray originates from the center of the camera and goes towards the mouse position."));

                if (serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_ViewType").enumValueIndex == (int)BuildingPlacer.RaycastSettings.RaycastType.THIRD_PERSON_VIEW)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_FromTransform"),
                        new GUIContent("Raycast From Transform", "The transform from which the raycast will start."));
                    EditorGUI.indentLevel--;
                }

#if EBS_XR
                if (serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_ViewType").enumValueIndex == (int)BuildingPlacer.RaycastSettings.RaycastType.FROM_XR_INTERACTOR)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_RaycastFromXRInteractor"));
                }
#endif

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_LayerMask"),
                    new GUIContent("Raycast Layer Mask", "The layers that will be considered for raycasting."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Distance"),
                    new GUIContent("Raycast Distance", "The maximum distance for the raycast."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_MaxDistance"),
                    new GUIContent("Raycast Max Distance", "The maximum allowed distance for the raycast."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_OffsetPosition"),
                    new GUIContent("Raycast Offset Position", "An offset position from which to cast the ray, useful for adjusting the origin position of the raycast."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Through"),
                    new GUIContent("Raycast Ignore Colliders", "Specifies whether the raycast should pass through colliders."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_SnappingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Snapping Settings"), m_SnappingFoldout);

            if (m_SnappingFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_Type"),
                    new GUIContent("Snapping Type", "The type of snapping detection."));

                if (serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_Type").enumValueIndex == (int)BuildingPlacer.SnappingSettings.DetectionType.OVERLAP)
                {
                    EditorGUI.indentLevel++;
                    serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue =
                        EditorGUILayout.Slider(new GUIContent("Snapping Max Angle", "The maximum angle to detect Building Sockets."),
                            serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue, 0, 360);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_LayerMask"),
                    new GUIContent("Snapping Layer Mask", "The layers to consider for snapping detection."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_AudioFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Audio Settings"), m_AudioFoldout);

            if (m_AudioFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_AudioSource"),
                    new GUIContent("Audio Source", "The audio source used for playing audio clips."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_PlacingAudioClips"),
                    new GUIContent("Placing Audio Clips", "Audio clips played when placing a building part."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_EditingAudioClips"),
                    new GUIContent("Editing Audio Clips", "Audio clips played when editing a building part."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_DestroyAudioClips"),
                    new GUIContent("Destroying Audio Clips", "Audio clips played when destroying a building part."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

#endregion
    }
}