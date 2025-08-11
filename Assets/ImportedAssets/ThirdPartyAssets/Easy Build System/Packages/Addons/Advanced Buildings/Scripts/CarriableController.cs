using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    public class CarriableController : MonoBehaviour
    {
        public static CarriableController Instance;

        [Serializable]
        public class CarriableSettings
        {
            [Serializable]
            public class CarriableType
            {
                public Transform Parent;
                public CarriableObject CarriableObject;
                public Vector3 OffsetPosition;
                public Vector3 OffsetRotation;
            }

            public string ResourceType;
            public CarriableType[] CarriableTypes;
        }

        [Header("Carriable Controller Settings")]
        [SerializeField] CarriableSettings[] m_Carriables;

#if ENABLE_INPUT_SYSTEM
        [SerializeField] UnityEngine.InputSystem.Key m_InputDropKey = UnityEngine.InputSystem.Key.G;
#else
        [SerializeField] KeyCode m_DropKey = KeyCode.G;
#endif
        [SerializeField] float m_DropForce = 10f;

        readonly List<CarriableObject> m_CurrentCarriables = new List<CarriableObject>();
        public List<CarriableObject> CurrentCarriables => m_CurrentCarriables;

        public int CarriableCount { get; set; }

        string m_CurrentType;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            InteractionController.Instance.OnInteractedEvent.AddListener(OnInteracted);

            m_CurrentType = string.Empty;
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current[m_InputDropKey].wasPressedThisFrame && CarriableCount > 0)
            {
                Drop(m_DropForce);
            }
#else
            if (Input.GetKeyDown(m_DropKey) && CarriableCount > 0)
            {
                Drop(m_DropForce);
            }
#endif
        }

        void OnInteracted(IInteractable interactable)
        {
            if (interactable.InteractableType != InteractableType.CARRIABLE)
                return;

            CarriableObject carriableObject = interactable as CarriableObject;

            if (!IsPickable(carriableObject))
                return;

            if (IsFull(carriableObject))
                return;

            if (m_CurrentType == string.Empty)
            {
                m_CurrentType = carriableObject.CarriableType;
            }
            else if (m_CurrentType != carriableObject.CarriableType)
            {
                return;
            }

            m_CurrentCarriables.Add(carriableObject);

            CarriableSettings.CarriableType nextPosition = GetNextPosition(carriableObject.CarriableType);

            if (nextPosition == null)
            {
                return;
            }

            carriableObject.PickUp(nextPosition);

            CarriableCount++;
        }

        public void Remove(CarriableObject carriableObject)
        {
            CarriableCount--;

            CurrentCarriables.Remove(carriableObject);

            if (CarriableCount == 0)
            {
                m_CurrentType = string.Empty;
            }

            Destroy(carriableObject.gameObject);
        }

        bool IsPickable(CarriableObject carriableObject)
        {
            return m_Carriables.Any(carriable => carriable.CarriableTypes.Any(type => type.CarriableObject.CarriableType == carriableObject.CarriableType));
        }

        bool IsFull(CarriableObject carriableObject)
        {
            foreach (CarriableSettings carriable in m_Carriables)
            {
                if (carriable.ResourceType == carriableObject.CarriableType && carriable.CarriableTypes.Length <= CarriableCount)
                {
                    return true;
                }
            }

            return false;
        }

        CarriableSettings.CarriableType GetNextPosition(string type)
        {
            foreach (CarriableSettings carriable in m_Carriables)
            {
                if (carriable.ResourceType == type && carriable.CarriableTypes.Length > CarriableCount)
                {
                    return carriable.CarriableTypes[CarriableCount];
                }
            }

            return null;
        }

        void Drop(float force)
        {
            int index = CarriableCount - 1;

            m_CurrentCarriables[index].Drop(transform.TransformDirection(Vector3.up + Vector3.forward * force));
            m_CurrentCarriables.RemoveAt(index);

            CarriableCount--;

            if (CarriableCount == 0)
            {
                m_CurrentType = string.Empty;
            }
        }
    }
}