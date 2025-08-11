/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaverEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Manager.Saver
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.IO;

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

using EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver;

namespace EasyBuildSystem.Features.Editor.Buildings.Manager.Saver
{
    [CustomEditor(typeof(BuildingSaver), true)]
    public class BuildingSaverEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingSaver Target
        {
            get
            {
                return ((BuildingSaver)target);
            }
        }

        bool m_GeneralFoldout = true;

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Saver", "This component allows you to save and load the locations of the Building Parts present in the scene.\n" +
                "You can find more information on the Building Saver component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                string loadPath = Target.GetSavePath;

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextField("Scene Identifier", Target.GetSavePath);
                GUI.enabled = true;
                GUI.enabled = File.Exists(loadPath);
                if (GUILayout.Button("Open File...", GUILayout.Width(100)))
                {
                    Application.OpenURL(loadPath);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LoadAsync"), new GUIContent("Use Load Async",
                    "Enables asynchronous loading of Building Parts, preventing freezing of the main thread during loading. Recommended for improved user experience."));

                if (serializedObject.FindProperty("m_LoadAsync").boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.HelpBox("The higher the value, the more the loading time will be reduced, but this may lead to an increase in frame rate drops.\n" +
                        "Conversely, the lower the value, the longer the loading time, but the impact on frame rate drops will be reduced.", MessageType.Info);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LoadAsyncLimit"), new GUIContent("Async Frame Rate Limit",
                        "Limit frame rate per seconds (1 frame per 0.0167 seconds)."));

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAutoSaver"), new GUIContent("Use Auto Saver",
                    "Enables automatic saving of all Building Parts at specified intervals. Recommended to avoid data loss or corruption in case of crashes."));

                if (serializedObject.FindProperty("m_UseAutoSaver").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoSaverInterval"), new GUIContent("Auto Saver Interval",
                        "The frequency at which the auto save operation occurs (in seconds)."));
                }

                GUI.enabled = File.Exists(loadPath);

                if (GUILayout.Button("Load Saving File..."))
                {
                    Target.ForceLoad(loadPath);
                }

                if (GUILayout.Button("Delete Saving File..."))
                {
                    if (EditorUtility.DisplayDialog("Easy Build System - Delete Saving File...",
                        "This action deletes the saving file that may contain your Building Parts data from the current scene. Do you want to continue?", "Yes", "Cancel"))
                    {
                        Target.ForceDelete(loadPath);
                    }
                }

                GUI.enabled = true;
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