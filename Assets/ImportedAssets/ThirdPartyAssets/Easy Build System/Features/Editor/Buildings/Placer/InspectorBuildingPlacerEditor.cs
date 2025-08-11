/// <summary>
/// Project : Easy Build System
/// Class : InspectorBuildingPlacerEditor.cs
/// Namespace : EasyBuildSystem.Editor.Runtime.Buildings.Placer
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Editor.Extensions;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace EasyBuildSystem.Editor.Runtime.Buildings.Placer
{
    [CustomEditor(typeof(InspectorBuildingPlacer))]
    public class InspectorBuildingPlacerEditor : UnityEditor.Editor
    {
        #region Fields

        static BuildingPlacer m_Builder;
        static BuildingPlacer Builder
        {
            get
            {
                if (m_Builder == null)
                {
                    if (FindObjectOfType<InspectorBuildingPlacer>() != null)
                    {
                        m_Builder = FindObjectOfType<InspectorBuildingPlacer>();
                    }
                    else
                    {
                        m_Builder = new GameObject("(Instance) Building Placer Editor").AddComponent<InspectorBuildingPlacer>();
                    }

                    m_Builder.GetRaycastSettings.ViewType = BuildingPlacer.RaycastSettings.RaycastType.TOP_DOWN_VIEW;
                    m_Builder.GetRaycastSettings.Distance = 100f;

                    m_Builder.GetSnappingSettings.MaxAngles = 5f;
                }

                return m_Builder;
            }
        }

        int m_BuildingSelectionIndex = 0;
        string[] m_BuildingCategory;

        bool m_RaycastFoldout = true;
        bool m_SnappingFoldout = true;

        Vector2 m_ScrollPosition;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            List<string> category = new List<string>();

            for (int i = 0; i < BuildingManager.Instance.BuildingPartReferences.Count; i++)
            {
                BuildingPart partReference = BuildingManager.Instance.BuildingPartReferences[i];

                if (partReference != null)
                {
                    if (!category.Contains(partReference.GetGeneralSettings.Type))
                    {
                        category.Add(partReference.GetGeneralSettings.Type);
                    }
                }
            }

            m_BuildingCategory = category.ToArray();

            serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue = 15f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Editor Building Placer",
                "This component manages the different building modes within the Unity Editor scene view.\n" +
                "You can find more information on the Building Placer Editor component in the documentation.");

            EditorGUIUtilityExtension.BeginVertical();

            GUILayout.BeginHorizontal("toolbar");

            m_BuildingSelectionIndex = GUILayout.Toolbar(m_BuildingSelectionIndex, m_BuildingCategory, "toolbarButton");

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (Builder.GetSelectedBuildingPart != null)
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                GUILayout.Button(Builder.GetSelectedBuildingPart.GetGeneralSettings.Thumbnail != null ?
                            new GUIContent(Builder.GetSelectedBuildingPart.GetGeneralSettings.Thumbnail) : EditorGUIUtility.IconContent("d__Help@2x"),
                                GUILayout.Width(87), GUILayout.Height(87));

                GUILayout.BeginVertical();

                EditorGUILayout.Separator();

                GUI.enabled = false;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Identifier :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Identifier =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Identifier);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Name :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Name =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Name);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Type :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Type =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Type);
                GUILayout.EndHorizontal();

                GUI.enabled = true;

                if (GUILayout.Button("Edit Building Settings..."))
                {
                    EditorGUIUtility.PingObject(Builder.GetSelectedBuildingPart.gameObject);
                    Selection.activeObject = Builder.GetSelectedBuildingPart.gameObject;
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, false, true, GUILayout.Height(70));

            if (BuildingManager.Instance.BuildingPartReferences.Count == 0)
            {
                GUILayout.Label("No references are included in the Building Manager.", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < BuildingManager.Instance.BuildingPartReferences.Count; i++)
                {
                    if (BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Type == m_BuildingCategory[m_BuildingSelectionIndex])
                    {
                        if (GUILayout.Button(BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Thumbnail != null ?
                            new GUIContent(BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Thumbnail) : EditorGUIUtility.IconContent("d__Help@2x"),
                            GUILayout.Width(60), GUILayout.Height(60)))
                        {
                            Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                            Builder.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                            Builder.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[i]);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (Builder.GetBuildMode != BuildingPlacer.BuildMode.NONE)
            {
                GUILayout.Label("Shortcuts : Left Mouse = Validate | R = Rotate Preview", EditorStyles.centeredGreyMiniLabel);

                GUILayout.Space(5f);
            }

            GUILayout.BeginHorizontal();

            if (Builder.GetBuildMode != BuildingPlacer.BuildMode.NONE)
            {
                GUI.color = Color.red / 2f + Color.white / 1.3f;
                if (GUILayout.Button("Exit " + Builder.GetBuildMode.ToString() + " Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("Placement Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }

                if (GUILayout.Button("Destruction Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }

                if (GUILayout.Button("Editing Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            GUILayout.EndHorizontal();

            EditorGUIUtilityExtension.EndVertical();

            EditorGUILayout.Separator();

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

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_LayerMask"),
                    new GUIContent("Raycast Layers", "The layers that will be considered for raycasting."));
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

            EditorGUILayout.Separator();

            if (GUILayout.Button("Close Editor Building Placer..."))
            {
                Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                DestroyImmediate(((InspectorBuildingPlacer)target).gameObject);
                return;
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}