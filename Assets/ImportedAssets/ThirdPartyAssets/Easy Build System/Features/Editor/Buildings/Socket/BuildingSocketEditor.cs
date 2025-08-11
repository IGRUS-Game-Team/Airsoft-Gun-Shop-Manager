/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocketEditor.cs
/// Namespace : EasyBuildSystem.Editor.Runtime.Buildings.Socket
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Linq;

using UnityEngine;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

#if UNITY_2020_1_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Editor.Runtime.Buildings.Socket
{
    [CustomEditor(typeof(BuildingSocket))]
    public class BuildingSocketEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingSocket Target
        {
            get
            {
                return ((BuildingSocket)target);
            }
        }

        static BuildingSocket.SnappingPointSettings m_CurrentOffset;

        static bool m_GeneralFoldout = true;
        static bool m_SnappingFoldout;

        string[] m_References = new string[0];
        int m_SelectedReference;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            PrefabStage.prefabStageDirtied += (PrefabStage stage) =>
            {
                if (Target == null)
                {
                    return;
                }

                EditorUtility.SetDirty(Target);
            };

            PrefabStage.prefabStageClosing += (PrefabStage stage) =>
            {
                if (Target == null)
                {
                    return;
                }

                Target.ClearPreview();
            };

            if (BuildingManager.Instance != null)
            {
                m_References = new string[BuildingManager.Instance.BuildingPartReferences.Count];

                for (int i = 0; i < BuildingManager.Instance.BuildingPartReferences.Count; i++)
                {
                    if (BuildingManager.Instance.BuildingPartReferences[i] != null)
                    {
                        m_References[i] = BuildingManager.Instance.BuildingPartReferences[i].name;
                    }
                }
            }
        }

        void OnDisable()
        {
            if (!Application.isPlaying && Target != null)
            {
                Target.ClearPreview();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Target.Preview != null)
            {
                if (m_CurrentOffset != null)
                {
                    Target.Snap(Target.Preview, m_CurrentOffset, Vector3.zero);
                }
            }

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Socket", "This component handles the alignment and connection of Building Parts to specific offset positions.\n" +
            "You can find more information on the Building Socket component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SocketRadius"),
                    new GUIContent("Socket Radius", "The radius of the socket used for snapping."));

                BuildingSocket.ShowGizmos = EditorGUILayout.Toggle("Show Gizmos", BuildingSocket.ShowGizmos);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_SnappingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Snapping Settings"), m_SnappingFoldout);

            if (m_SnappingFoldout)
            {
                EditorGUILayout.Separator();

                if (serializedObject.FindProperty("m_SnappingPoints").arraySize == 0)
                {
                    GUILayout.Label("No snapping points available...", EditorStyles.miniLabel);
                }
                else
                {
                    int index = 0;

                    foreach (BuildingSocket.SnappingPointSettings offset in Target.SnappingPoints.ToList())
                    {
                        if (offset == null)
                        {
                            return;
                        }

                        EditorGUIUtilityExtension.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(2f);

                        GUILayout.Label("Snapping Point #" + index, EditorStyles.whiteLargeLabel);

                        GUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_MatchBy"),
                            new GUIContent("Snapping Match By", "The type of matching used for snapping."));

                        EditorGUI.indentLevel++;

                        if ((BuildingSocket.SnappingPointSettings.MatchType)serializedObject.FindProperty("m_SnappingPoints")
                            .GetArrayElementAtIndex(index).FindPropertyRelative("m_MatchBy").enumValueIndex == BuildingSocket.SnappingPointSettings.MatchType.BUILDING_PART_TYPE)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Type"),
                                new GUIContent("Building Part Type", "The type of Building Part required to snap on this socket."));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_BuildingPart"),
                                new GUIContent("Building Part Reference", "The reference to the Building Part required to snap on this socket."));
                        }

                        EditorGUI.indentLevel--;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Position"),
                            new GUIContent("Snapping Position", "The position to which the Building Part will be snapped. This position is relative to the parent transform."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Rotation"),
                            new GUIContent("Snapping Rotation", "The rotation to which the Building Part will be snapped. This rotation is relative to the parent transform."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Scale"),
                            new GUIContent("Snapping Scale", "The scale to which the Building Part will be snapped. This scale is relative to the parent transform."));

                        if (Target.Preview != null &&
                            (offset != null && offset.BuildingPart != null ?
                            Target.Preview.GetGeneralSettings.Identifier == offset.BuildingPart.GetGeneralSettings.Identifier :
                            Target.Preview.GetGeneralSettings.Type == offset.Type))
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Hide Preview"))
                            {
                                for (int x = 0; x < Selection.gameObjects.Length; x++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[x].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();
                                    }

                                    EditorUtility.SetDirty(target);
                                }

                                return;
                            }
                            GUI.color = Color.white;

                            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                            {
                                GUI.enabled = !Application.isPlaying && Selection.gameObjects.Length <= 1;
                            }

                            if (GUILayout.Button("Instantiate Preview"))
                            {
                                m_CurrentOffset = Target.SnappingPoints[index];

                                for (int i = 0; i < Selection.gameObjects.Length; i++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[i].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();

                                        BuildingPart buildingPart = buildingSocket.GetOffsetBuildingPart(m_CurrentOffset);

                                        if (buildingPart != null)
                                        {
                                            BuildingSocket.SnappingPointSettings offsetSettings = buildingSocket.GetOffset(buildingPart);
                                            BuildingPart instancedBuildingPart = BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                                offsetSettings.Position, offsetSettings.Rotation, offsetSettings.Scale, false);
                                            buildingSocket.Snap(instancedBuildingPart, offsetSettings, Vector3.zero);
                                            Selection.activeObject = instancedBuildingPart.gameObject;
                                            SceneView.FrameLastActiveSceneView();
                                        }
                                    }
                                }
                            }
                            GUI.enabled = true;
                        }
                        else
                        {
                            if (m_CurrentOffset != null)
                            {
                                if (m_CurrentOffset.MatchBy == BuildingSocket.SnappingPointSettings.MatchType.BUILDING_PART_TYPE &&
                                    m_CurrentOffset.Type != string.Empty)
                                {
                                    m_CurrentOffset.BuildingPart = null;
                                }
                            }

                            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                            {
                                GUI.enabled = !Application.isPlaying && Selection.gameObjects.Length <= 1;
                            }

                            if (GUILayout.Button("Show Preview"))
                            {
                                m_CurrentOffset = Target.SnappingPoints[index];

                                for (int i = 0; i < Selection.gameObjects.Length; i++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[i].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();

                                        BuildingPart buildingPart = buildingSocket.GetOffsetBuildingPart(m_CurrentOffset);

                                        if (buildingPart != null)
                                        {
                                            buildingSocket.ShowPreview(buildingSocket.GetOffset(buildingPart));
                                        }
                                    }
                                }
                            }

                            GUI.enabled = true;
                        }

                        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            GUI.enabled = Selection.gameObjects.Length <= 1;
                        }

                        EditorGUILayout.Separator();

                        if (GUILayout.Button("Duplicate Snapping Point"))
                        {
                            Undo.RecordObject(target, "Cancel duplicate offset");

                            BuildingSocket.SnappingPointSettings offsetSettings = Target.SnappingPoints[index];

                            Target.SnappingPoints.Add(new BuildingSocket.SnappingPointSettings()
                            {
                                MatchBy = offsetSettings.MatchBy,
                                BuildingPart = offsetSettings.BuildingPart,
                                Type = offsetSettings.Type,
                                Position = offsetSettings.Position,
                                Rotation = offsetSettings.Rotation,
                                Scale = offsetSettings.Scale
                            });

                            EditorUtility.SetDirty(target);

                            m_CurrentOffset = Target.SnappingPoints[index];

                            return;
                        }

                        if (GUILayout.Button("Remove Snapping Point"))
                        {
                            Undo.RecordObject(target, "Cancel remove offset");
                            Target.SnappingPoints.Remove(Target.SnappingPoints[index]);
                            EditorUtility.SetDirty(target);
                            return;
                        }

                        GUI.enabled = true;

                        GUILayout.Space(1f);

                        EditorGUIUtilityExtension.EndVertical();

                        EditorGUILayout.Separator();

                        index++;
                    }
                }

                EditorGUILayout.Separator();

                EditorGUIUtilityExtension.BeginVertical();

                Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

                GUI.Box(dropRect, "Drag and Drop your Building Part(s) here to snap them...", EditorStyles.centeredGreyMiniLabel);

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

                            if (!PrefabUtility.IsPartOfPrefabAsset(draggedObject))
                            {
                                draggedObject = PrefabUtility.GetCorrespondingObjectFromSource(draggedObject);

                                if (draggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> The object does not have a Building Part component or the prefab is not the original.");
                                    return;
                                }
                            }

                            BuildingPart draggedBuildingPart = draggedObject.GetComponent<BuildingPart>();

                            if (draggedBuildingPart == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> The object is missing the Building Part component!");
                                return;
                            }

                            if (!BuildingManager.Instance.BuildingPartReferences.Contains(draggedBuildingPart))
                            {
                                Debug.LogError("<b>Easy Build System</b> The Building Part reference does not exist in the Building Manager!");
                                return;
                            }

                            Target.ClearPreview();

                            BuildingSocket.SnappingPointSettings Offset = new BuildingSocket.SnappingPointSettings() { BuildingPart = draggedBuildingPart };

                            Target.SnappingPoints.Insert(Target.SnappingPoints.Count, Offset);
                            Target.SnappingPoints = Target.SnappingPoints.OrderBy(x => i).ToList();

                            m_CurrentOffset = Offset;

                            Target.ShowPreview(m_CurrentOffset);

                            SceneView.FrameLastActiveSceneView();

                            Repaint();

                            EditorUtility.SetDirty(target);
                        }

                        Event.current.Use();
                    }
                }

                EditorGUIUtilityExtension.EndVertical();

                EditorGUI.BeginChangeCheck();

                m_SelectedReference = EditorGUILayout.Popup("Load Building Part", m_SelectedReference, m_References);

                if (EditorGUI.EndChangeCheck())
                {
                    Target.SnappingPoints.Add(new BuildingSocket.SnappingPointSettings()
                    {
                        BuildingPart = BuildingManager.Instance.BuildingPartReferences[m_SelectedReference]
                    });
                }

                if (GUILayout.Button("Create New Snapping Point..."))
                {
                    Target.SnappingPoints.Add(new BuildingSocket.SnappingPointSettings()
                    {
                        BuildingPart = BuildingManager.Instance.BuildingPartReferences[m_SelectedReference]
                    });
                }

                EditorGUILayout.Separator();
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