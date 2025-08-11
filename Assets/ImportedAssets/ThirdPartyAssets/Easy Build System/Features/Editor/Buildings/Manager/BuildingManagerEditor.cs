/// <summary>
/// Project : Easy Build System
/// Class : BuildingManagerEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Manager
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection;

using EasyBuildSystem.Features.Editor.Extensions;
using EasyBuildSystem.Features.Editor.Extensions.ReorderableList;

namespace EasyBuildSystem.Features.Editor.Buildings.Manager
{
    [CustomEditor(typeof(BuildingManager))]
    public class BuildingManagerEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_GeneralFoldout = true;
        bool m_AreaOfInterestFoldout;
        bool m_BuildingBatchingFoldout;

        List<BuildingCollection> m_BuildingCollections = new List<BuildingCollection>();
        string[] m_Collections;
        int m_CollectionIndex;

        ReorderableList m_BuildingPartList;
        ReorderableList m_BuildingTypeList;

        readonly List<UnityEditor.Editor> BuildingPartPreviews = new List<UnityEditor.Editor>();

        BuildingManager Target 
        { 
            get 
            { 
                return (BuildingManager)target; 
            } 
        }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_BuildingCollections = FindAssetsByType<BuildingCollection>();

            m_Collections = new string[m_BuildingCollections.Count + 1];
            m_Collections[0] = "Load Building Collection...";
            for (int i = 0; i < m_BuildingCollections.Count; i++)
            {
                m_Collections[i + 1] = m_BuildingCollections[i].name;
            }

            m_BuildingPartList = new ReorderableList(serializedObject.FindProperty("m_BuildingPartReferences"), false);
            m_BuildingTypeList = new ReorderableList(serializedObject.FindProperty("m_BuildingTypes"), false);
        }

        void OnDisable()
        {
            for (int i = 0; i < BuildingPartPreviews.Count; i++)
            {
                DestroyImmediate(BuildingPartPreviews[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Manager",
                "This component manages all the various components of the system and offers optimization features to improve performance.\n" +
                "You can find more information on the Building Manager component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.Separator();

                GUILayout.Label("Building Parts Settings", EditorStyles.boldLabel);

                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(3f);
                GUILayout.BeginVertical();
                if (m_BuildingPartList != null)
                {
                    m_BuildingPartList.Layout();
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                EditorGUI.BeginChangeCheck();

                GUI.color = Color.black / 4;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your Building Part / Collection here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

                if (DropRect.Contains(Event.current.mousePosition))
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
                            if (DragAndDrop.objectReferences[i] is BuildingCollection)
                            {
                                Target.BuildingPartReferences.AddRange(((BuildingCollection)DragAndDrop.objectReferences[i]).BuildingParts);
                                EditorUtility.SetDirty(target);
                                Repaint();
                            }
                            else
                            {
                                GameObject DraggedObject = DragAndDrop.objectReferences[i] as GameObject;

                                if (DraggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> Cannot add empty object!");
                                    return;
                                }

                                if (!PrefabUtility.IsPartOfPrefabAsset(DraggedObject))
                                {
                                    DraggedObject = PrefabUtility.GetCorrespondingObjectFromSource(DraggedObject);

                                    if (DraggedObject == null)
                                    {
                                        Debug.LogError("<b>Easy Build System</b> Object have not Building Part component or the prefab is not the original.");
                                        return;
                                    }
                                }

                                BuildingPart DraggedPiece = DraggedObject.GetComponent<BuildingPart>();

                                if (DraggedPiece == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> Only building parts can be added to list!");
                                    return;
                                }

                                if (Target.BuildingPartReferences.Find(entry => entry.GetGeneralSettings.Identifier == DraggedPiece.GetGeneralSettings.Identifier) == null)
                                {
                                    Undo.RecordObject(target, "Cancel Add Piece");
                                    Target.BuildingPartReferences.Add(DraggedPiece);
                                    EditorUtility.SetDirty(target);
                                    Repaint();
                                }
                                else
                                {
                                    Debug.LogError("<b>Easy Build System</b> The building part 'identifier' already exists in the list.");
                                }
                            }
                        }

                        Event.current.Use();
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                }

                GUILayout.EndVertical();

                EditorGUILayout.Separator();

                EditorGUI.BeginChangeCheck();

                m_CollectionIndex = EditorGUILayout.Popup("Building Collection", m_CollectionIndex, m_Collections);

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_CollectionIndex - 1 != -1)
                    {
                        Undo.RecordObject(target, "Cancel Push Collection");

                        for (int i = 0; i < m_BuildingCollections[m_CollectionIndex - 1].BuildingParts.Count; i++)
                        {
                            BuildingPart buildingPart = m_BuildingCollections[m_CollectionIndex - 1].BuildingParts[i];

                            if (buildingPart != null)
                            {
                                if (Target.BuildingPartReferences.Find(x => x != null &&
                                    x.GetGeneralSettings.Identifier == buildingPart.GetGeneralSettings.Identifier) == null)
                                {
                                    Target.BuildingPartReferences.Add(buildingPart);
                                }
                                else
                                {
                                    Debug.LogWarning("<b>Easy Build System</b> The Building Part with the name <b>" + buildingPart.GetGeneralSettings.Name + "</b> already exists!");
                                }
                            }
                        }

                        if (m_BuildingCollections[m_CollectionIndex - 1].BuildingParts.Count == 0)
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Collection <b>" + m_BuildingCollections[m_CollectionIndex - 1].name + "</b> is empty!");
                        }
                        else
                        {
                            Debug.Log("<b>Easy Build System</b> The Building Collection <b>" + m_BuildingCollections[m_CollectionIndex - 1].name + "</b> has been added!");
                        }

                        EditorUtility.SetDirty(target);

                        Repaint();

                        m_CollectionIndex = 0;
                    }
                }

                if (GUILayout.Button("Create New Building Collection..."))
                {
                    MenuComponent.CreateBuildingCollection();
                }

                if (GUILayout.Button("Clear All Building Part References..."))
                {
                    if (EditorUtility.DisplayDialog("Clear All Building Parts",
                            "Are you sure you want to clear all the building parts references? This action cannot be undone.", "Yes", "No"))
                    {
                        Target.BuildingPartReferences.Clear();
                        Debug.Log("<b>Easy Build System</b> All building part references have been cleared.");
                    }
                }

                EditorGUILayout.Separator();

                GUILayout.Label("Building Types Settings", EditorStyles.boldLabel);

                EditorGUILayout.Separator();

                if (m_BuildingTypeList != null)
                {
                    EditorGUI.BeginChangeCheck();

                    m_BuildingTypeList.Layout();

                    if (EditorGUI.EndChangeCheck())
                    {
                        BuildingType.Instance.BuildingTypes.Clear();

                        for (int i = 0; i < m_BuildingTypeList.Native.count; i++)
                        {
                            SerializedProperty element = m_BuildingTypeList.Native.serializedProperty.GetArrayElementAtIndex(i);
                            BuildingType.Instance.BuildingTypes.Add(element.stringValue);
                        }

                        EditorUtility.SetDirty(BuildingType.Instance);
                        EditorUtility.SetDirty(target);

                    }
                }

                EditorGUILayout.Separator();
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_AreaOfInterestFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Area Of Interest Settings"), m_AreaOfInterestFoldout);

            if (m_AreaOfInterestFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AreaOfInterest"),
                    new GUIContent("Use Area Of Interest",
                    "Disable Building Areas and Building Sockets that are far from the camera to optimize performance by preventing collider limit issues."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AffectBuildingAreas"),
                    new GUIContent("Area Of Interest Affect Areas", "Affect Building Areas?"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AffectBuildingSockets"),
                    new GUIContent("Area Of Interest Affect Sockets", "Affect Building Building Sockets?"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_RefreshInterval"),
                    new GUIContent("Area Of Interest Refresh Interval",
                    "The update frequency in seconds. Recommended minimum value is 0.5f by default."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_BuildingBatchingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Building Batching Settings"), m_BuildingBatchingFoldout);

            if (m_BuildingBatchingFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingBatchingSettings").FindPropertyRelative("m_UseBuildingBatching"),
                    new GUIContent("Use Building Batching",
                    "Combine meshes of Building Parts sharing the same Materials to reduce draw calls and improve performance at runtime."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        #endregion
    }
}