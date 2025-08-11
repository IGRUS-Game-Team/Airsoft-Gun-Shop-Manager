/// <summary>
/// Project : Easy Build System
/// Class : BuildingNavMeshConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part.Condition
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.AI;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part.Condition
{
    [CustomEditor(typeof(BuildingNavMeshCondition))]
    public class BuildingNavMeshConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public BuildingNavMeshCondition Target
        {
            get
            {
                return (BuildingNavMeshCondition)target;
            }
        }

        UnityEditor.Editor m_NavMeshObstacle;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (NavMesh.CalculateTriangulation().vertices.Length == 0)
            {
                EditorGUILayout.HelpBox("No NavMesh surfaces detected in the current scene.\n" +
                    "Please bake a new NavMesh to ensure correct functionality for this condition.", MessageType.Warning);

                EditorGUILayout.Separator();
            }

            if (m_NavMeshObstacle == null)
            {
                m_NavMeshObstacle = UnityEditor.Editor.CreateEditor(Target.Obstacle);
            }
            else
            {
                m_NavMeshObstacle.OnInspectorGUI();
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(m_NavMeshObstacle);
        }

        #endregion

        #region Internal Methods

        #endregion
    }
}