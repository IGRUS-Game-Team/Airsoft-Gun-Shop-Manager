/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Group
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Group;

namespace EasyBuildSystem.Features.Editor.Buildings.Group
{
    [CustomEditor(typeof(BuildingGroup))]
    public class BuildingGroupEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingGroup Target
        {
            get
            {
                return ((BuildingGroup)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.TextField(new GUIContent("Group Identifier", "A unique identifier for the Building Group."), Target.Identifier);
            GUI.enabled = true;

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}