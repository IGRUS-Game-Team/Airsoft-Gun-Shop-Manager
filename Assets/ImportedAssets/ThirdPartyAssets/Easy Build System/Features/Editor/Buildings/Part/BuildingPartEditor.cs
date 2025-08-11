/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingPart), true)]
    public class BuildingPartEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingPart Target
        {
            get
            {
                return ((BuildingPart)target);
            }
        }

        static bool m_GeneralFoldout = true;
        static bool m_ModelFoldout;
        static bool m_PreviewFoldout;
        static bool m_ConditionFoldout;

        UnityEditor.Editor m_ModelEditor = null;

        static readonly bool[] m_ConditionsFoldout = new bool[999];

        List<BuildingConditionAttribute> m_BuildingConditions = new List<BuildingConditionAttribute>();

        readonly Dictionary<int, UnityEditor.Editor> m_CachedEditors = new Dictionary<int, UnityEditor.Editor>();

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_BuildingConditions = LoadBuildingConditions();

            for (int i = 0; i < m_BuildingConditions.Count; i++)
            {
                if (Target.GetComponent(m_BuildingConditions[i].Type) != null)
                {
                    Target.GetComponent(m_BuildingConditions[i].Type).hideFlags = HideFlags.HideInInspector;
                }
            }
        }

        void OnDisable()
        {
            Target.HidePreviewIndicator();

            if (m_ModelEditor != null)
            {
                DestroyImmediate(m_ModelEditor);
            }

            m_CachedEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Part",
                "This component contains all the data related to building, including the model, preview, and conditions settings.\n" +
                "You can find more information on the Building Part component in the documentation.");

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Changes made to this component during runtime will not be saved.\n" +
                    "To apply changes, click the button below. This will apply the changes to all buildings with the same identifier.", MessageType.Warning);

                if (GUILayout.Button("Apply All Changes..."))
                {
                    BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(Target.GetGeneralSettings.Identifier);

                    if (buildingPart != null)
                    {
                        EditorUtility.CopySerialized(Target, buildingPart);

                        for (int i = 0; i < Target.Conditions.Count; i++)
                        {
                            if (Target.Conditions[i] != null)
                            {
                                EditorUtility.CopySerialized(Target.Conditions[i], buildingPart.GetComponents<BuildingCondition>()[i]);
                            }
                        }

                        Debug.Log("<b>Easy Build System</b> All changes to the Building Part \"" + buildingPart.GetGeneralSettings.Name + "\" have been applied.");
                    }
                }
            }

            if (HasMissingColliders())
            {
                if (Target.GetModelSettings.Models != null)
                {
                    EditorGUILayout.HelpBox("No colliders have been found in the children's transforms.", MessageType.Warning);

                    if (GUILayout.Button("Add MeshCollider(s)..."))
                    {
                        foreach (Renderer renderer in Target.gameObject.GetComponentsInChildren<Renderer>())
                        {
                            renderer.gameObject.AddComponent<MeshCollider>();

                            Target.Colliders = null;
                            _ = Target.Colliders;

                            EditorUtility.SetDirty(target);

                            Debug.Log("<b>Easy Build System</b> The missing collider(s) have been added to the renderer(s) successfully.");
                        }
                    }
                }
            }

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(new GUIContent("Building Identifier",
                    "Generated UID that allows the system to easily find the Building Part."), Target.GetGeneralSettings.Identifier);
                if (GUILayout.Button("Generate ID", GUILayout.Width(90)))
                {
                    Target.GetGeneralSettings.Identifier = Guid.NewGuid().ToString("N");
                    EditorUtility.SetDirty(target);
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Name"),
                    new GUIContent("Building Name", "The name of the building."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Type"),
                    new GUIContent("Building Type", "The type of the building."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Thumbnail"),
                    new GUIContent("Building Thumbnail", "The thumbnail image for the building."));

                if (GUILayout.Button("Generate Basic Building Thumbnail..."))
                {
                    EditorApplication.delayCall += () =>
                    {
                        GenerateThumbnail();
                    };
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_ModelFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Model Settings"), m_ModelFoldout);

            if (m_ModelFoldout)
            {
                EditorGUILayout.Separator();

                if (Target.GetModelSettings.GetModel != null)
                {
                    if (m_ModelEditor == null)
                    {
                        m_ModelEditor = CreateEditor(Target.GetModelSettings.GetModel);
                    }

                    EditorGUILayout.Separator();

                    m_ModelEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128f, 128f), EditorStyles.whiteLabel);

                    EditorGUILayout.Separator();
                }

                for (int i = 0; i < Target.GetModelSettings.Models.Count; i++)
                {
                    if (Target.GetModelSettings.Models[i] != null)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(1f);

                        GUILayout.Label("Model : " + Target.GetModelSettings.Models[i].name);

                        GUILayout.FlexibleSpace();

                        GUI.enabled = Target.GetModelSettings.ModelIndex != i;

                        if (GUILayout.Button("Set Model As Default...", GUILayout.Width(200)))
                        {
                            Target.GetModelSettings.ModelIndex = i;

                            Target.UpdateModel();

                            DestroyImmediate(m_ModelEditor);

                            EditorUtility.SetDirty(target);
                        }

                        GUI.enabled = true;

                        GUILayout.EndHorizontal();

                        EditorGUILayout.Separator();

                        GUILayout.BeginHorizontal();

                        GUI.enabled = Target.transform != null;
                        if (GUILayout.Button("Edit Model Offset..."))
                        {
                            ModelOffsetEditor.Init(Target.transform, Target);
                        }
                        GUI.enabled = true;

                        GUI.enabled = Target.transform != null;
                        if (GUILayout.Button("Reset Model Offset..."))
                        {
                            Target.GetModelSettings.GetModel.transform.localPosition = Vector3.zero;
                            Target.GetModelSettings.GetModel.transform.localEulerAngles = Vector3.zero;
                            Target.GetModelSettings.GetModel.transform.localScale = Vector3.one;
                        }
                        GUI.enabled = true;

                        if (GUILayout.Button("Exclude Model..."))
                        {
                            Target.GetModelSettings.ModelIndex = i - 1;

                            Target.UpdateModel();

                            Target.GetModelSettings.Models.Remove(Target.GetModelSettings.Models[i]);
                            Repaint();
                        }

                        GUILayout.EndHorizontal();

                        EditorGUILayout.Separator();
                    }
                }

                EditorGUIUtilityExtension.BeginVertical();

                GUI.enabled = Target.gameObject.scene.IsValid();

                Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

                GUI.Box(dropRect, "Drag and Drop your 3D model here to add it...", EditorStyles.centeredGreyMiniLabel);

                if (dropRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        Event.current.Use();
                    }
                    else if (Event.current.type == EventType.DragPerform)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            GameObject draggedObject = DragAndDrop.objectReferences[i] as GameObject;

                            if (draggedObject == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> Cannot add empty gameObject!");
                                return;
                            }

                            ModelChanger.ChangeModel(Target, Target.GetModelSettings.GetModel, draggedObject);

                            DestroyImmediate(m_ModelEditor, true);
                        }

                        Event.current.Use();
                    }
                }

                GUI.enabled = true;

                EditorGUIUtilityExtension.EndVertical();

                EditorGUILayout.Separator();

                GUILayout.Label("Bounds Settings", EditorStyles.boldLabel);

                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ModelSettings").FindPropertyRelative("m_ModelBounds"),
                    new GUIContent("Building Model Bounds", "The bounds of the building model. This is used for placement and collision calculations."));

                GUI.enabled = Target.GetModelSettings.Models != null;
                if (GUILayout.Button("Generate Building Model Bounds..."))
                {
                    Target.UpdateModelBounds();
                    EditorUtility.SetDirty(target);
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_PreviewFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Preview Settings"), m_PreviewFoldout);

            if (m_PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PreviewElevation"),
                    new GUIContent("Preview Elevation", "Enables preview elevation from the ground."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PreviewElevation").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PreviewElevationHeight"),
                        new GUIContent("Preview Elevation Height", "Preview elevation height."));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_UseGridSnapping"),
                    new GUIContent("Preview Grid Snapping", "Enable or disable grid snapping for preview movement."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_UseGridSnapping").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementGridSize"),
                        new GUIContent("Preview Grid Size", "Define the size of each grid cell for snapping the preview movement."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementGridOffset"),
                        new GUIContent("Preview Grid Offset", "Specify the offset for the grid to adjust the starting position of the grid cells."));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_SmoothMovement"),
                    new GUIContent("Preview Smooth Movement", "Enable or disable smooth movement for the preview."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_SmoothMovement").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementSmoothTime"),
                        new GUIContent("Preview Smooth Movement Time", "Specify the duration for smoothing the preview movement."));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_OffsetPosition"),
                    new GUIContent("Preview Offset Position", "The offset position of the preview."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampPosition"),
                    new GUIContent("Preview Clamp Position", "Whether to clamp the position of the preview."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampPosition").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMinPosition"),
                        new GUIContent("Preview Clamp Min Position", "The minimum position to clamp the preview to."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMaxPosition"),
                        new GUIContent("Preview Clamp Max Position", "The maximum position to clamp the preview to."));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_CanMovingIfPlaceable"),
                    new GUIContent("Preview Can Move Only Placeable", "Whether the preview can only move when it is placeable."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampRotation"),
                    new GUIContent("Preview Clamp Rotation", "Whether to clamp the rotation of the preview."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampRotation").boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMinRotation"),
                        new GUIContent("Preview Clamp Min Rotation", "The minimum rotation to clamp the preview to."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMaxRotation"),
                        new GUIContent("Preview Clamp Max Rotation", "The maximum rotation to clamp the preview to."));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAxis"),
                    new GUIContent("Preview Rotate Axis", "The rotation axis on which the preview can be rotated."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAngle"),
                    new GUIContent("Preview Rotate According Angle", "Whether to rotate the preview according to the surface angle."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAngle").boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAxis"),
                        new GUIContent("Preview Rotate According Axis", "The rotation axis according to which the preview can be rotated."));

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_CanRotateOnSocket"),
                    new GUIContent("Preview Can Rotate On Socket", "Whether the preview can rotate when snapped on a Building Socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ResetRotation"),
                    new GUIContent("Preview Reset Rotation", "Reset the current preview rotation after placing."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_LockRotation"),
                    new GUIContent("Preview Lock Rotation", "Lock the rotation of the preview to the camera rotation."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Indicator"),
                    new GUIContent("Preview Indicator", "Whether to show an indicator during the preview state, which can be useful for visualizing the rotation preview."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Indicator").boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").FindPropertyRelative("m_Object"),
                        new GUIContent("Preview Indicator Object", "The object used as the preview indicator."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").FindPropertyRelative("m_Color"),
                        new GUIContent("Preview Indicator Color", "The color of the preview indicator."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").FindPropertyRelative("m_OffsetPosition"),
                        new GUIContent("Preview Indicator Offset Position", "The offset position of the preview indicator."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").FindPropertyRelative("m_OffsetRotation"),
                        new GUIContent("Preview Indicator Offset Rotation", "The offset rotation of the preview indicator."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").FindPropertyRelative("m_OffsetScale"),
                        new GUIContent("Preview Indicator Offset Scale", "The offset scale of the preview indicator."));

                    if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").isExpanded)
                    {
                        GUI.enabled = Target.gameObject.scene.IsValid();

                        if (Target.InstancedIndicatorObject == null)
                        {
                            GUI.color = Color.white;
                            if (GUILayout.Button("Show Preview Indicator"))
                            {
                                Target.ShowPreviewIndicator();
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Cancel Preview Indicator"))
                            {
                                Target.HidePreviewIndicator();
                            }
                            GUI.color = Color.white;
                        }

                        GUI.enabled = true;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Material"),
                    new GUIContent("Preview Material", "The material used for the preview."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Transition"),
                    new GUIContent("Preview Material Transition", "The transition type of the preview material."), true);

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Transition").enumValueIndex == (int)BuildingPart.PreviewSettings.TransitionType.FADE)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_FadingTransition").FindPropertyRelative("m_FadeDuration"),
                        new GUIContent("Preview Material Fading Duration", "The duration of the fading transition for the preview material."), true);
                    EditorGUI.indentLevel--;
                }
                else if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Transition").enumValueIndex == (int)BuildingPart.PreviewSettings.TransitionType.PULSE)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PulseTransition").FindPropertyRelative("m_PulseDuration"),
                        new GUIContent("Preview Material Pulse Duration", "The duration of the pulse transition for the preview material."), true);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PulseTransition").FindPropertyRelative("m_PulseMinAlpha"),
                        new GUIContent("Preview Material Pulse Min Alpha", "The minimum alpha value for the pulse transition of the preview material."), true);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PulseTransition").FindPropertyRelative("m_PulseMaxAlpha"),
                        new GUIContent("Preview Material Pulse Max Alpha", "The maximum alpha value for the pulse transition of the preview material."), true);
                    EditorGUI.indentLevel--;
                }
                else if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Transition").enumValueIndex == (int)BuildingPart.PreviewSettings.TransitionType.FLASH)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_FlashingTransition").FindPropertyRelative("m_FlashDuration"),
                        new GUIContent("Preview Material Flashing Duration", "The duration of the flashing transition for the preview material."), true);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_FlashingTransition").FindPropertyRelative("m_FlashCount"),
                        new GUIContent("Preview Material Flashing Count", "The number of times the preview material should flash."), true);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PlacingColor"),
                    new GUIContent("Preview Placing Color", "The color of the preview material when in the placing state."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_EditingColor"),
                    new GUIContent("Preview Editing Color", "The color of the preview material when in the editing state."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DestroyingColor"),
                    new GUIContent("Preview Destroying Color", "The color of the preview material when in the destroying state."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IgnoreRenderers"),
                    new GUIContent("Preview Ignore Renderers", "Renderers to ignore when applying the preview material."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DisableGameObjects"),
                    new GUIContent("Preview Disable GameObjects", "GameObjects to disable during the preview state."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DisableMonoBehaviours"),
                    new GUIContent("Preview Disable MonoBehaviours", "MonoBehaviours to disable during the preview state."), true);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_ConditionFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Conditions Settings"), m_ConditionFoldout, false);

            if (m_ConditionFoldout)
            {
                GUILayout.Space(3f);

                if (m_BuildingConditions.Count == 0)
                {
                    EditorGUIUtilityExtension.BeginVertical();
                    GUILayout.Space(5f);
                    GUILayout.Label("No conditions were found for this component.", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.Space(5f);
                    EditorGUIUtilityExtension.EndVertical();
                }

                int index = 0;

                foreach (BuildingConditionAttribute condition in m_BuildingConditions)
                {
                    m_ConditionsFoldout[index] = EditorGUIUtilityExtension.BeginFoldout(new GUIContent(condition.Name), m_ConditionsFoldout[index], false, 16);

                    GUILayout.Space(-21);

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();

                    if (Target.GetComponent(condition.Type) != null)
                    {
                        GUILayout.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Copy Settings", GUILayout.Width(120)))
                        {
                            ComponentUtility.CopyComponent(Target.GetComponent(condition.Type));
                        }

                        if (GUILayout.Button("Paste Settings", GUILayout.Width(120)))
                        {
                            ComponentUtility.PasteComponentValues(Target.GetComponent(condition.Type));
                            EditorUtility.SetDirty(target);
                        }

                        if (!condition.Type.Equals(typeof(BuildingBasicsCondition)))
                        {
                            if (GUILayout.Button("Disable Condition", GUILayout.Width(120)))
                            {
                                try
                                {
                                    ((BuildingCondition)Target.gameObject.GetComponent(condition.Type)).DisableCondition();
                                    DestroyImmediate(Target.gameObject.GetComponent(condition.Type), true);
                                    break;
                                }
                                catch { }
                            }
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Enable Condition", GUILayout.Width(120)))
                        {
                            if (Target.GetComponent(condition.Type) != null)
                            {
                                return;
                            }

                            Component component = Target.gameObject.AddComponent(condition.Type);
                            component.hideFlags = HideFlags.HideInInspector;

                            ((BuildingCondition)component).EnableCondition();

                            m_ConditionsFoldout[index] = true;
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();

                    if (m_ConditionsFoldout[index])
                    {
                        GUILayout.BeginHorizontal();

                        if (Target.GetComponent(condition.Type) != null)
                        {
                            GUILayout.BeginVertical();

                            if (Selection.gameObjects.Length > 1)
                            {
                                EditorGUILayout.HelpBox("Multiple-editing not yet supported.", MessageType.Warning);
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(15f);
                                GUI.enabled = false;
                                GUILayout.Label(condition.Description, EditorStyles.miniLabel);
                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                                EditorGUILayout.Space();

                                Component component = Target.GetComponent(condition.Type);

                                if (m_CachedEditors.ContainsKey(component.GetInstanceID()))
                                {
                                    m_CachedEditors.TryGetValue(component.GetInstanceID(), out UnityEditor.Editor editor);

                                    EditorGUI.indentLevel++;
                                    editor.OnInspectorGUI();
                                    EditorGUI.indentLevel--;
                                }
                                else
                                {
                                    UnityEditor.Editor conditionEditor = CreateEditor(Target.GetComponent(condition.Type));
                                    m_CachedEditors.Add(component.GetInstanceID(), conditionEditor);

                                    EditorGUI.indentLevel++;
                                    conditionEditor.OnInspectorGUI();
                                    EditorGUI.indentLevel--;
                                }
                            }

                            GUILayout.EndVertical();
                        }

                        GUILayout.EndHorizontal();
                    }

                    EditorGUIUtilityExtension.EndFoldout(false);

                    index++;
                }
            }

            EditorGUIUtilityExtension.EndFoldout(false);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Internal Methods

        void GenerateThumbnail()
        {
            try
            {
                string path = EditorUtility.SaveFilePanelInProject(
                           "Save As Thumbnail...",
                           "New Thumbnail.png",
                           "png",
                           "");

                if (path.Length != 0)
                {
                    Texture2D thumbnailTexture = AssetPreview.GetAssetPreview(Target.gameObject);
                    File.WriteAllBytes(path, thumbnailTexture.EncodeToPNG());

                    AssetDatabase.Refresh();

                    Target.GetGeneralSettings.Thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
            catch
            { }
        }

        bool HasMissingColliders()
        {
            List<Transform> missingColliders = Target.GetComponentsInParent<Transform>(true).ToList();

            missingColliders.AddRange(Target.GetComponentsInChildren<Transform>(true));

            for (int i = 0; i < missingColliders.Count; i++)
            {
                if (missingColliders[i].GetComponent<BuildingSocket>() == null)
                {
                    if (missingColliders[i].GetComponent<Collider>() != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        List<BuildingConditionAttribute> LoadBuildingConditions()
        {
            List<BuildingConditionAttribute> conditions = new List<BuildingConditionAttribute>();

            Type[] activeBehaviours = GetAllSubTypes(typeof(MonoBehaviour));

            foreach (Type type in activeBehaviours)
            {
                object[] attributes = type.GetCustomAttributes(typeof(BuildingConditionAttribute), false);

                if (attributes != null)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if ((BuildingConditionAttribute)attributes[i] != null)
                        {
                            ((BuildingConditionAttribute)attributes[i]).Type = type;
                            conditions.Add((BuildingConditionAttribute)attributes[i]);
                        }
                    }
                }
            }

            conditions = conditions.OrderBy(x => x.Order).ToList();

            return conditions;
        }

        Type[] GetAllSubTypes(Type baseType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type T in types)
                {
                    if (T.IsSubclassOf(baseType))
                    {
                        result.Add(T);
                    }
                }
            }

            return result.ToArray();
        }

        #endregion
    }

    public class ModelChanger
    {
        #region Internal Methods

        public static void ChangeModel(BuildingPart target, GameObject lastReference, GameObject newReference)
        {
            if (newReference != null)
            {
                bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target.gameObject);

                string prefabAsset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target.gameObject);

                if (isPrefabInstance)
                {
                    PrefabUtility.UnpackPrefabInstance(target.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                bool inChildren = target.transform.Find(newReference.name) != null;
                GameObject instancedObject;

                if (inChildren)
                {
                    instancedObject = newReference;
                }
                else
                {
                    instancedObject = MonoBehaviour.Instantiate(newReference, target.transform);
                }

                instancedObject.name = instancedObject.name.Replace("(Clone)", "");

                target.GetModelSettings.Models.Add(instancedObject);
                target.GetModelSettings.ModelIndex = target.GetModelSettings.Models.Count - 1;
                target.UpdateModel();

                if (lastReference != null)
                {
                    instancedObject.transform.localPosition = lastReference.transform.localPosition;
                }
                else
                {
                    instancedObject.transform.localPosition = Vector3.zero;
                    instancedObject.transform.localEulerAngles = Vector3.zero;
                }

                target.Colliders = null;
                _ = target.Colliders;

                if (isPrefabInstance)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(target.gameObject, prefabAsset, InteractionMode.UserAction);
                }

                Selection.activeObject = target.transform;
                SceneView.FrameLastActiveSceneView();
            }
        }

        #endregion
    }

    public class ModelOffsetEditor : EditorWindow
    {
        #region Fields

        static Rect m_WindowRect = new Rect(10, 30, 400, 200);

        static Transform m_TransformParent;

        static Transform m_SelectedTransform;
        static Transform GetSelectedTransform
        {
            get
            {
                if (Selection.activeGameObject != null)
                {
                    Transform selectedTransform = Selection.activeGameObject.transform;

                    if (selectedTransform != m_TransformParent)
                    {
                        if (m_TransformParent.Find(selectedTransform.name) != null)
                        {
                            m_SelectedTransform = selectedTransform;
                        }
                        else
                        {
                            m_SelectedTransform = null;
                        }
                    }
                    else
                    {
                        m_SelectedTransform = null;
                    }
                }
                else
                {
                    m_SelectedTransform = null;
                }

                return m_SelectedTransform;
            }
        }

        static BuildingPart m_BuildingPart;

        static Vector3 m_LastPosition;
        static Vector3 m_LastRotation;
        static Vector3 m_LastScale;

        static Vector3 m_DefaultPosition;
        static Vector3 m_DefaultRotation;
        static Vector3 m_DefaultScale;

        #endregion

        #region Unity Methods

        public static void Init(Transform transformParent, BuildingPart buildingPart)
        {
            m_BuildingPart = buildingPart;

            SceneView.duringSceneGui += OnScene;

            m_TransformParent = transformParent;

            if (buildingPart.GetModelSettings.Models != null)
            {
                Selection.activeGameObject = buildingPart.GetModelSettings.GetModel;
            }

            m_DefaultPosition = GetSelectedTransform.localPosition;
            m_DefaultRotation = GetSelectedTransform.localEulerAngles;
            m_DefaultScale = GetSelectedTransform.localScale;
        }

        static void OnScene(SceneView sceneview)
        {
            if (Selection.activeGameObject == null)
            {
                SceneView.duringSceneGui -= OnScene;
            }

            if (GetSelectedTransform != null)
            {
                if (m_LastPosition != m_SelectedTransform.localPosition || m_LastRotation != m_SelectedTransform.localEulerAngles ||
                    m_LastScale != m_SelectedTransform.localScale)
                {
                    m_LastPosition = m_SelectedTransform.localPosition;
                    m_LastRotation = m_SelectedTransform.localEulerAngles;
                    m_LastScale = m_SelectedTransform.localScale;
                }
            }

            Handles.BeginGUI();

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            GUI.backgroundColor = new Color(1, 1, 1, 0f);
            m_WindowRect = GUILayout.Window(controlId, m_WindowRect, WindowContent, "");
            GUI.backgroundColor = new Color(1, 1, 1, 1f);

            Handles.EndGUI();
        }

        static void WindowContent(int id)
        {
            GUILayout.Space(-20);

            GUILayout.BeginVertical("window");

            GUILayout.Space(-20);

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Model Offset Editor",
                "This editor allows you to adjust the offset positions of your model.\n" +
                "Use the fields below to modify the position in each axis.");

            GUI.enabled = GetSelectedTransform != null;

            if (GetSelectedTransform != null)
            {
                GetSelectedTransform.localPosition = EditorGUILayout.Vector3Field("Model Offset Position", GetSelectedTransform.localPosition);
                GetSelectedTransform.localEulerAngles = EditorGUILayout.Vector3Field("Model Offset Rotation", GetSelectedTransform.localEulerAngles);
                GetSelectedTransform.localScale = EditorGUILayout.Vector3Field("Model Offset Scale", GetSelectedTransform.localScale);
            }
            else
            {
                GUILayout.Label("Select a transform child to edit offset...");
            }

            GUI.enabled = true;

            EditorGUILayout.Separator();

            if (GUILayout.Button("Save & Close..."))
            {
                SceneView.duringSceneGui -= OnScene;
                Selection.activeObject = m_TransformParent;
                m_BuildingPart.UpdateModelBounds();
                SceneView.FrameLastActiveSceneView();

                EditorUtility.SetDirty(m_BuildingPart);
            }

            if (GUILayout.Button("Cancel..."))
            {
                SceneView.duringSceneGui -= OnScene;

                if (GetSelectedTransform != null)
                {
                    GetSelectedTransform.localPosition = m_DefaultPosition;
                    GetSelectedTransform.localEulerAngles = m_DefaultRotation;
                    GetSelectedTransform.localScale = m_DefaultScale;
                }

                Selection.activeObject = m_TransformParent;

                SceneView.FrameLastActiveSceneView();
            }

            GUI.DragWindow();

            GUILayout.EndVertical();
        }

        #endregion
    }
}