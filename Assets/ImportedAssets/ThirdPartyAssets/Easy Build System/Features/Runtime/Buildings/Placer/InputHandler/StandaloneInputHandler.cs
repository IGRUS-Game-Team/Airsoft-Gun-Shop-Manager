/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacerInput.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
{
    public class StandaloneInputHandler : BaseInputHandler
    {
#if ENABLE_INPUT_SYSTEM

        #region Fields

        [Serializable]
        public class InputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] InputActionReference m_ValidateInputReference;
            public InputActionReference ValidateInputReference { get { return m_ValidateInputReference; } set { m_ValidateInputReference = value; } }

            [SerializeField] InputActionReference m_CancelInputReference;
            public InputActionReference CancelInputReference { get { return m_CancelInputReference; } set { m_CancelInputReference = value; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] InputActionReference m_RotateModeInputReference;
            public InputActionReference RotateModeInputReference { get { return m_RotateModeInputReference; } set { m_RotateModeInputReference = value; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] bool m_CanSelectBuildingPartEvenPlacement = true;
            public bool CanSelectBuildingPartEvenPlacement { get { return m_CanSelectBuildingPartEvenPlacement; } set { m_CanSelectBuildingPartEvenPlacement = value; } }

            [SerializeField] InputActionReference m_SelectInputReference;
            public InputActionReference SelectInputReference { get { return m_SelectInputReference; } set { m_SelectInputReference = value; } }

            [SerializeField] bool m_UsePlacingModeShortcut = true;
            public bool UsePlacingModeShortcut { get { return m_UsePlacingModeShortcut; } set { m_UsePlacingModeShortcut = value; } }

            [SerializeField] InputActionReference m_PlacingModeInputReference;
            public InputActionReference PlacingModeInputReference { get { return m_PlacingModeInputReference; } set { m_PlacingModeInputReference = value; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_UseEditingModeShortcut = true;
            public bool UseEditingModeShortcut { get { return m_UseEditingModeShortcut; } set { m_UseEditingModeShortcut = value; } }

            [SerializeField] InputActionReference m_EditingModeInputReference;
            public InputActionReference EditingModeInputReference { get { return m_EditingModeInputReference; } set { m_EditingModeInputReference = value; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_UseDestroyingModeShortcut = true;
            public bool UseDestroyingModeShortcut { get { return m_UseDestroyingModeShortcut; } set { m_UseDestroyingModeShortcut = value; } }

            [SerializeField] InputActionReference m_DestroyingModeInputReference;
            public InputActionReference DestroyingModeInputReference { get { return m_DestroyingModeInputReference; } set { m_DestroyingModeInputReference = value; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }

        [SerializeField] InputSettings m_InputSettings;

        public InputSettings GetInputSettings { get { return m_InputSettings; } set { m_InputSettings = value; } }

        int m_SelectionIndex;

        #endregion

        #region Unity Methods

        void Reset()
        {
#if UNITY_EDITOR
            UnityEditor.Presets.Preset preset = Resources.Load<UnityEditor.Presets.Preset>("Default_IH_Settings");

            if (preset != null && preset.CanBeAppliedTo(Camera.main.gameObject.GetComponent<StandaloneInputHandler>()))
            {
                preset.ApplyTo(Camera.main.gameObject.GetComponent<StandaloneInputHandler>());
            }
#endif
        }

        void OnEnable()
        {
            if (m_InputSettings.ValidateInputReference == null)
                m_InputSettings.ValidateInputReference = InputReferencerFinder.FindReference("Validate");

            if (m_InputSettings.ValidateInputReference != null)
            {
                m_InputSettings.ValidateInputReference.action.Enable();
            }

            if (m_InputSettings.CancelInputReference == null)
                m_InputSettings.CancelInputReference = InputReferencerFinder.FindReference("Cancel");

            if (m_InputSettings.CancelInputReference != null)
            {
                m_InputSettings.CancelInputReference.action.Enable();
            }

            if (m_InputSettings.SelectInputReference == null)
                m_InputSettings.SelectInputReference = InputReferencerFinder.FindReference("Select");

            if (m_InputSettings.SelectInputReference != null)
            {
                m_InputSettings.SelectInputReference.action.Enable();
            }

            if (m_InputSettings.RotateModeInputReference == null)
                m_InputSettings.RotateModeInputReference = InputReferencerFinder.FindReference("Rotate");

            if (m_InputSettings.RotateModeInputReference != null)
            {
                m_InputSettings.RotateModeInputReference.action.Enable();
            }

            if (m_InputSettings.PlacingModeInputReference == null)
                m_InputSettings.PlacingModeInputReference = InputReferencerFinder.FindReference("Place Mode");

            if (m_InputSettings.PlacingModeInputReference != null)
            {
                m_InputSettings.PlacingModeInputReference.action.Enable();
            }

            if (m_InputSettings.EditingModeInputReference == null)
                m_InputSettings.EditingModeInputReference = InputReferencerFinder.FindReference("Edit Mode");

            if (m_InputSettings.EditingModeInputReference != null)
            {
                m_InputSettings.EditingModeInputReference.action.Enable();
            }

            if (m_InputSettings.DestroyingModeInputReference == null)
                m_InputSettings.DestroyingModeInputReference = InputReferencerFinder.FindReference("Destruction Mode");

            if (m_InputSettings.DestroyingModeInputReference != null)
            {
                m_InputSettings.DestroyingModeInputReference.action.Enable();
            }
        }

        void OnDestroy()
        {
            if (m_InputSettings.ValidateInputReference != null)
            {
                m_InputSettings.ValidateInputReference.action.Disable();
            }

            if (m_InputSettings.CancelInputReference != null)
            {
                m_InputSettings.CancelInputReference.action.Disable();
            }

            if (m_InputSettings.SelectInputReference != null)
            {
                m_InputSettings.SelectInputReference.action.Disable();
            }

            if (m_InputSettings.RotateModeInputReference != null)
            {
                m_InputSettings.RotateModeInputReference.action.Disable();
            }

            if (m_InputSettings.PlacingModeInputReference != null)
            {
                m_InputSettings.PlacingModeInputReference.action.Disable();
            }

            if (m_InputSettings.EditingModeInputReference != null)
            {
                m_InputSettings.EditingModeInputReference.action.Disable();
            }

            if (m_InputSettings.DestroyingModeInputReference != null)
            {
                m_InputSettings.DestroyingModeInputReference.action.Disable();
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_InputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            HandleBuildModes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes.
        /// </summary>
        void HandleBuildModes()
        {
            if (m_InputSettings.UsePlacingModeShortcut)
            {
                if (m_InputSettings.PlacingModeInputReference.action.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }
            }

            if (m_InputSettings.UseDestroyingModeShortcut)
            {
                if (m_InputSettings.DestroyingModeInputReference.action.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }
            }

            if (m_InputSettings.UseEditingModeShortcut)
            {
                if (m_InputSettings.EditingModeInputReference.action.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                HandleBuildingPartSelection();
            }
            else
            {
                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        HandleBuildingPartSelection();
                    }

                    HandlePlacingMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
                {
                    HandleDestroyMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    HandleEditingMode();
                }
            }
        }

        /// <summary>
        /// Handle placing mode according to the user inputs.
        /// </summary>
        void HandlePlacingMode()
        {
            if (m_InputSettings.ValidateInputReference.action.triggered)
            {
                if (Placer.PlacingBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_InputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CanRotateBuildingPart)
            {
                if (m_InputSettings.RotateModeInputReference.action.triggered)
                {
                    Placer.RotatePreview(true);
                }
            }

            if (m_InputSettings.CancelInputReference.action.triggered)
            {
                if (Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    Placer.CancelEditing();
                }
                else
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                }
            }
        }

        /// <summary>
        /// Handle destroy mode according to the user inputs.
        /// </summary>
        void HandleDestroyMode()
        {
            if (m_InputSettings.ValidateInputReference.action.triggered)
            {
                if (Placer.DestroyBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CancelInputReference.action.triggered)
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle editing mode according the user inputs.
        /// </summary>
        void HandleEditingMode()
        {
            if (m_InputSettings.ValidateInputReference.action.triggered)
            {
                Placer.EditingBuildingPart();
            }

            if (m_InputSettings.CancelInputReference.action.triggered)
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the building part selection according the user inputs.
        /// </summary>
        void HandleBuildingPartSelection()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }

            if (m_InputSettings.CanSelectBuildingPart)
            {
                float wheelAxis = m_InputSettings.SelectInputReference.action.ReadValue<float>();

                BuildingPart[] buildingParts = BuildingManager.Instance.BuildingPartReferences.ToArray();

                if (wheelAxis > 0)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        Placer.CancelPreview();
                    }

                    if (m_SelectionIndex < buildingParts.Length - 1)
                    {
                        m_SelectionIndex++;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = 0;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
                else if (wheelAxis < 0)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        Placer.CancelPreview();
                    }

                    if (m_SelectionIndex > 0)
                    {
                        m_SelectionIndex--;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = buildingParts.Length - 1;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
            }
        }

        #endregion

#elif EBS_REWIRED

        #region Fields

        [Serializable]
        public class InputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] string m_ValidateActionName = "Validate";
            public string ValidateActionName { get { return m_ValidateActionName; } }

            [SerializeField] string m_CancelActionName = "Cancel";
            public string CancelActionName { get { return m_CancelActionName; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] string m_RotateActionName = "Rotate";
            public string RotateActionName { get { return m_RotateActionName; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] bool m_CanSelectBuildingPartEvenPlacement = true;
            public bool CanSelectBuildingPartEvenPlacement { get { return m_CanSelectBuildingPartEvenPlacement; } }

            [SerializeField] string m_SelectActionName = "Select";
            public string SelectActionName { get { return m_SelectActionName; } }

            [SerializeField] bool m_UsePlacingModeShortcut = true;
            public bool UsePlacingModeShortcut { get { return m_UsePlacingModeShortcut; } set { m_UsePlacingModeShortcut = value; } }

            [SerializeField] string m_PlaceModeActionName = "Place Mode";
            public string PlaceModeActionName { get { return m_PlaceModeActionName; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_UseEditingModeShortcut = true;
            public bool UseEditingModeShortcut { get { return m_UseEditingModeShortcut; } set { m_UseEditingModeShortcut = value; } }

            [SerializeField] string m_EditModeActionName = "Edit Mode";
            public string EditModeActionName { get { return m_EditModeActionName; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_UseDestroyingModeShortcut = true;
            public bool UseDestroyingModeShortcut { get { return m_UseDestroyingModeShortcut; } set { m_UseDestroyingModeShortcut = value; } }

            [SerializeField] string m_DestructionModeActionName = "Destruction Mode";
            public string DestructionModeActionName { get { return m_DestructionModeActionName; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }
        [SerializeField] InputSettings m_InputSettings = new InputSettings();
        public InputSettings GetInputSettings { get { return m_InputSettings; } set { m_InputSettings = value; } }

        int m_SelectionIndex;

        private Rewired.Player player;

        public const string REWIRED_PROFILE_NAME = "Easy Build System";

        #endregion

        #region Unity Methods

        void Awake()
        {
            player = Rewired.ReInput.players.GetPlayer(REWIRED_PROFILE_NAME);
        }

        void Update()
        {
            if (Placer == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_InputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            HandleBuildModes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes accordingly.
        /// </summary>
        void HandleBuildModes()
        {
            if (m_InputSettings.UsePlacingModeShortcut)
            {
                if (player.GetButtonDown(m_InputSettings.PlaceModeActionName))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }
            }

            if (m_InputSettings.UseDestroyingModeShortcut)
            {
                if (player.GetButtonDown(m_InputSettings.DestructionModeActionName))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }
            }

            if (m_InputSettings.UseEditingModeShortcut)
            {
                if (player.GetButtonDown(m_InputSettings.EditModeActionName))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                HandleBuildingPartSelection();
            }
            else
            {
                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        HandleBuildingPartSelection();
                    }

                    HandlePlacingMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
                {
                    HandleDestroyMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    HandleEditingMode();
                }
            }
        }

        /// <summary>
        /// Handle the placing mode based on user inputs.
        /// </summary>
        void HandlePlacingMode()
        {
            if (player.GetButtonDown(m_InputSettings.ValidateActionName))
            {
                if (Placer.PlacingBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_InputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CanRotateBuildingPart)
            {
                if (player.GetButtonDown(m_InputSettings.RotateActionName))
                {
                    Placer.RotatePreview(true);
                }
            }

            if (player.GetButtonDown(m_InputSettings.CancelActionName))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the destroy mode based on user inputs.
        /// </summary>
        void HandleDestroyMode()
        {
            if (player.GetButtonDown(m_InputSettings.ValidateActionName))
            {
                if (Placer.DestroyBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (player.GetButtonDown(m_InputSettings.CancelActionName))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the editing mode based on user inputs.
        /// </summary>
        void HandleEditingMode()
        {
            if (player.GetButtonDown(m_InputSettings.ValidateActionName))
            {
                Placer.EditingBuildingPart();
            }

            if (player.GetButtonDown(m_InputSettings.CancelActionName))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the building part selection based on user inputs.
        /// </summary>
        void HandleBuildingPartSelection()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }

            if (m_InputSettings.CanSelectBuildingPart)
            {
                float wheelAxis = player.GetAxis(m_InputSettings.SelectActionName);

                BuildingPart[] buildingParts = BuildingManager.Instance.BuildingPartReferences.ToArray();

                if (wheelAxis > 0)
                {
                    if (m_SelectionIndex < buildingParts.Length - 1)
                    {
                        m_SelectionIndex++;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = 0;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
                else if (wheelAxis < 0)
                {
                    if (m_SelectionIndex > 0)
                    {
                        m_SelectionIndex--;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = buildingParts.Length - 1;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
            }
        }

        #endregion

#else
        #region Fields

        [Serializable]
        public class InputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] KeyCode m_RotateActionKey = KeyCode.V;
            public KeyCode RotateActionKey { get { return m_RotateActionKey; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] bool m_CanSelectBuildingPartEvenPlacement = true;
            public bool CanSelectBuildingPartEvenPlacement { get { return m_CanSelectBuildingPartEvenPlacement; } set { m_CanSelectBuildingPartEvenPlacement = value; } }

            [SerializeField] KeyCode m_ValidateActionKey = KeyCode.Mouse0;
            public KeyCode ValidateActionKey { get { return m_ValidateActionKey; } }

            [SerializeField] KeyCode m_CancelActionKey = KeyCode.Mouse1;
            public KeyCode CancelActionKey { get { return m_CancelActionKey; } }

            [SerializeField] bool m_UsePlacingModeShortcut = true;
            public bool UsePlacingModeShortcut { get { return m_UsePlacingModeShortcut; } set { m_UsePlacingModeShortcut = value; } }

            [SerializeField] KeyCode m_PlacingModeKey = KeyCode.E;
            public KeyCode PlacingModeKey { get { return m_PlacingModeKey; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_UseEditingModeShortcut = true;
            public bool UseEditingModeShortcut { get { return m_UseEditingModeShortcut; } set { m_UseEditingModeShortcut = value; } }

            [SerializeField] KeyCode m_EditingModeKey = KeyCode.T;
            public KeyCode EditingModeKey { get { return m_EditingModeKey; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_UseDestroyingModeShortcut = true;
            public bool UseDestroyingModeShortcut { get { return m_UseDestroyingModeShortcut; } set { m_UseDestroyingModeShortcut = value; } }

            [SerializeField] KeyCode m_DestroyingModeKey = KeyCode.R;
            public KeyCode DestroyingModeKey { get { return m_DestroyingModeKey; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }
        [SerializeField] InputSettings m_InputSettings = new InputSettings();
        public InputSettings GetInputSettings { get { return m_InputSettings; } set { m_InputSettings = value; } }

        int m_SelectionIndex;

        #endregion

        #region Unity Methods

        void Update()
        {
            if (Placer == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_InputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            HandleBuildModes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes accordingly.
        /// </summary>
        void HandleBuildModes()
        {
            if (m_InputSettings.UsePlacingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.PlacingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }
            }

            if (m_InputSettings.UseDestroyingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.DestroyingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }
            }

            if (m_InputSettings.UseEditingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.EditingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                HandleBuildingPartSelection();
            }
            else
            {
                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        HandleBuildingPartSelection();
                    }

                    HandlePlacingMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
                {
                    HandleDestroyMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    HandleEditingMode();
                }
            }
        }

        /// <summary>
        /// Handle the placing mode based on user inputs.
        /// </summary>
        void HandlePlacingMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                if (Placer.PlacingBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_InputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CanRotateBuildingPart)
            {
                if (Input.GetKeyDown(m_InputSettings.RotateActionKey))
                {
                    Placer.RotatePreview(true);
                }
            }

            if (Input.GetKeyDown(m_InputSettings.CancelActionKey))
            {
                if (Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    Placer.CancelEditing();
                }
                else
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                }
            }
        }

        /// <summary>
        /// Handle the destroy mode based on user inputs.
        /// </summary>
        void HandleDestroyMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                if (Placer.DestroyBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (Input.GetKeyDown(m_InputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the editing mode based on user inputs.
        /// </summary>
        void HandleEditingMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                Placer.EditingBuildingPart();
            }
        }

        /// <summary>
        /// Handle the building part selection based on user inputs.
        /// </summary>
        void HandleBuildingPartSelection()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }

            if (m_InputSettings.CanSelectBuildingPart)
            {
                float wheelAxis = Input.GetAxis("Mouse ScrollWheel");

                BuildingPart[] buildingParts = BuildingManager.Instance.BuildingPartReferences.ToArray();

                if (wheelAxis > 0)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        Placer.CancelPreview();
                    }

                    if (m_SelectionIndex < buildingParts.Length - 1)
                    {
                        m_SelectionIndex++;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = 0;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
                else if (wheelAxis < 0)
                {
                    if (m_InputSettings.CanSelectBuildingPartEvenPlacement)
                    {
                        Placer.CancelPreview();
                    }

                    if (m_SelectionIndex > 0)
                    {
                        m_SelectionIndex--;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = buildingParts.Length - 1;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
            }
        }

        #endregion
#endif
    }
}