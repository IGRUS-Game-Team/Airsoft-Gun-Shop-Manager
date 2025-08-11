/// <summary>
/// Project : Easy Build System
/// Class : BuildingBasicsCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Area;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Basics Condition",
    "This condition checks fundamental criteria that need to be met for the Building Part.\n\n" +
    "You can find more information on the Building Basics Condition component in the documentation.")]
    public class BuildingBasicsCondition : BuildingCondition
    {
        #region Fields

        [SerializeField] bool m_CanPlacing = true;
        public bool CanPlacing { get { return m_CanPlacing; } set { m_CanPlacing = value; } }

        [SerializeField] bool m_CanDestroying = true;
        public bool CanDestroying { get { return m_CanDestroying; } set { m_CanDestroying = value; } }

        [SerializeField] bool m_CanEditing = true;
        public bool CanEditing { get { return m_CanEditing; } set { m_CanEditing = value; } }

        [SerializeField] bool m_RequireArea;
        public bool RequireArea { get { return m_RequireArea; } set { m_RequireArea = value; } }

        [SerializeField] bool m_RequireSocket;
        public bool RequireSocket { get { return m_RequireSocket; } set { m_RequireSocket = value; } }

        [SerializeField] bool m_IgnoreSocket;
        public bool IgnoreSocket { get { return m_IgnoreSocket; } set { m_IgnoreSocket = value; } }

        [SerializeField] bool m_ShowDebugs = false;

        #endregion

        #region Internal Methods

        public override bool CheckPlacingCondition()
        {
            if (!CanPlacing)
            {
                if (m_ShowDebugs)
                {
                    Debug.LogWarning("<b>Easy Build System</b> The Building Part cannot be placed.");
                }

                return false;
            }

            if (BuildingManager.Instance != null)
            {
                BuildingArea closestArea = BuildingManager.Instance.GetClosestBuildingArea(GetBuildingPart);

                if (closestArea != null)
                {
                    if (!closestArea.CanPlacingAnyBuildingParts)
                    {
                        if (!closestArea.CanPlacingBuildingPart(GetBuildingPart))
                        {
                            if (m_ShowDebugs)
                            {
                                Debug.LogWarning("<b>Easy Build System</b> The Building Part cannot be placed in the current area.");
                            }

                            return false;
                        }
                    }
                }
                else
                {
                    if (RequireArea)
                    {
                        if (m_ShowDebugs)
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Part requires an area to be placed.");
                        }

                        return false;
                    }
                }

                if (RequireSocket)
                {
                    if (GetBuildingPart.AttachedBuildingSocket == null)
                    {
                        if (m_ShowDebugs)
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Part requires a socket to be placed.");
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        public override bool CheckDestroyCondition()
        {
            if (!CanDestroying)
            {
                if (m_ShowDebugs)
                {
                    Debug.LogWarning("<b>Easy Build System</b>: The Building Part cannot be destroyed.");
                }

                return false;
            }

            BuildingArea closestArea = BuildingManager.Instance.GetClosestBuildingArea(GetBuildingPart);

            if (closestArea != null)
            {
                if (!closestArea.CanDestroyingAnyBuildingParts)
                {
                    if (!closestArea.CanDestroyBuildingPart(GetBuildingPart))
                    {
                        if (m_ShowDebugs)
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Part cannot be destroyed in the current area.");
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        public override bool CheckEditingCondition()
        {
            if (!CanEditing)
            {
                if (m_ShowDebugs)
                {
                    Debug.LogWarning("<b>Easy Build System</b> The Building Part cannot be edited.");
                }

                return false;
            }

            BuildingArea closestArea = BuildingManager.Instance.GetClosestBuildingArea(GetBuildingPart);

            if (closestArea != null)
            {
                if (!closestArea.CanEditingAnyBuildingParts)
                {
                    if (!closestArea.CanEditingBuildingPart(GetBuildingPart))
                    {
                        if (m_ShowDebugs)
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Part cannot be edited in the current area.");
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}