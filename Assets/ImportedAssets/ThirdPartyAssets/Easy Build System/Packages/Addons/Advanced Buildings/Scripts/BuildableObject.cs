using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using EasyBuildSystem.Features.Runtime.Buildings.Part;

using EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    [RequireComponent(typeof(BuildingPart))]
    public class BuildableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] InteractableType m_InteractableType;
        public InteractableType InteractableType => m_InteractableType;

        [Serializable]
        public class RequiredResource
        {
            [SerializeField] string m_ResourceType;
            public string ResourceType { get { return m_ResourceType; } set { m_ResourceType = value; } }

            [SerializeField] Renderer[] m_Meshes;
            public Renderer[] Meshes => m_Meshes;

            public bool IsAdded { get; set; }
        }

        [SerializeField] RequiredResource[] m_RequiredResources;
        public RequiredResource[] RequiredResources => m_RequiredResources;

        List<Renderer> m_DuplicatedRenderers = new List<Renderer>();

        BuildingPart m_BuildingPart;
        public BuildingPart BuildingPart
        {
            get
            {
                if (m_BuildingPart == null)
                {
                    m_BuildingPart = GetComponent<BuildingPart>();
                }

                return m_BuildingPart;
            }
        }

        Renderer m_Renderer;
        public Bounds MeshBounds
        {
            get
            {
                if (m_Renderer == null)
                {
                    m_Renderer = GetComponentInChildren<Renderer>();
                }

                return m_Renderer.bounds;
            }
        }

        void Awake()
        {
            BuildingPart.OnChangedStateEvent.AddListener(OnChangedState);

            foreach (RequiredResource requiredResource in m_RequiredResources)
            {
                if (requiredResource.Meshes != null)
                {
                    foreach (Renderer mesh in requiredResource.Meshes)
                    {
                        Renderer duplicatedRender = Instantiate(mesh, transform, true);
                        duplicatedRender.transform.localScale *= 1.025f;
                        duplicatedRender.enabled = false;
                        m_DuplicatedRenderers.Add(duplicatedRender);
                    }
                }
            }

            BuildingPart.GetPreviewSettings.IgnoreRenderers.AddRange(m_DuplicatedRenderers);

            InteractionController.Instance.OnInteractedEvent.AddListener(OnInteracted);

            SetCollisionWithPlayer(true);
        }

        void Start()
        {
            if (BuildingPart.State != BuildingPart.StateType.QUEUE)
            {
                return;
            }

            if (BuildingPart.GetSaveData().Properties.Count == 0)
            {
                return;
            }

            for (int x = 0; x < m_RequiredResources.Length; x++)
            {
                string[] data = BuildingPart.GetSaveData().Properties[x].Split('|');

                m_RequiredResources[x].ResourceType = data[0];
                m_RequiredResources[x].IsAdded = bool.Parse(data[1]);

                if (m_RequiredResources[x].IsAdded)
                {
                    if (m_DuplicatedRenderers.Count > 0 && m_DuplicatedRenderers.Count > m_RequiredResources.ToList().IndexOf(m_RequiredResources[x]))
                    {
                        m_DuplicatedRenderers[m_RequiredResources.ToList().IndexOf(m_RequiredResources[x])].enabled = true;
                    }
                }
            }

            if (IsComplete())
            {
                Complete();
            }
        }

        void SetCollisionWithPlayer(bool ignore)
        {
            Collider[] colliders = BuildingPart.Colliders.ToArray();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");

                    if (player != null)
                    {
                        Collider playerCollider = player.GetComponent<Collider>();

                        if (playerCollider != null)
                        {
                            Physics.IgnoreCollision(colliders[i], playerCollider, ignore);
                        }
                    }
                }
            }
        }

        void OnInteracted(IInteractable interactable)
        {
            if (!(interactable is BuildableObject buildableObject) || buildableObject != this)
                return;

            foreach (RequiredResource requiredResource in m_RequiredResources)
            {
                if (requiredResource.IsAdded)
                    continue;

                CarriableObject carriableObject = CarriableController.Instance.CurrentCarriables.FindLast(x => x.CarriableType == requiredResource.ResourceType);

                if (carriableObject == null)
                    continue;

                requiredResource.IsAdded = true;

                BuildingPart.GetSaveData().Properties.Clear();
                for (int i = 0; i < m_RequiredResources.Length; i++)
                {
                    BuildingPart.GetSaveData().Properties.Add(m_RequiredResources[i].ResourceType + "|" + m_RequiredResources[i].IsAdded);
                }

                if (m_DuplicatedRenderers.Count > 0 && m_DuplicatedRenderers.Count > m_RequiredResources.ToList().IndexOf(requiredResource))
                {
                    m_DuplicatedRenderers[m_RequiredResources.ToList().IndexOf(requiredResource)].enabled = true;
                }

                CarriableController.Instance.Remove(carriableObject);

                break;
            }

            if (IsComplete())
            {
                Complete();
            }
        }

        public bool IsComplete()
        {
            return m_RequiredResources.All(requiredResource => requiredResource.IsAdded);
        }

        public void Complete()
        {
            foreach (Renderer duplicatedRender in m_DuplicatedRenderers)
            {
                Destroy(duplicatedRender.gameObject);
            }

            BuildingPart.OnChangedStateEvent.RemoveAllListeners();

            BuildingPart.ChangeState(BuildingPart.StateType.PLACED);

            SetCollisionWithPlayer(false);

            Destroy(this);
        }

        public int GetCurrentResourceCount(string resourceType)
        {
            return m_RequiredResources.Count(requiredResource => requiredResource.ResourceType == resourceType && requiredResource.IsAdded);
        }

        public int GetResourceCount(string resourceType)
        {
            return m_RequiredResources.Count(requiredResource => requiredResource.ResourceType == resourceType);
        }

        void OnChangedState(BuildingPart.StateType state)
        {
            if (state == BuildingPart.StateType.PLACED)
            {
                BuildingPart.ChangeState(BuildingPart.StateType.QUEUE);
            }
        }
    }
}