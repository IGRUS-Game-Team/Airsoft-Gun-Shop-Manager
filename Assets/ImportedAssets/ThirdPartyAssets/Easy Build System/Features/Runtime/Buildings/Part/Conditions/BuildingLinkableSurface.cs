/// <summary>
/// Project : Easy Build System
/// Class : BuildingLinkableSurface.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Group;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    public class BuildingLinkableSurface : MonoBehaviour
    {
        [SerializeField] Bounds m_Bounds;

        BuildingGroup m_AttachedGroup;

        void Start()
        {
            BuildingManager.Instance.OnPlacingBuildingPartEvent.AddListener((BuildingPart part) =>
            {
                if (part.State != BuildingPart.StateType.PLACED)
                {
                    return;
                }

                Bounds worldBounds = transform.GetWorldBounds(m_Bounds);

                Collider[] colliders = Physics.OverlapBox(worldBounds.center, worldBounds.extents / 2);

                List<BuildingPart> closestParts = new List<BuildingPart>();

                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].GetComponentInParent<BuildingPart>() != null)
                    {
                        closestParts.Add(colliders[i].GetComponentInParent<BuildingPart>());
                    }
                }

                if (closestParts.Count > 0)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        BuildingGroup closestGroup = colliders[i].GetComponentInParent<BuildingGroup>();

                        if (closestGroup != null)
                        {
                            if (m_AttachedGroup == null)
                            {
                                BuildingGroup instancedGroup = BuildingManager.Instance.CreateBuildingGroup();

                                instancedGroup.IsActive = false;
                                instancedGroup.transform.SetParent(transform);

                                m_AttachedGroup = instancedGroup;
                            }

                            for (int x = 0; x < closestParts.Count; x++)
                            {
                                closestParts[x].AttachedBuildingGroup.UnregisterBuildingPart(closestParts[x]);
                                m_AttachedGroup.RegisterBuildingPart(closestParts[x]);
                            }

                            break;
                        }
                    }
                }
            });
        }

        void OnDrawGizmosSelected()
        {
            Bounds worldBounds = transform.GetWorldBounds(m_Bounds);
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.extents);
        }
    }
}