/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacer.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

using Random = UnityEngine.Random;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-placer")]
    public class BuildingPlacer : MonoBehaviour
    {
        #region Fields

        public static BuildingPlacer Instance;

        public enum BuildMode { NONE, PLACE, DESTROY, EDIT }

        BuildMode m_LastBuildMode;
        public BuildMode LastBuildMode { get { return m_LastBuildMode; } }

        BuildMode m_BuildMode;
        public BuildMode GetBuildMode => m_BuildMode;

        [SerializeField] BaseInputHandler m_InputHandler;
        public BaseInputHandler GetInputHandler
        {
            get
            {
                if (m_InputHandler == null)
                {
                    m_InputHandler = GetComponent<BaseInputHandler>();
                }

                return m_InputHandler;
            }

            set { m_InputHandler = value; }
        }

        [Serializable]
        public class RaycastSettings
        {
            public enum RaycastType
            {
                FIRST_PERSON_VIEW,
                THIRD_PERSON_VIEW,
                TOP_DOWN_VIEW
#if EBS_XR
                ,FROM_XR_INTERACTOR
#endif
            }

            [SerializeField] Camera m_Camera;
            public Camera Camera { get { return m_Camera; } set { m_Camera = value; } }

            [SerializeField] RaycastType m_ViewType;
            public RaycastType ViewType { get { return m_ViewType; } set { m_ViewType = value; } }

#if EBS_XR
            [SerializeField] UnityEngine.XR.Interaction.Toolkit.XRRayInteractor m_RaycastFromXRInteractor;
            public UnityEngine.XR.Interaction.Toolkit.XRRayInteractor RaycastFromXRInteractor { get { return m_RaycastFromXRInteractor; } }
#endif

            [SerializeField] Transform m_FromTransform;
            public Transform FromTransform { get { return m_FromTransform; } set { m_FromTransform = value; } }

            [SerializeField] LayerMask m_LayerMask = 1 << 0;
            public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

            [SerializeField] bool m_Through = false;
            public bool Through { get { return m_Through; } set { m_Through = value; } }

            [SerializeField] Vector3 m_OffsetPosition = new Vector3(0, 0, 0);
            public Vector3 OffsetPosition { get { return m_OffsetPosition; } }

            [SerializeField] float m_Distance = 10f;
            public float Distance { get { return m_Distance; } set { m_Distance = value; } }

            [SerializeField] float m_MaxDistance = 0f;
            public float MaxDistance { get { return m_MaxDistance; } }

            public virtual Ray GetRay()
            {
                if (m_Camera == null)
                {
                    Debug.LogWarning("<b>Easy Build System</b> : BuildingPlacer component require a camera to work properly!");
                    return new Ray();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (Event.current == null)
                    {
                        return new Ray();
                    }

                    Event currentEvent = Event.current;
                    Vector2 mousePosition = currentEvent.mousePosition;
                    return UnityEditor.HandleUtility.GUIPointToWorldRay(mousePosition);
                }
#endif

                if (m_ViewType == RaycastType.TOP_DOWN_VIEW)
                {
#if ENABLE_INPUT_SYSTEM
                    return m_Camera.ScreenPointToRay(new Vector3(UnityEngine.InputSystem.Mouse.current.position.ReadValue().x, 
                        UnityEngine.InputSystem.Mouse.current.position.ReadValue().y, 0f) + m_OffsetPosition);
#else
                    return m_Camera.ScreenPointToRay(Input.mousePosition + m_OffsetPosition);
#endif
                }
                else if (m_ViewType == RaycastType.FIRST_PERSON_VIEW)
                {
                    return new Ray(m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f) + m_OffsetPosition), m_Camera.transform.forward);
                }
                else if (m_ViewType == RaycastType.THIRD_PERSON_VIEW)
                {
                    if (m_FromTransform != null)
                    {
                        return new Ray(m_FromTransform.position + m_FromTransform.TransformDirection(m_OffsetPosition),
                            m_Camera.transform.forward);
                    }
                }
#if EBS_XR
                else if (m_ViewType == RaycastType.FROM_XR_INTERACTOR)
                {
                    return new Ray(m_RaycastFromXRInteractor.rayOriginTransform.position, m_RaycastFromXRInteractor.rayOriginTransform.forward);
                }
#endif

                return new Ray();
            }

            Transform _Caster;
            public virtual Transform GetCaster
            {
                get
                {
                    if (!Application.isPlaying)
                    {
                        if (Camera.current != null)
                        {
                            _Caster = Camera.current.transform;
                        }
                    }

                    if (m_ViewType == RaycastType.THIRD_PERSON_VIEW)
                    {
                        _Caster = m_FromTransform;
                    }
                    else
                    {
                        if (Camera.main != null)
                        {
                            _Caster = Camera.main.transform;
                        }
                        else
                        {
                            _Caster = m_Camera.transform;
                        }
                    }

                    return _Caster;
                }
            }

            public float GetRaycastDistance
            {
                get
                {
                    return MaxDistance == 0 ? Distance : MaxDistance;
                }
            }
        }
        [SerializeField] RaycastSettings m_RaycastSettings = new RaycastSettings();
        public RaycastSettings GetRaycastSettings => m_RaycastSettings;

        [Serializable]
        public class SnappingSettings
        {
            public enum DetectionType { OVERLAP, RAYCAST }

            [SerializeField] DetectionType m_Type;
            public DetectionType Type { get { return m_Type; } set { m_Type = value; } }

            [SerializeField] LayerMask m_LayerMask = 1 << 8;
            public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

            [SerializeField] float m_MaxAngle = 20f;
            public float MaxAngles { get { return m_MaxAngle; } set { m_MaxAngle = value; } }
        }
        [SerializeField] SnappingSettings m_SnappingSettings = new SnappingSettings();
        public SnappingSettings GetSnappingSettings { get { return m_SnappingSettings; } }

        [Serializable]
        public class AudioSettings
        {
            [SerializeField] AudioSource m_AudioSource;
            public AudioSource AudioSource { get { return m_AudioSource; } }

            [SerializeField] AudioClip[] m_PlacingAudioClips;
            public AudioClip[] PlacingAudioClips { get { return m_PlacingAudioClips; } }

            [SerializeField] AudioClip[] m_DestroyAudioClips;
            public AudioClip[] DestroyAudioClips { get { return m_DestroyAudioClips; } }

            [SerializeField] AudioClip[] m_EditingAudioClips;
            public AudioClip[] EditingAudioClips { get { return m_EditingAudioClips; } }
        }
        [SerializeField] AudioSettings m_AudioSettings = new AudioSettings();
        public AudioSettings GetAudioSettings { get { return m_AudioSettings; } }

        [SerializeField] BuildingPart m_SelectedBuildingPart;
        public BuildingPart GetSelectedBuildingPart { get { return m_SelectedBuildingPart; } }

        public bool CanPlacing { get { return CheckPlacingCondition(); } }

        public bool CanDestroy { get { return CheckDestroyCondition(); } }

        public bool CanEditing { get { return CheckEditingCondition(); } }

        BuildingPart m_LastPreview;

        BuildingPart m_CurrentPreview;
        public BuildingPart GetCurrentPreview { get { return m_CurrentPreview; } }

        BuildingSocket m_CurrentSocket;
        public BuildingSocket GetCurrentSocket { get {  return m_CurrentSocket; } }

        Vector3 m_CurrentPreviewRotation;
        Vector3 m_CurrentPreviewDefaultScale;
        Vector3 m_LastAllowedPosition;

        Vector3 m_LastPosition;
        Vector3 m_LastRotation;
        Vector3 m_LastScale;

        #region Events

        /// <summary>
        /// Event that is triggered when the build mode is changed.
        /// </summary>
        [Serializable] public class ChangedBuildModeEvent : UnityEvent<BuildMode> { }
        public ChangedBuildModeEvent OnChangedBuildModeEvent = new ChangedBuildModeEvent();

        /// <summary>
        /// Event that is triggered when the Building Part selection is changed.
        /// </summary>
        [Serializable] public class ChangedBuildingPartEvent : UnityEvent<BuildingPart> { }
        public ChangedBuildingPartEvent OnChangedBuildingPartEvent = new ChangedBuildingPartEvent();

        #endregion

        #endregion

        #region Unity Methods

        public virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Start()
        {
            m_SnappingSettings.LayerMask |= (1 << LayerMask.NameToLayer("Socket"));
            
            if (BuildingManager.Instance != null && BuildingManager.Instance.BuildingPartReferences.Count != 0)
            {
                SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[0]);
            }
        }

        public virtual void Update()
        {
            if (m_RaycastSettings.Camera == null)
            {
                m_RaycastSettings.Camera = Camera.current;
            }

            switch (m_BuildMode)
            {
                case BuildMode.PLACE:
                    HandlePlacingMode();
                    break;
                case BuildMode.DESTROY:
                    HandleDestroyMode();
                    break;
                case BuildMode.EDIT:
                    HandleEditingMode();
                    break;
            }

            if (m_BuildMode != BuildMode.NONE)
            {
                HandlePreviewMaterials();
            }
        }

        void Reset()
        {
            if (m_RaycastSettings.Camera == null)
            {
                m_RaycastSettings.Camera = Camera.main;
            }

            m_SnappingSettings.LayerMask |= (1 << LayerMask.NameToLayer("Socket"));
        }

        void OnDisable()
        {
            ChangeBuildMode(BuildMode.NONE);
        }

        void OnDestroy()
        {
            if (m_InputHandler != null)
            {
                if (!Application.isPlaying)
                {
                    DestroyImmediate(m_InputHandler);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Ray ray = m_RaycastSettings.GetRay();
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(ray.origin, ray.direction * m_RaycastSettings.Distance);
        }

        #endregion

        #region Internal Methods

        #region Placing Mode

        /// <summary>
        /// Handles the placing mode, creating a preview and handling snapping and free placement.
        /// </summary>
        void HandlePlacingMode()
        {
            if (!HasPreview())
            {
                CreatePreview(GetSelectedBuildingPart);
            }
            else
            {
                if (GetClosestSocket())
                {
                    HandleSnapping();
                }
                else
                {
                    HandleFree();
                }
            }
        }

        /// <summary>
        /// Handles the snap placement of the building, aligning it with the appropriate snapping points or sockets.
        /// </summary>
        void HandleSnapping()
        {
            BuildingSocket.SnappingPointSettings socketOffset = m_CurrentSocket.GetOffset(m_CurrentPreview);

            if (socketOffset != null)
            {
                m_PreviewIsLerping = 
                    (Vector3.Distance(m_CurrentPreview.transform.position, m_CurrentSocket.transform.TransformPoint(socketOffset.Position)) > 0.001f) ||
                    (Quaternion.Angle(m_CurrentPreview.transform.rotation, m_CurrentSocket.transform.rotation * Quaternion.Euler(socketOffset.Rotation)) > 0.001f);

                m_CurrentSocket.Snap(m_CurrentPreview, socketOffset, m_CurrentPreviewRotation);

                m_CurrentPreview.AttachedBuildingSocket = m_CurrentSocket;

                //if (!CanPlacing)
                //{
                //    HandleFree();
                //}
            }
        }

        bool m_PreviewIsLerping;

        /// <summary>
        /// Handles the free placement mode of the building.
        /// </summary>
        void HandleFree()
        {
            m_CurrentPreview.AttachedBuildingSocket = null;

            Quaternion rotation;

            if (m_CurrentPreview.GetPreviewSettings.LockRotation)
            {
                rotation = Quaternion.Euler(new Vector3(0, m_RaycastSettings.GetCaster.eulerAngles.y, 0)) *
                    GetSelectedBuildingPart.transform.localRotation * Quaternion.Euler(m_CurrentPreviewRotation);
            }
            else
            {
                rotation = Quaternion.Euler(m_CurrentPreviewRotation);
            }

            m_CurrentPreview.transform.rotation = rotation;

            m_CurrentPreview.transform.localScale = m_CurrentPreviewDefaultScale;

            if (Physics.Raycast(m_RaycastSettings.GetRay(), out RaycastHit hit,
                m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask))
            {
                Vector3 nextPosition = GetLookPosition(hit);

                m_PreviewIsLerping =
                    (Vector3.Distance(m_CurrentPreview.transform.position, nextPosition) > 0.1f);

                if (m_CurrentPreview.GetPreviewSettings.CanMovingIfPlaceable)
                {
                    if (m_CurrentPreview.GetPreviewSettings.SmoothMovement)
                    {
                        m_CurrentPreview.transform.position = Vector3.Lerp(m_CurrentPreview.transform.position, nextPosition,
                            m_CurrentPreview.GetPreviewSettings.MovementSmoothTime * Time.deltaTime);
                    }
                    else
                    {
                        m_CurrentPreview.transform.position = nextPosition;
                    }

                    m_CurrentPreview.transform.position = CanPlacing ? m_CurrentPreview.transform.position : m_LastAllowedPosition;
                }
                else
                {
                    if (m_CurrentPreview.GetPreviewSettings.SmoothMovement)
                    {
                        m_CurrentPreview.transform.position = Vector3.Lerp(m_CurrentPreview.transform.position, nextPosition,
                            m_CurrentPreview.GetPreviewSettings.MovementSmoothTime * Time.deltaTime);
                    }
                    else
                    {
                        m_CurrentPreview.transform.position = nextPosition;
                    }
                }

                if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAngle)
                {
                    Quaternion baseRotation = rotation;

                    Vector3 localAxis;
                    switch (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis)
                    {
                        case BuildingPart.PreviewSettings.AxisType.Forward:
                            localAxis = Vector3.forward;
                            break;
                        case BuildingPart.PreviewSettings.AxisType.Backward:
                            localAxis = Vector3.back;
                            break;
                        case BuildingPart.PreviewSettings.AxisType.Left:
                            localAxis = Vector3.left;
                            break;
                        case BuildingPart.PreviewSettings.AxisType.Right:
                            localAxis = Vector3.right;
                            break;
                        case BuildingPart.PreviewSettings.AxisType.Up:
                            localAxis = Vector3.up;
                            break;
                        case BuildingPart.PreviewSettings.AxisType.Down:
                            localAxis = Vector3.down;
                            break;
                        default:
                            localAxis = Vector3.up;
                            break;
                    }

                    Vector3 currentAxisWorld = baseRotation * localAxis;
                    Quaternion alignmentRotation = Quaternion.FromToRotation(currentAxisWorld, hit.normal);

                    m_CurrentPreview.transform.rotation = alignmentRotation * baseRotation;
                }

                if (m_CurrentPreview.GetPreviewSettings.ClampRotation)
                {
                    m_CurrentPreview.transform.eulerAngles = MathExtension.Clamp(m_CurrentPreview.transform.eulerAngles,
                        m_CurrentPreview.GetPreviewSettings.ClampMinRotation, m_CurrentPreview.GetPreviewSettings.ClampMaxRotation);
                }

                return;
            }

            Vector3 lookDistance = GetLookDistance();

            m_PreviewIsLerping =
                (Vector3.Distance(m_CurrentPreview.transform.position, lookDistance) > 0.1f);

            m_CurrentPreview.transform.position = lookDistance + m_CurrentPreview.GetPreviewSettings.OffsetPosition;

            if (m_CurrentPreview.GetPreviewSettings.UseGridSnapping)
            {
                m_CurrentPreview.transform.position = MathExtension.Grid(m_CurrentPreview.GetPreviewSettings.MovementGridSize, lookDistance);
            }
        }

        /// <summary>
        /// Calculates the rotation quaternion based on the specified axis type and surface normal of a hit point.
        /// This method is used to ensure proper alignment and orientation of the placed building part during placement, enhancing the visual accuracy of the preview object.
        /// </summary>
        /// <param name="axisType">The axis type to align the rotation with.</param>
        /// <param name="surfaceNormal">The surface normal vector of the hit point.</param>
        /// <returns>The calculated rotation quaternion.</returns>
        Quaternion GetRotationBasedOnAxis(BuildingPart.PreviewSettings.AxisType axisType, Vector3 hitNormal)
        {
            switch (axisType)
            {
                case BuildingPart.PreviewSettings.AxisType.Forward:
                    return Quaternion.LookRotation(hitNormal);
                case BuildingPart.PreviewSettings.AxisType.Backward:
                    return Quaternion.LookRotation(-hitNormal);
                case BuildingPart.PreviewSettings.AxisType.Left:
                    return Quaternion.LookRotation(-hitNormal, Vector3.up);
                case BuildingPart.PreviewSettings.AxisType.Right:
                    return Quaternion.LookRotation(hitNormal, Vector3.up);
                case BuildingPart.PreviewSettings.AxisType.Up:
                    return Quaternion.LookRotation(Vector3.up, hitNormal);
                case BuildingPart.PreviewSettings.AxisType.Down:
                    return Quaternion.LookRotation(-Vector3.up, hitNormal);
                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Calculates the look position based on the given point.
        /// </summary>
        /// <param name="point">The point to calculate the look position from.</param>
        /// <returns>The calculated look position.</returns>
        Vector3 GetLookPosition(RaycastHit hit)
        {
            Vector3 targetPoint = hit.point + m_CurrentPreview.GetPreviewSettings.OffsetPosition;

            Vector3 clampedPoint = m_CurrentPreview.GetPreviewSettings.ClampPosition ?
                                MathExtension.Clamp(targetPoint, m_CurrentPreview.GetPreviewSettings.ClampMinPosition, m_CurrentPreview.GetPreviewSettings.ClampMaxPosition) :
                                targetPoint;

            if (m_CurrentPreview.GetPreviewSettings.UseGridSnapping)
            {
                float gridSize = m_CurrentPreview.GetPreviewSettings.MovementGridSize;
                float halfHeight = m_CurrentPreview.GetPreviewSettings.MovementGridOffset == 0 ? 0.5f
                    : m_CurrentPreview.GetPreviewSettings.MovementGridOffset;
                Vector3 spawnPos = hit.point + hit.normal * 0.5f;
                spawnPos.x = Mathf.Round(spawnPos.x / gridSize) * gridSize;
                spawnPos.y = Mathf.Floor(spawnPos.y / halfHeight) * halfHeight;
                spawnPos.z = Mathf.Round(spawnPos.z / gridSize) * gridSize;
                clampedPoint = spawnPos;
            }

            return clampedPoint;
        }

        /// <summary>
        /// Calculates the look distance based on the current preview position.
        /// </summary>
        /// <returns>The calculated look distance.</returns>
        Vector3 GetLookDistance()
        {
            Transform startTransform = (m_CurrentPreview.GetPreviewSettings.PreviewElevationHeight == 0) ? m_RaycastSettings.GetCaster : m_RaycastSettings.Camera.transform;
            Vector3 lookDistance = startTransform.position + startTransform.forward * m_RaycastSettings.GetRaycastDistance;

            if (m_CurrentPreview.GetPreviewSettings.PreviewElevation)
            {
                float maxHeight;

                if (Physics.Raycast(m_CurrentPreview.transform.position +
                    new Vector3(0, 0.3f, 0), Vector3.down, out RaycastHit hit, Mathf.Infinity, m_RaycastSettings.LayerMask))
                {
                    float groundHeight = hit.point.y;
                    maxHeight = groundHeight + m_CurrentPreview.GetPreviewSettings.PreviewElevationHeight;
                }
                else
                {
                    maxHeight = m_RaycastSettings.GetCaster.transform.position.y + m_CurrentPreview.GetPreviewSettings.PreviewElevationHeight;
                }

                lookDistance.y = Mathf.Clamp(lookDistance.y, -m_CurrentPreview.GetPreviewSettings.PreviewElevationHeight, maxHeight);
            }
            else
            {
                if (Physics.Raycast(m_CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                    Vector3.down, out RaycastHit hit, Mathf.Infinity, m_RaycastSettings.LayerMask))
                {
                    lookDistance.y = hit.point.y;
                }
            }

            return lookDistance;
        }

        /// <summary>
        /// Checks the conditions for placing the building part.
        /// </summary>
        /// <returns>True if the placement conditions are satisfied, false otherwise.</returns>
        public bool CheckPlacingCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            if (m_CurrentPreview.GetPreviewSettings.SmoothMovement && m_PreviewIsLerping)
            {
                return false;
            }

            Transform caster = m_RaycastSettings.GetCaster;
            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(caster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckPlacingCondition())
            {
                return false;
            }

            m_LastAllowedPosition = m_CurrentPreview.transform.position;

            return true;
        }

        [Serializable]
        public class PlacingBuildingPartOnSocketEvent : UnityEvent<BuildingPart, BuildingSocket> { }
        public PlacingBuildingPartOnSocketEvent OnPlacingBuildingPartOnSocketEvent = new PlacingBuildingPartOnSocketEvent();

        /// <summary>
        /// Places the Building Part.
        /// </summary>
        /// <returns>True if the Building Part was successfully placed, false otherwise.</returns>
        public virtual bool PlacingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanPlacing)
            {
                return false;
            }


            BuildingPart instancedPart = BuildingManager.Instance.PlaceBuildingPart(GetSelectedBuildingPart,
                m_CurrentPreview.transform.position,
                m_CurrentPreview.transform.eulerAngles,
                m_CurrentPreview.transform.localScale);

            if (m_CurrentPreview.AttachedBuildingSocket != null)
            {
                OnPlacingBuildingPartOnSocketEvent?.Invoke(instancedPart, m_CurrentPreview.AttachedBuildingSocket);
            }

            if (m_LastBuildMode == BuildMode.EDIT)
            {
                ChangeBuildMode(BuildMode.EDIT, true);
            }
            else
            {
                CancelPreview();
            }

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.PlacingAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.PlacingAudioClips[Random.Range(0,
                        m_AudioSettings.PlacingAudioClips.Length)]);
                }
            }

            return true;
        }

        #endregion

        #region Destroy Mode

        /// <summary>
        /// Handles the destroy mode.
        /// </summary>
        void HandleDestroyMode()
        {
            m_CurrentPreview = GetTargetBuildingPart();

            if (m_CurrentPreview != null)
            {
                if (m_LastPreview != null)
                {
                    if (m_LastPreview != m_CurrentPreview)
                    {
                        m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                        m_LastPreview = null;
                    }
                }

                m_LastPreview = m_CurrentPreview;
                m_CurrentPreview.ChangeState(BuildingPart.StateType.DESTROY);
            }
            else
            {
                if (m_LastPreview != null)
                {
                    m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                    m_LastPreview = null;
                }
            }
        }

        /// <summary>
        /// Checks the conditions for the destruction.
        /// </summary>
        /// <returns>True if the destruction conditions are met, false otherwise.</returns>
        public bool CheckDestroyCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            Transform caster = m_RaycastSettings.GetCaster;
            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(caster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckDestroyCondition())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Destroys the Building Part.
        /// </summary>
        /// <returns>True if the Building Part is successfully destroyed, false otherwise.</returns>
        public virtual bool DestroyBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanDestroy)
            {
                return false;
            }

            BuildingManager.Instance.DestroyBuildingPart(m_CurrentPreview);

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.DestroyAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.DestroyAudioClips[Random.Range(0,
                        m_AudioSettings.DestroyAudioClips.Length)]);
                }
            }

            return true;
        }

        #endregion

        #region Editing Mode

        /// <summary>
        /// Handle the editing mode.
        /// </summary>
        void HandleEditingMode()
        {
            m_CurrentPreview = GetTargetBuildingPart();

            if (m_CurrentPreview != null)
            {
                if (m_LastPreview != null)
                {
                    if (m_LastPreview != m_CurrentPreview)
                    {
                        m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                        m_LastPreview = null;
                    }
                }

                m_LastPreview = m_CurrentPreview;
                m_CurrentPreview.ChangeState(BuildingPart.StateType.EDIT);
            }
            else
            {
                if (m_LastPreview != null)
                {
                    m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                    m_LastPreview = null;
                }
            }
        }

        /// <summary>
        /// Checks the conditions for the edition.
        /// </summary>
        /// <returns>True if the destruction conditions are met, false otherwise.</returns>
        public bool CheckEditingCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            Transform caster = m_RaycastSettings.GetCaster;
            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(caster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckEditingCondition())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Edit the Building Part.
        /// </summary>
        /// <returns>True if the Building Part is successfully edited, false otherwise.</returns>
        public virtual bool EditingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanEditing)
            {
                return false;
            }

            m_LastPosition = m_CurrentPreview.transform.position;
            m_LastRotation = m_CurrentPreview.transform.eulerAngles;
            m_LastScale = m_CurrentPreview.transform.localScale;

            SelectBuildingPart(m_CurrentPreview);

            if (!m_CurrentPreview.GetPreviewSettings.RotateAccordingAngle)
            {
                m_CurrentPreviewRotation = m_CurrentPreview.transform.eulerAngles;
            }
            
            m_CurrentPreviewDefaultScale = m_CurrentPreview.transform.localScale;

            m_CurrentPreview.ChangeState(BuildingPart.StateType.PREVIEW);

            ChangeBuildMode(BuildMode.PLACE, false);

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.EditingAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.EditingAudioClips[Random.Range(0,
                        m_AudioSettings.EditingAudioClips.Length)]);
                }
            }

            return true;
        }

        /// <summary>
        /// Cance editing of the current Building Part.
        /// </summary>
        public void CancelEditing()
        {
            BuildingManager.Instance.PlaceBuildingPart(GetSelectedBuildingPart,
               m_LastPosition, m_LastRotation, m_LastScale);

            ChangeBuildMode(BuildMode.NONE, true);
        }

        #endregion

        #region Preview

        /// <summary>
        /// Handles the preview materials of the Building Part according to different states such as placement, destruction, and editing.
        /// </summary>
        void HandlePreviewMaterials()
        {
            if (!HasPreview())
            {
                return;
            }

            if (m_BuildMode == BuildMode.PLACE)
            {
                m_CurrentPreview.UpdatePreviewMaterials(CanPlacing ? m_CurrentPreview.GetPreviewSettings.PlacingColor : m_CurrentPreview.GetPreviewSettings.DestroyingColor);
            }
            else if (m_BuildMode == BuildMode.DESTROY)
            {
                m_CurrentPreview.UpdatePreviewMaterials(m_CurrentPreview.GetPreviewSettings.DestroyingColor);
            }
            else if (m_BuildMode == BuildMode.EDIT)
            {
                m_CurrentPreview.UpdatePreviewMaterials(m_CurrentPreview.GetPreviewSettings.EditingColor);
            }
        }

        /// <summary>
        /// Rotate the current preview.
        /// </summary>
        /// <param name="reverse">Whether to rotate the preview in reverse direction.</param>
        public void RotatePreview(bool reverse = false)
        {
            if (!HasPreview())
            {
                return;
            }

            m_CurrentPreviewRotation += reverse ? -m_CurrentPreview.GetPreviewSettings.RotateAxis
                : m_CurrentPreview.GetPreviewSettings.RotateAxis;
        }

        /// <summary>
        /// Create a preview Building Part.
        /// </summary>
        /// <param name="buildingPart">The Building Part to create a preview for.</param>
        /// <returns>The created preview Building Part.</returns>
        public BuildingPart CreatePreview(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return null;
            }

            Vector3 instantiatePoint = Vector3.zero;

            if (Physics.Raycast(m_RaycastSettings.GetRay(), out RaycastHit hit,
                m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask))
            {
                instantiatePoint = hit.point;
            }

            m_CurrentPreview = Instantiate(buildingPart);
            m_CurrentPreview.ChangeState(BuildingPart.StateType.PREVIEW);
            m_CurrentPreview.transform.position = instantiatePoint;

            m_CurrentPreviewDefaultScale = m_CurrentPreview.transform.localScale;

            return m_CurrentPreview;
        }

        /// <summary>
        /// Cancel the current preview.
        /// </summary>
        public void CancelPreview()
        {
            if (m_CurrentPreview == null)
            {
                return;
            }

            RenderExtension.ChangeMaterialColorRecursively(m_CurrentPreview.Renderers.ToArray(), new Color(1, 1, 1, 0.25f),
                    m_CurrentPreview.GetPreviewSettings.IgnoreRenderers.ToArray());

            if (m_CurrentPreview.State == BuildingPart.StateType.PREVIEW)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_CurrentPreview.gameObject);
                }
                else
                {
                    DestroyImmediate(m_CurrentPreview.gameObject);
                }
            }
            else if (m_CurrentPreview.State == BuildingPart.StateType.DESTROY)
            {
                m_CurrentPreview.ChangeState(BuildingPart.StateType.PLACED);
            }
            else if (m_CurrentPreview.State == BuildingPart.StateType.EDIT)
            {
                m_CurrentPreview.ChangeState(BuildingPart.StateType.PLACED);
            }

            if (m_CurrentPreview.GetPreviewSettings.ResetRotation)
            {
                m_CurrentPreviewRotation = Vector3.zero;
            }

            m_CurrentPreview = null;
        }

        /// <summary>
        /// Check if a preview is currently active.
        /// </summary>
        public bool HasPreview()
        {
            return m_CurrentPreview != null;
        }

        #endregion

        #region Socket

        /// <summary>
        /// Get the closest socket to snap to.
        /// </summary>
        /// <returns>True if a socket was found, false otherwise.</returns>
        bool GetClosestSocket()
        {
            if (m_CurrentPreview.TryGetBasicsCondition.IgnoreSocket)
            {
                return false;
            }

            m_CurrentSocket = null;

            Ray ray = m_RaycastSettings.GetRay();

            if (m_SnappingSettings.Type == SnappingSettings.DetectionType.RAYCAST)
            {
                RaycastHit[] hits = new RaycastHit[BuildingManager.Instance.RegisteredBuildingSockets.Count];

                int colliderCount = Physics.RaycastNonAlloc(ray, hits, m_RaycastSettings.Distance, m_SnappingSettings.LayerMask, QueryTriggerInteraction.UseGlobal);

                for (int i = 0; i < colliderCount; i++)
                {
                    if (hits[i].collider.TryGetComponent(out BuildingSocket buildingSocket))
                    {
                        m_CurrentSocket = buildingSocket;
                    }
                }
            }
            else
            {
                Collider[] hitColliders = new Collider[BuildingManager.Instance.RegisteredBuildingSockets.Count];
                float closestAngle = Mathf.Infinity;

                int numColliders = Physics.OverlapSphereNonAlloc(m_RaycastSettings.GetCaster.position,
                    m_RaycastSettings.Distance, hitColliders, m_SnappingSettings.LayerMask, QueryTriggerInteraction.UseGlobal);

                for (int i = 0; i < numColliders; i++)
                {
                    Physics.Linecast(m_RaycastSettings.GetRay().origin, hitColliders[i].ClosestPointOnBounds(m_RaycastSettings.GetRay().origin),
                        out RaycastHit hit, m_RaycastSettings.LayerMask);

                    if (hit.collider != null && hit.collider.GetComponentInParent<BuildingPart>() != null && !m_RaycastSettings.Through)
                    {
                        continue;
                    }

                    if (hitColliders[i].TryGetComponent(out BuildingSocket buildingSocket))
                    {
                        if (buildingSocket.IsEnabled() && buildingSocket.IsFitting(m_CurrentPreview) && CloseFromPosition(buildingSocket.transform.position))
                        {
                            float angle = Vector3.Angle(ray.direction, buildingSocket.transform.position - ray.origin);

                            if (angle < closestAngle && angle < m_SnappingSettings.MaxAngles && !buildingSocket.IsBusy(m_CurrentPreview))
                            {
                                closestAngle = angle;
                                m_CurrentSocket = buildingSocket;
                            }
                        }
                    }
                }
            }

            return m_CurrentSocket != null;
        }

        /// <summary>
        /// Check if a position is close enough for snapping.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is close enough for snapping, false otherwise.</returns>
        bool CloseFromPosition(Vector3 position)
        {
            if (m_RaycastSettings.ViewType == RaycastSettings.RaycastType.TOP_DOWN_VIEW)
            {
                return true;
            }

            return ((position - m_RaycastSettings.GetCaster.position).sqrMagnitude < Mathf.Pow(m_RaycastSettings.Distance, 2));
        }

        #endregion

        /// <summary>
        /// Get the target Building Part based on the closest overlap sphere hit.
        /// </summary>
        /// <returns>The target Building Part if found, null otherwise.</returns>
        public BuildingPart GetTargetBuildingPart()
        {
            Ray ray = m_RaycastSettings.GetRay();

            if (Physics.Raycast(ray, out RaycastHit hit, m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask))
            {
                BuildingPart buildingPart = hit.collider.GetComponentInParent<BuildingPart>();

                if (buildingPart != null)
                {
                    return buildingPart;
                }
            }

            return null;
        }

        /// <summary>
        /// Change the build mode.
        /// </summary>
        /// <param name="mode">The new build mode.</param>
        /// <param name="clearPreview">Whether to clear the current preview or not.</param>
        public void ChangeBuildMode(BuildMode mode, bool clearPreview = true)
        {
            if (clearPreview)
            {
                CancelPreview();
            }

            m_LastBuildMode = m_BuildMode;
            m_BuildMode = mode;

            OnChangedBuildModeEvent?.Invoke(mode);
        }

        /// <summary>
        /// Select a specific Building Part.
        /// </summary>
        /// <param name="part">The Building Part to select.</param>
        public void SelectBuildingPart(BuildingPart part)
        {
            if (part == null)
            {
                return;
            }

            BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(part.GetGeneralSettings.Identifier);
            m_SelectedBuildingPart = buildingPart;

            OnChangedBuildingPartEvent.Invoke(m_SelectedBuildingPart);
        }

        #endregion
    }
}