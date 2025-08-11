/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollectionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Manager.Collection
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection;

namespace EasyBuildSystem.Features.Editor.Buildings.Manager.Collection
{
    [CustomEditor(typeof(BuildingCollection))]
    public class BuildingCollectionEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingCollection Target;

        #endregion

        #region Unity Methods

        public void OnEnable()
        {
            Target = (BuildingCollection)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Collection",
                        "This component stores a collection of parts that can be easily loaded into the Building Manager.\n" +
                        "You can find more information on the Building Collection component in the documentation.");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingParts"),
                new GUIContent("Building Parts", "The references to the Building Parts associated with this Building Collection."));

            EditorGUILayout.Separator();

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
                            Target.BuildingParts.AddRange(((BuildingCollection)DragAndDrop.objectReferences[i]).BuildingParts);
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

                            if (Target.BuildingParts.Find(entry => entry.GetGeneralSettings.Identifier == DraggedPiece.GetGeneralSettings.Identifier) == null)
                            {
                                Undo.RecordObject(target, "Cancel Add Piece");
                                Target.BuildingParts.Add(DraggedPiece);
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

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}