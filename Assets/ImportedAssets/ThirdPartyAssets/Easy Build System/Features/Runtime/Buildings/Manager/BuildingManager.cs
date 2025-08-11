/// <summary>
/// Project : Easy Build System
/// Class : BuildingManager.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Area;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Group;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager
{
    [ExecuteInEditMode]
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-manager")]
    [DefaultExecutionOrder(-999)]
    public class BuildingManager : MonoBehaviour
    {
        #region Fields

        public static BuildingManager Instance;

        [SerializeField] List<BuildingPart> m_BuildingPartReferences = new List<BuildingPart>();
        public List<BuildingPart> BuildingPartReferences { get { return m_BuildingPartReferences; } set { m_BuildingPartReferences = value; } }

        [SerializeField] List<string> m_BuildingTypes = new List<string>();
        public List<string> BuildingTypes { get { return m_BuildingTypes; } set { m_BuildingTypes = value; } }

        [Serializable]
        public class AreaOfInterestSettings
        {
            [SerializeField] bool m_AreaOfInterest;
            public bool AreaOfInterest { get { return m_AreaOfInterest; } }

            [SerializeField] bool m_AffectBuildingAreas = true;
            public bool AffectBuildingAreas { get { return m_AffectBuildingAreas; } }

            [SerializeField] bool m_AffectBuildingSockets = true;
            public bool AffectBuildingSockets { get { return m_AffectBuildingSockets; } }

            [SerializeField] float m_RefreshInterval = 0.5f;
            public float RefreshInterval { get { return m_RefreshInterval; } }
        }
        [SerializeField] AreaOfInterestSettings m_AreaOfInterestSettings = new AreaOfInterestSettings();
        public AreaOfInterestSettings GetAreaOfInterestSettings { get { return m_AreaOfInterestSettings; } }

        [Serializable]
        public class BuildingBatchingSettings
        {
            [SerializeField] bool m_UseBuildingBatching = true;
            public bool UseDynamicBatching { get { return m_UseBuildingBatching; } }
        }
        [SerializeField] BuildingBatchingSettings m_BuildingBatchingSettings = new BuildingBatchingSettings();
        public BuildingBatchingSettings GetBuildingBatchingSettings { get { return m_BuildingBatchingSettings; } }

        List<BuildingArea> m_RegisteredBuildingAreas = new List<BuildingArea>();
        public List<BuildingArea> RegisteredBuildingAreas { get { return m_RegisteredBuildingAreas; } private set { m_RegisteredBuildingAreas = value; } }

        List<BuildingPart> m_RegisteredBuildingParts = new List<BuildingPart>();
        public List<BuildingPart> RegisteredBuildingParts { get { return m_RegisteredBuildingParts; } private set { m_RegisteredBuildingParts = value; } }

        List<BuildingSocket> m_RegisteredBuildingSockets = new List<BuildingSocket>();
        public List<BuildingSocket> RegisteredBuildingSockets { get { return m_RegisteredBuildingSockets; } private set { m_RegisteredBuildingSockets = value; } }

        List<BuildingGroup> m_RegisteredBuildingGroups = new List<BuildingGroup>();
        public List<BuildingGroup> RegisteredBuildingGroups { get { return m_RegisteredBuildingGroups; } private set { m_RegisteredBuildingGroups = value; } }

        #region Events

        /// <summary>
        /// Event triggered when a Building Part is being placed.
        /// </summary>
        [Serializable]
        public class PlacingBuildingPartEvent : UnityEvent<BuildingPart> { }
        public PlacingBuildingPartEvent OnPlacingBuildingPartEvent = new PlacingBuildingPartEvent();

        /// <summary>
        /// Event triggered when a Building Part is being destroyed.
        /// </summary>
        [Serializable]
        public class DestroyingBuildingPartEvent : UnityEvent<BuildingPart> { }
        public DestroyingBuildingPartEvent OnDestroyingBuildingPartEvent = new DestroyingBuildingPartEvent();

        /// <summary>
        /// Event triggered when a Building Area is registered.
        /// </summary>
        [Serializable]
        public class RegisterBuildingAreaEvent : UnityEvent<BuildingArea> { }
        public RegisterBuildingAreaEvent OnRegisterBuildingAreaEvent = new RegisterBuildingAreaEvent();

        /// <summary>
        /// Event triggered when a Building Area is unregistered.
        /// </summary>
        [Serializable]
        public class UnregisterBuildingAreaEvent : UnityEvent { }
        public UnregisterBuildingAreaEvent OnUnregisterBuildingAreaEvent = new UnregisterBuildingAreaEvent();

        /// <summary>
        /// Event triggered when a Building Part is registered.
        /// </summary>
        [Serializable]
        public class RegisterBuildingPartEvent : UnityEvent<BuildingPart> { }
        public RegisterBuildingPartEvent OnRegisterBuildingPartEvent = new RegisterBuildingPartEvent();

        /// <summary>
        /// Event triggered when a Building Part is unregistered.
        /// </summary>
        [Serializable]
        public class UnregisterBuildingPartEvent : UnityEvent { }
        public UnregisterBuildingPartEvent OnUnregisterBuildingPartEvent = new UnregisterBuildingPartEvent();

        /// <summary>
        /// Event triggered when a Building Socket is registered.
        /// </summary>
        [Serializable]
        public class RegisterBuildingSocketEvent : UnityEvent<BuildingSocket> { }
        public RegisterBuildingSocketEvent OnRegisterBuildingSocketEvent = new RegisterBuildingSocketEvent();

        /// <summary>
        /// Event triggered when a Building Socket is unregistered.
        /// </summary>
        [Serializable]
        public class UnregisterBuildingSocketEvent : UnityEvent { }
        public UnregisterBuildingSocketEvent OnUnregisterBuildingSocketEvent = new UnregisterBuildingSocketEvent();

        /// <summary>
        /// Event triggered when a Building Group is registered.
        /// </summary>
        [Serializable]
        public class RegisterBuildingGroupEvent : UnityEvent<BuildingGroup> { }
        public RegisterBuildingGroupEvent OnRegisterBuildingGroupEvent = new RegisterBuildingGroupEvent();

        /// <summary>
        /// Event triggered when a Building Group is unregistered.
        /// </summary>
        [Serializable]
        public class UnregisterBuildingGroupEvent : UnityEvent { }
        public UnregisterBuildingGroupEvent OnUnregisterBuildingGroupEvent = new UnregisterBuildingGroupEvent();

        #endregion

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_BuildingTypes = BuildingType.Instance.BuildingTypes;

            Instance = this;
        }

        void Awake()
        {
            Instance = this;

            if (m_AreaOfInterestSettings.AreaOfInterest)
            {
                InvokeRepeating(nameof(UpdateAreaOfInterest),
                    m_AreaOfInterestSettings.RefreshInterval, m_AreaOfInterestSettings.RefreshInterval);
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_BuildingBatchingSettings.UseDynamicBatching)
            {
                UpdateBuildingBatching();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Updates the Area of Interest based on the position of the BuildingPlacer.
        /// </summary>
        void UpdateAreaOfInterest()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            if (m_AreaOfInterestSettings.AffectBuildingAreas)
            {
                for (int i = 0; i < RegisteredBuildingAreas.Count; i++)
                {
                    if (RegisteredBuildingAreas[i] != null)
                    {
                        RegisteredBuildingAreas[i].gameObject.SetActive((Vector3.Distance(BuildingPlacer.Instance.transform.position,
                            RegisteredBuildingAreas[i].transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance));
                    }
                }
            }

            if (m_AreaOfInterestSettings.AffectBuildingSockets)
            {
                for (int i = 0; i < RegisteredBuildingSockets.Count; i++)
                {
                    if (RegisteredBuildingSockets[i] != null)
                    {
                        RegisteredBuildingSockets[i].gameObject.SetActive((Vector3.Distance(BuildingPlacer.Instance.transform.position,
                            RegisteredBuildingSockets[i].transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance));
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Building Batching based on the position of the BuildingPlacer.
        /// </summary>
        void UpdateBuildingBatching()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            for (int i = 0; i < m_RegisteredBuildingGroups.Count; i++)
            {
                if (m_RegisteredBuildingGroups[i] != null)
                {
                    if (Vector3.Distance(m_RegisteredBuildingGroups[i].GetCenterPosition(),
                        BuildingPlacer.Instance.transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance * 2f)
                    {
                        m_RegisteredBuildingGroups[i].UnbatchBuildingGroup();
                    }
                    else
                    {
                        m_RegisteredBuildingGroups[i].BatchBuildingGroup();
                    }
                }
            }
        }

        #region Building Area

        /// <summary>
        /// Retrieves the closest Building Area to the specified position.
        /// </summary>
        /// <param name="buildingPart">The Building Part to find the closest Building Area to.</param>
        /// <returns>The closest Building Area to the specified position, or null if no active Building Areas are found.</returns>
        public BuildingArea GetClosestBuildingArea(BuildingPart buildingPart)
        {
            BuildingArea highestPriorityArea = null;
            int highestPriority = int.MinValue;

            for (int i = 0; i < m_RegisteredBuildingAreas.Count; i++)
            {
                if (m_RegisteredBuildingAreas[i] != null && m_RegisteredBuildingAreas[i].gameObject.activeSelf)
                {
                    bool intersects = false;

                    if (m_RegisteredBuildingAreas[i].Shape == BuildingArea.ShapeType.BOUNDS)
                    {
                        Bounds areaBounds = m_RegisteredBuildingAreas[i].transform.GetWorldBounds(m_RegisteredBuildingAreas[i].Bounds);
                        Bounds previewBounds = buildingPart.transform.GetWorldBounds(buildingPart.GetModelSettings.ModelBounds);

                        if (areaBounds.Intersects(previewBounds))
                        {
                            intersects = true;
                        }
                    }
                    else
                    {
                        Vector3 sphereCenter = m_RegisteredBuildingAreas[i].transform.position;
                        Bounds previewBounds = buildingPart.transform.GetWorldBounds(buildingPart.GetModelSettings.ModelBounds);
                        intersects = previewBounds.Intersects(new Bounds(sphereCenter, Vector3.one * (m_RegisteredBuildingAreas[i].Radius * 2f)));
                    }

                    if (intersects && m_RegisteredBuildingAreas[i].Priority > highestPriority)
                    {
                        highestPriorityArea = m_RegisteredBuildingAreas[i];
                        highestPriority = m_RegisteredBuildingAreas[i].Priority;
                    }
                }
            }

            return highestPriorityArea;
        }

        /// <summary>
        /// Registers a Building Area.
        /// </summary>
        /// <param name="area">The Building Area to register.</param>
        public void RegisterBuildingArea(BuildingArea area)
        {
            if (area == null)
            {
                return;
            }

            if (RegisteredBuildingAreas.Contains(area))
            {
                return;
            }

            RegisteredBuildingAreas.Add(area);
            OnRegisterBuildingAreaEvent.Invoke(area);
        }

        /// <summary>
        /// Unregisters a Building Area.
        /// </summary>
        /// <param name="area">The Building Area to unregister.</param>
        public void UnregisterBuildingArea(BuildingArea area)
        {
            if (area == null)
            {
                return;
            }

            RegisteredBuildingAreas.Remove(area);

            OnUnregisterBuildingAreaEvent.Invoke();
        }

        #endregion

        #region Building Part

        /// <summary>
        /// Places a new Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to place.</param>
        /// <param name="position">The position to place the Building Part.</param>
        /// <param name="rotation">The rotation of the Building Part.</param>
        /// <param name="scale">The scale of the Building Part.</param>
        /// <param name="createNewGroup">Whether to create a new Building Group for the Building Part (default: true).</param>
        /// <returns>The placed Building Part.</returns>
        public BuildingPart PlaceBuildingPart(BuildingPart buildingPart, Vector3 position, Vector3 rotation, Vector3 scale, bool createNewGroup = true)
        {
            if (buildingPart == null)
            {
                return null;
            }

            if (buildingPart.AttachedBuildingSocket != null)
            {
                Debug.Log(buildingPart.AttachedBuildingSocket.name);
            }

            BuildingPart instancedBuildingPart = Instantiate(buildingPart.gameObject,
                position, Quaternion.Euler(rotation)).GetComponent<BuildingPart>();

            instancedBuildingPart.transform.localScale = scale;

            instancedBuildingPart.ChangeState(BuildingPart.StateType.PLACED);

            BuildingArea closestArea = GetClosestBuildingArea(buildingPart);

            if (closestArea != null)
            {
                closestArea.RegisterBuildingPart(instancedBuildingPart);
            }
            
            if (createNewGroup != false)
            {
                BuildingGroup closestGroup = GetClosestBuildingGroup(instancedBuildingPart);

                if (closestGroup != null)
                {
                    closestGroup.RegisterBuildingPart(instancedBuildingPart);
                }
                else
                {
                    BuildingGroup instancedGroup = CreateBuildingGroup();
                    instancedGroup.RegisterBuildingPart(instancedBuildingPart);
                }
            }

            OnPlacingBuildingPartEvent.Invoke(instancedBuildingPart);

            return instancedBuildingPart;
        }

        /// <summary>
        /// Destroys a Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to destroy.</param>
        public void DestroyBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(buildingPart.gameObject);
            }
            else
            {
                DestroyImmediate(buildingPart.gameObject);
            }
        }

        /// <summary>
        /// Gets a Building Part by its identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the Building Part to retrieve.</param>
        /// <returns>The Building Part with the specified identifier, or null if not found.</returns>
        public BuildingPart GetBuildingPartByIdentifier(string identifier)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Identifier == identifier)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a Building Part by its name.
        /// </summary>
        /// <param name="name">The name of the Building Part to retrieve.</param>
        /// <returns>The Building Part with the specified name, or null if not found.</returns>
        public BuildingPart GetBuildingPartByName(string name)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Name == name)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a Building Part by its category.
        /// </summary>
        /// <param name="category">The category of the Building Part to retrieve.</param>
        /// <returns>The Building Part with the specified category, or null if not found.</returns>
        public BuildingPart GetBuildingPartByCategory(string type)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Type == type)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a Building Part by index.
        /// </summary>
        public BuildingPart GetBuildingPartByIndex(int index)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (i == index)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            Debug.LogWarning("<b>Easy Build System</b> : Could not find the Building Part with index: " + index + "");

            return null;
        }

        /// <summary>
        /// Registers a Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to register.</param>
        public void RegisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            if (RegisteredBuildingParts.Contains(buildingPart))
            {
                return;
            }

            RegisteredBuildingParts.Add(buildingPart);
            OnRegisterBuildingPartEvent.Invoke(buildingPart);
        }

        /// <summary>
        /// Unregisters a Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to unregister.</param>
        public void UnregisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            RegisteredBuildingParts.Remove(buildingPart);
            OnUnregisterBuildingPartEvent.Invoke();
        }

        #endregion

        #region Building Socket

        /// <summary>
        /// Registers a Building Socket.
        /// </summary>
        /// <param name="socket">The Building Socket to register.</param>
        public void RegisterBuildingSocket(BuildingSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            if (RegisteredBuildingSockets.Contains(socket))
            {
                return;
            }

            RegisteredBuildingSockets.Add(socket);
            OnRegisterBuildingSocketEvent.Invoke(socket);
        }

        /// <summary>
        /// Unregisters a Building Socket.
        /// </summary>
        /// <param name="socket">The Building Socket to unregister.</param>
        public void UnregisterBuildingSocket(BuildingSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            RegisteredBuildingSockets.Remove(socket);
            OnUnregisterBuildingSocketEvent.Invoke();
        }

        #endregion

        #region Building Group 

        /// <summary>
        /// Creates a new Building Group with the specified Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to include in the new Building Group.</param>
        /// <returns>The newly created Building Group.</returns>
        public BuildingGroup CreateBuildingGroup()
        {
            int newInstanceId = MathExtension.GetUniqueID();

            BuildingGroup instancedGroup =
                new GameObject("Building Group (" + newInstanceId + ")").AddComponent<BuildingGroup>();

            instancedGroup.Identifier = newInstanceId.ToString();

            return instancedGroup;
        }

        /// <summary>
        /// Gets the closest Building Group to the specified Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to find the closest Building Group from.</param>
        /// <returns>The closest Building Group to the specified Building Part, or null if not found.</returns>
        public BuildingGroup GetClosestBuildingGroup(BuildingPart buildingPart)
        {
            for (int i = 0; i < RegisteredBuildingGroups.Count; i++)
            {
                if (RegisteredBuildingGroups[i] != null)
                {
                    if (RegisteredBuildingGroups[i].IsActive)
                    {
                        if (Vector3.Distance(buildingPart.transform.position,
                            RegisteredBuildingGroups[i].GetCenterPosition()) < 16)
                        {
                            return RegisteredBuildingGroups[i];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Registers a Building Group.
        /// </summary>
        /// <param name="group">The Building Group to register.</param>
        public void RegisterBuildingGroup(BuildingGroup group)
        {
            if (group == null)
            {
                return;
            }

            if (RegisteredBuildingGroups.Contains(group))
            {
                return;
            }

            RegisteredBuildingGroups.Add(group);
            OnRegisterBuildingGroupEvent.Invoke(group);
        }

        /// <summary>
        /// Unregisters a Building Group.
        /// </summary>
        /// <param name="group">The Building Group to unregister.</param>
        public void UnregisterBuildingGroup(BuildingGroup group)
        {
            if (group == null)
            {
                return;
            }

            RegisteredBuildingGroups.Remove(group);
            OnUnregisterBuildingGroupEvent.Invoke();
        }

        #endregion

        #endregion
    }
}