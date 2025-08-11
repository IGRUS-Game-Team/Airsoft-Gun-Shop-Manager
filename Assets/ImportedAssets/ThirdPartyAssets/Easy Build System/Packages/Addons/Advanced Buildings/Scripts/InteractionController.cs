using System;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Bases;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    public class InteractionController : MonoBehaviour
    {
        public static InteractionController Instance;

        [Header("Interaction Settings")]

#if ENABLE_INPUT_SYSTEM
        [SerializeField] UnityEngine.InputSystem.Key m_InputInteractionKey = UnityEngine.InputSystem.Key.E;
#else
        [SerializeField] KeyCode m_InteractionKey = KeyCode.F;
#endif
        [SerializeField] float m_InteractionRange = 2f;
        [SerializeField] float m_InteractionAngleThreshold = 45f;
        [SerializeField] LayerMask m_InteractionLayer;

        IInteractable m_Interactable;

        public IInteractable Interactable { get { return m_Interactable; } }

        Collider m_LastHitCollider;

        public class InteractedEvent : UnityEvent<IInteractable> { }
        public InteractedEvent OnInteractedEvent = new InteractedEvent();

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            CheckForInteractables();
            HandleInput();
        }

        void CheckForInteractables()
        {
            m_Interactable = null;

            if (m_LastHitCollider != null && Vector3.Distance(transform.position, m_LastHitCollider.transform.position) > m_InteractionRange)
            {
                m_LastHitCollider = null;
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_InteractionRange, m_InteractionLayer);

            float closestAngle = Mathf.Infinity;

            foreach (Collider hitCollider in hitColliders)
            {
                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    Vector3 direction = ((Component)interactable).transform.position - transform.position;
                    float angle = Vector3.Angle(direction, transform.forward);

                    if (angle < closestAngle && angle < m_InteractionAngleThreshold)
                    {
                        m_Interactable = interactable;
                        m_LastHitCollider = hitCollider;
                        closestAngle = angle;
                    }
                }
            }
        }

        void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (m_Interactable != null && UnityEngine.InputSystem.Keyboard.current[m_InputInteractionKey].wasPressedThisFrame)
            {
                OnInteractedEvent.Invoke(m_Interactable);
            }
#else
            if (m_Interactable != null && Input.GetKeyDown(m_InteractionKey))
            {
                OnInteractedEvent.Invoke(m_Interactable);
            }
#endif
        }
    }
}