/// <summary>
/// Project : Easy Build System
/// Class : BuildingAreaEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Area
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;
using EasyBuildSystem.Features.Editor.Extensions.ReorderableList;

using EasyBuildSystem.Features.Runtime.Buildings.Area;

namespace EasyBuildSystem.Features.Editor.Buildings.Area
{
    [CustomEditor(typeof(BuildingArea))]
    public class BuildingAreaEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_GeneralFoldout = true;
        bool m_RestrictionFoldout;

        ReorderableList m_PlacingSpecificBuildingParts;
        ReorderableList m_EditingSpecificBuildingParts;
        ReorderableList m_DestroyingSpecificBuildingParts;
        
        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_PlacingSpecificBuildingParts = new ReorderableList(serializedObject.FindProperty("m_CanPlacingSpecificBuildingParts"), false);
            m_EditingSpecificBuildingParts = new ReorderableList(serializedObject.FindProperty("m_CanEditingSpecificBuildingParts"), false);
            m_DestroyingSpecificBuildingParts = new ReorderableList(serializedObject.FindProperty("m_CanDestroyingSpecificBuildingParts"), false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Area",
                "This component limits the building actions such as placement, destruction, and editing within the defined area.\n" +
                "You can find more information on the Building Area component in the documentation.");

            if (((BuildingArea)target).transform.localScale != Vector3.one)
            {
                EditorGUILayout.HelpBox("The scale of the transform to which the component is attached is not equal to one.\n" +
                                        "The building area scale will be based on the transform's scale as well as the radius / bounds of the area.", MessageType.Warning);
                EditorGUILayout.Separator();
            }

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Priority"), new GUIContent("Area Priority", "The higher the value, the more it will be taken into account." +
                    "This can be useful if you have overlapping areas, as it allows the one with the highest priority to be considered."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Shape"), new GUIContent("Area Shape", "The shape of the area (sphere, bounds)."));

                if (serializedObject.FindProperty("m_Shape").enumValueIndex == (int)BuildingArea.ShapeType.BOUNDS)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Bounds"),
                        new GUIContent("Area Bounds", "The bounds of the area."));
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Radius"),
                        new GUIContent("Area Radius", "The radius of the area."));
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_RestrictionFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Restrictions Settings"), m_RestrictionFoldout);

            if (m_RestrictionFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlacingAnyBuildingParts"), 
                    new GUIContent("Allow Placing Any Building Parts", "Enable to allow placement of any building parts in the area."));

                if (serializedObject.FindProperty("m_CanPlacingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.Separator();

                    if (m_PlacingSpecificBuildingParts != null)
                    {
                        m_PlacingSpecificBuildingParts.Layout();
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanEditingAnyBuildingParts"), 
                    new GUIContent("Allow Editing Any Building Parts", "Enable to allow editing of any building parts in the area."));

                if (serializedObject.FindProperty("m_CanEditingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.Separator();

                    if (m_EditingSpecificBuildingParts != null)
                    {
                        m_EditingSpecificBuildingParts.Layout();
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanDestroyingAnyBuildingParts"), 
                    new GUIContent("Allow Destroying Any Building Parts", "Enable to allow destruction of any building parts in the area."));

                if (serializedObject.FindProperty("m_CanDestroyingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.Separator();

                    if (m_DestroyingSpecificBuildingParts != null)
                    {
                        m_DestroyingSpecificBuildingParts.Layout();
                    }
                }
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