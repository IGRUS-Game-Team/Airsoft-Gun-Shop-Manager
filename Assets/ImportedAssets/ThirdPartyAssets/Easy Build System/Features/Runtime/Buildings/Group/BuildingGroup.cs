/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroup.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Group
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Group
{
    [ExecuteInEditMode]
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-group")]
    public class BuildingGroup : MonoBehaviour
    {
        #region Fields

        const string ORIGIN_TRANSFORM_NAME = "OriginalParent";
        const string BATCHED_TRANSFORM_NAME = "BatchedParent";

        Transform m_OriginalTransform;
        Transform m_BatchedTransform;

        string m_Identifier;
        public string Identifier { get { return m_Identifier; } set { m_Identifier = value; } }

        [SerializeField] List<BuildingPart> m_RegisteredParts = new List<BuildingPart>();
        public List<BuildingPart> RegisteredBuildingParts { get { return m_RegisteredParts; } }

        public bool IsBatched { get; set; }

        public bool IsActive;

        #region Events

        /// <summary>
        /// Event triggered when a Building Part is registered in this group.
        /// </summary>
        [Serializable]
        public class RegisterBuildingPartEvent : UnityEvent<BuildingPart> { }
        public RegisterBuildingPartEvent OnRegisterBuildingPartEvent = new RegisterBuildingPartEvent();

        /// <summary>
        /// Event triggered when a Building Part is unregistered in this group.
        /// </summary>
        [Serializable]
        public class UnregisterBuildingPartEvent : UnityEvent { }
        public UnregisterBuildingPartEvent OnUnregisterBuildingPartEvent = new UnregisterBuildingPartEvent();

        /// <summary>
        /// Event triggered when the Building Group is batched.
        /// </summary>
        [Serializable]
        public class BatchedBuildingGroupEvent : UnityEvent { }
        public BatchedBuildingGroupEvent OnBatchedBuildingGroupEvent = new BatchedBuildingGroupEvent();

        /// <summary>
        /// Event triggered when the Building Group is unbatched.
        /// </summary>
        [Serializable]
        public class UnbatchedBuildingGroupEvent : UnityEvent { }
        public UnbatchedBuildingGroupEvent OnUnbatchedBuildingGroupEvent = new UnbatchedBuildingGroupEvent();

        #endregion

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterBuildingGroup(this);
            }
        }

        void OnDestroy()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuildingGroup(this);
            }
        }

        void Awake()
        {
            if (transform.Find(ORIGIN_TRANSFORM_NAME) == null)
            {
                m_OriginalTransform = new GameObject(ORIGIN_TRANSFORM_NAME).transform;
                m_OriginalTransform.SetParent(transform);
            }
            else
            {
                m_OriginalTransform = transform.Find(ORIGIN_TRANSFORM_NAME);
            }

            if (transform.Find(BATCHED_TRANSFORM_NAME) == null)
            {
                m_BatchedTransform = new GameObject(BATCHED_TRANSFORM_NAME).transform;
                m_BatchedTransform.SetParent(transform);
            }
            else
            {
                m_BatchedTransform = transform.Find(BATCHED_TRANSFORM_NAME);
            }

            if (Application.isPlaying)
            {
                m_RegisteredParts = new List<BuildingPart>();

                foreach (BuildingPart buildingPart in GetComponentsInChildren<BuildingPart>())
                {
                    RegisterBuildingPart(buildingPart);
                }
            }

            IsActive = true;
        }

        void Start()
        {
            if (BuildingManager.Instance != null)
            {
                if (BuildingManager.Instance.GetBuildingBatchingSettings.UseDynamicBatching && Application.isPlaying)
                {
                    BatchBuildingGroup();
                }
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_RegisteredParts.Count == 0)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Batches the building group, combining the renderers into a single batch.
        /// </summary>
        public void BatchBuildingGroup()
        {
            if (IsBatched)
            {
                return;
            }

            IsBatched = true;

            for (int i = 0; i < m_RegisteredParts.Count; i++)
            {
                if (m_RegisteredParts[i] != null)
                {
                    m_RegisteredParts[i].ChangeState(BuildingPart.StateType.PLACED);
                }
            }

            m_BatchedTransform.gameObject.SetActive(true);

            MeshRenderer[] renderers = m_OriginalTransform.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                BuildingPart buildingPart = renderers[i].GetComponentInParent<BuildingPart>();

                if (buildingPart != null)
                {
                    BuildingPhysicsCondition physicsCondition = buildingPart.TryGetPhysicsCondition;

                    if (physicsCondition != null)
                    {
                        if (!physicsCondition.IsFalling)
                        {
                            MeshRenderer instancedRenderer = Instantiate(renderers[i], m_BatchedTransform.transform, true);
                            instancedRenderer.GetComponent<MeshRenderer>().enabled = true;
                            renderers[i].enabled = false;
                        }
                    }
                    else
                    {
                        MeshRenderer instancedRenderer = Instantiate(renderers[i], m_BatchedTransform.transform, true);
                        instancedRenderer.GetComponent<MeshRenderer>().enabled = true;
                        renderers[i].enabled = false;
                    }
                }
            }

            StaticBatchingUtility.Combine(m_BatchedTransform.gameObject);

            OnBatchedBuildingGroupEvent?.Invoke();
        }

        /// <summary>
        /// Unbatches the building group, restoring the individual renderers.
        /// </summary>
        public void UnbatchBuildingGroup()
        {
            if (!IsBatched)
            {
                return;
            }

            IsBatched = false;

            m_BatchedTransform.gameObject.SetActive(false);

            for (int i = 0; i < m_BatchedTransform.transform.childCount; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_BatchedTransform.transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(m_BatchedTransform.transform.GetChild(i).gameObject);
                }
            }

            MeshRenderer[] renderers = m_OriginalTransform.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;

                if (renderers[i].GetComponent<MeshRenderer>() != null)
                {
                    renderers[i].GetComponent<MeshRenderer>().enabled = true;
                }
            }

            OnUnbatchedBuildingGroupEvent?.Invoke();
        }

        /// <summary>
        /// Calculate the central position, considering all building parts attached to this group.
        /// </summary>
        public Vector3 GetCenterPosition()
        {
            if (m_RegisteredParts.Count == 0 || m_RegisteredParts[0] == null)
            {
                return transform.position;
            }

            Bounds bounds = new Bounds(m_RegisteredParts[0].transform.position, Vector3.zero);

            for (int i = 1; i < m_RegisteredParts.Count; i++)
            {
                if (m_RegisteredParts[i] != null)
                {
                    bounds.Encapsulate(m_RegisteredParts[i].transform.position);
                }
            }

            return bounds.center;
        }

        /// <summary>
        /// Register the Building Part in this group.
        /// </summary>
        /// <param name="buildingPart">The Building Part to register.</param>
        public void RegisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            buildingPart.AttachedBuildingGroup = this;

            buildingPart.transform.SetParent(m_OriginalTransform, true);

            m_RegisteredParts.Add(buildingPart);

            OnRegisterBuildingPartEvent?.Invoke(buildingPart);
        }

        /// <summary>
        /// Unregister the Building Part from this group. 
        /// </summary>
        /// <param name="buildingPart">The Building Part to unregister.</param>
        public void UnregisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            buildingPart.AttachedBuildingGroup = null;

            if (buildingPart.gameObject.activeInHierarchy)
            {
                buildingPart.transform.parent = null;
            }

            m_RegisteredParts.Remove(buildingPart);

            OnUnregisterBuildingPartEvent?.Invoke();
        }

        #endregion
    }
}