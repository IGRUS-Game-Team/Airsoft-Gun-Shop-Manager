/// <summary>
/// Project : Easy Build System
/// Class : BuildingNavMeshCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.AI;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building NavMesh Condition",
    "This conditions enables agents to avoid obstacles by using NavMeshObstacles component based on the model bounds.\n\n" +
    "You can find more information on the Building NavMesh Condition component in the documentation.", 5)]
    public class BuildingNavMeshCondition : BuildingCondition
    {
        #region Fields

        [SerializeField] NavMeshObstacle m_Obstacle;
        public NavMeshObstacle Obstacle { get { return m_Obstacle; } }

        #endregion

        #region Internal Methods

        public override void EnableCondition()
        {
            if (m_Obstacle == null)
            {
                m_Obstacle = gameObject.AddComponent<NavMeshObstacle>();
                m_Obstacle.hideFlags = HideFlags.HideInInspector;

                m_Obstacle.center = GetBuildingPart.GetModelSettings.ModelBounds.center;
                m_Obstacle.size = GetBuildingPart.GetModelSettings.ModelBounds.size;

                m_Obstacle.carving = true;
            }
            else
            {
                m_Obstacle = GetComponent<NavMeshObstacle>();
                m_Obstacle.hideFlags = HideFlags.HideInInspector;
            }
        }

        public override void DisableCondition()
        {
            if (m_Obstacle != null)
            {
                DestroyImmediate(m_Obstacle, true);
            }
        }

        #endregion
    }
}