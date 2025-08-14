/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Extensions;
using System.Linq;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Collision Condition",
        "This condition checks if the Building Part avoids collisions with undesired colliders during the preview state.\n\n" +
        "You can find more information on the Building Collision Condition component in the documentation.", 2)]
    public class BuildingCollisionCondition : BuildingCondition
    {
        #region Fields

        [SerializeField] LayerMask m_LayerMask = 1 << 0;

        [SerializeField] [Range(0f, 10f)] float m_Tolerance = 1f;

        [SerializeField] bool m_PreventOverlapping = false;

        [SerializeField] float m_OverlappingRaycastLength = 0.5f;

        [SerializeField, BuildingType] string[] m_IgnoreOverlappingTypes;

        [SerializeField] bool m_RequiredBuildingPartCollision = false;

        [SerializeField] bool m_RequireBuildingSurfaces = false;

        [SerializeField] string[] m_BuildingSurfaceTags;

        [SerializeField] bool m_ShowDebugs = false;

        [SerializeField] bool m_ShowGizmos = true;

        #endregion

        public void SetLayerMask(string[] layerNames)
        {
            m_LayerMask = 0;

            foreach (string name in layerNames)
            {
                int layerIndex = LayerMask.NameToLayer(name);

                if (layerIndex != -1)
                {
                    m_LayerMask |= 1 << layerIndex;
                }
                else
                {
                    Debug.LogError("Layer '" + name + "' does not exist.");
                }
            }
        }


        #region Unity Methods

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.Selection.gameObjects.Length > 6)
            {
                return;
            }
#endif

            if (GetBuildingPart == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(transform.position + new Vector3(0, 0.1f, 0), transform.position + new Vector3(0, -m_OverlappingRaycastLength, 0));

            bool canPlacing = CheckPlacingCondition();

            Gizmos.matrix = GetBuildingPart.transform.localToWorldMatrix;

            Gizmos.color = (canPlacing ? Color.cyan : Color.red) / 2f;
            Gizmos.DrawCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);

            Gizmos.color = (canPlacing ? Color.cyan : Color.red);
            Gizmos.DrawWireCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);
        }

        #endregion

        #region Internal Methods

        public override bool CheckPlacingCondition()
        {
            Bounds worldCollisionBounds = GetBuildingPart.transform.GetWorldBounds(GetBuildingPart.GetModelSettings.ModelBounds);
            Collider[] colliders = PhysicsExtension.GetNeighborsType<Collider>(worldCollisionBounds.center, worldCollisionBounds.extents * m_Tolerance, GetBuildingPart.transform.rotation, m_LayerMask);

            bool requiredBuildingPartCollisionSatisfied = !m_RequiredBuildingPartCollision;
            bool requireBuildingSurfacesSatisfied = !m_RequireBuildingSurfaces;

            if (m_PreventOverlapping)
            {
                RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position + new Vector3(0, 0.1f, 0), -transform.up), m_OverlappingRaycastLength, m_LayerMask);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null)
                    {
                        BuildingPart buildingPart = hit.collider.GetComponentInParent<BuildingPart>();

                        if (buildingPart != null && buildingPart.GetInstanceID() != GetBuildingPart.GetInstanceID())
                        {
                            if (m_IgnoreOverlappingTypes.Length == 0)
                            {
                                return false;
                            }
                            else
                            {
                                if (!m_IgnoreOverlappingTypes.Contains(buildingPart.GetGeneralSettings.Type))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            foreach (Collider collider in colliders)
            {
                if (collider == null) continue;

                if (!m_RequiredBuildingPartCollision && !m_RequireBuildingSurfaces)
                {
                    if (collider != null)
                    {
                        BuildingPart colliderBuildingPart = collider.GetComponentInParent<BuildingPart>();

                        if (colliderBuildingPart != null)
                        {
                            if (colliderBuildingPart.GetInstanceID() != GetBuildingPart.GetInstanceID())
                            {
                                if (m_ShowDebugs)
                                {
                                    Debug.LogWarning($"<b>Easy Build System</b> Colliding with object: ({collider.name}).");
                                }

                                return false;
                            }
                        }
                        else
                        {
                            if (m_ShowDebugs)
                            {
                                Debug.LogWarning($"<b>Easy Build System</b> Colliding with object: ({collider.name}).");
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    BuildingPart colliderBuildingPart = collider.GetComponentInParent<BuildingPart>();
                    BuildingCollisionSurface buildingCollisionSurface = collider.GetComponent<BuildingCollisionSurface>();

                    if (colliderBuildingPart != null && colliderBuildingPart.GetInstanceID() == GetBuildingPart.GetInstanceID()) continue;

                    if (m_ShowDebugs)
                    {
                        Debug.LogWarning($"<b>Easy Build System</b> Colliding with object: ({collider.name}).");
                    }

                    if (m_RequiredBuildingPartCollision && colliderBuildingPart != null)
                    {
                        requiredBuildingPartCollisionSatisfied = true;
                    }

                    if (m_RequireBuildingSurfaces && buildingCollisionSurface != null && ContainsSurface(buildingCollisionSurface.Tag))
                    {
                        requireBuildingSurfacesSatisfied = true;
                    }
                    else
                    {
                        if (m_RequiredBuildingPartCollision && colliderBuildingPart != null)
                        {
                            requiredBuildingPartCollisionSatisfied = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return requiredBuildingPartCollisionSatisfied && requireBuildingSurfacesSatisfied;
        }

        bool ContainsSurface(string tag)
        {
            for (int i = 0; i < m_BuildingSurfaceTags.Length; i++)
            {
                if (tag == m_BuildingSurfaceTags[i])
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}