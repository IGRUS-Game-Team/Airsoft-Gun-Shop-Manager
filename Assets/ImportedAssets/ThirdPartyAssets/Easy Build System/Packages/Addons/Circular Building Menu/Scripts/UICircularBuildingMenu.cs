/// <summary>
/// Project : Easy Build System
/// Class : UICircularBuildingMenu.cs
/// Namespace : EasyBuildSystem.Packages.Addons.CircularBuildingMenu
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;

using EasyBuildSystem.Features.Runtime.Extensions;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Packages.Addons.CircularBuildingMenu
{
    public class UICircularBuildingMenu : MonoBehaviour
    {
        #region Fields

        public static UICircularBuildingMenu Instance;

        public enum PlatformTarget
        {
            STANDALONE,
            GAMEPAD,
            MOBILE,
            XR
        }

        [Serializable]
        public class CircularButtonSettings
        {
            [SerializeField] string m_Name;
            public string Name { get { return m_Name; } set { m_Name = value; } }

            [SerializeField] string m_Description;
            public string Description { get { return m_Description; } set { m_Description = value; } }

            [SerializeField] Texture2D m_Icon;
            public Texture2D Icon { get { return m_Icon; } set { m_Icon = value; } }

            [SerializeField] UnityEvent m_Action;
            public UnityEvent Action { get { return m_Action; } set { m_Action = value; } }

            [SerializeField] BuildingPart m_BuildingPart;
            public BuildingPart BuildingPart { get { return m_BuildingPart; } set { m_BuildingPart = value; } }
        }

        [Serializable]
        public class CircularCategory
        {
            [SerializeField] string m_Name = "New Category";
            public string Name { get { return m_Name; } set { m_Name = value; } }

            [SerializeField] List<CircularButtonSettings> m_Buttons = new List<CircularButtonSettings>();
            public List<CircularButtonSettings> Buttons { get { return m_Buttons; } set { m_Buttons = value; } }

            List<UICircularBuildingMenuButton> m_InstancedButtons = new List<UICircularBuildingMenuButton>();
            public List<UICircularBuildingMenuButton> InstancedButtons { get { return m_InstancedButtons; } set { m_InstancedButtons = value; } }

            [SerializeField] GameObject m_ContentTransform;
            public GameObject ContentTransform { get { return m_ContentTransform; } set { m_ContentTransform = value; } }

            public void Init(Transform transform)
            {
                if (m_ContentTransform == null)
                {
                    GameObject instancedContainer = new GameObject(m_Name);
                    instancedContainer.transform.SetParent(transform, false);
                    m_ContentTransform = instancedContainer;
                }
                else
                {
                    m_ContentTransform = transform.RecursiveFindChild(m_Name).gameObject;
                }
            }
        }

        [SerializeField] int m_DefaultCategory = 0;

        [SerializeField] PlatformTarget m_PlatformTarget;

        [SerializeField] bool m_InputLockCursor = true;

#if !ENABLE_INPUT_SYSTEM
        [SerializeField] KeyCode m_KeyboardToggleKey = KeyCode.Tab;
#endif

        [SerializeField] GameObject[] m_DisableGameObjectsWhenOpen;

        [SerializeField] CanvasGroup m_CanvasGroup;
        public CanvasGroup CanvasGroup { get { return m_CanvasGroup; } }

        [SerializeField] Image m_UICircularSelectionFillImage;
        [SerializeField] Image m_UICircularSelectionCenterFillImage;

        [SerializeField] RawImage m_UICircularSelectionIcon;
        [SerializeField] Text m_UICircularSelectionText;
        [SerializeField] Text m_UICircularSelectionDescription;

        [SerializeField] UICircularBuildingMenuButton m_UICircularButtonPrefab;

        [SerializeField] Color m_UICircularButtonNormalColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] float m_UICircularButtonNormalScale = 1.25f;
        [SerializeField] Color m_UICircularButtonHoverColor = new Color(1, 1, 1, 1);
        [SerializeField] float m_UICircularButtonHoverScale = 1.25f;
        [SerializeField] float m_UICircularButtonSpacing = 160f;

        [SerializeField] List<CircularCategory> m_Categories = new List<CircularCategory>();
        public List<CircularCategory> Categories { get { return m_Categories; } set { m_Categories = value; } }

#if ENABLE_INPUT_SYSTEM
        [SerializeField] InputActionReference m_ToggleInputReference;
        [SerializeField] InputActionReference m_SelectInputReference;
        [SerializeField] InputActionReference m_ValidateInputReference;
        [SerializeField] InputActionReference m_CancelInputReference;
#endif

        GameObject m_SelectedButtonObject;

        int m_DefaultCategoryIndex;

        CircularCategory m_CurrentCategory;

        readonly List<float> m_ButtonsRotation = new List<float>();

        int m_ButtonCount;
        float m_ButtonFill;
        float m_ButtonRotation;

        bool m_IsOpened;
        public bool IsOpened
        {
            get
            {
                return m_IsOpened;
            }
        }

        #endregion

        #region Unity Methods

#if ENABLE_INPUT_SYSTEM
        void OnEnable()
        {
            if (m_ToggleInputReference == null)
                m_ToggleInputReference = InputReferencerFinder.FindReference("Toggle");

            m_ToggleInputReference.action.Enable();

            if (m_SelectInputReference == null)
                m_SelectInputReference = InputReferencerFinder.FindReference("Select");

            m_SelectInputReference.action.Enable();

            if (m_ValidateInputReference == null)
                m_ValidateInputReference = InputReferencerFinder.FindReference("Validate");

            m_ValidateInputReference.action.Enable();

            if (m_CancelInputReference == null)
                m_CancelInputReference = InputReferencerFinder.FindReference("Cancel");

            m_CancelInputReference.action.Enable();

            RefreshMenu();
        }

        void OnDisable()
        {
            m_ToggleInputReference.action.Disable();
            m_SelectInputReference.action.Disable();
            m_ValidateInputReference.action.Disable();
            m_CancelInputReference.action.Disable();
        }

#else
        void OnEnable()
        {
            RefreshMenu();
        }
#endif

        void Awake()
        {
            Instance = this;

            RefreshMenu();
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, m_IsOpened ? 1f : 0f, 10f * Time.deltaTime);
            m_CanvasGroup.blocksRaycasts = m_CanvasGroup.alpha >= 0.9f;

#if ENABLE_INPUT_SYSTEM
            if (m_CancelInputReference.action.triggered)
            {
                BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                CloseMenu();
            }

            if (m_ToggleInputReference.action.triggered)
            {
                if (m_IsOpened)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
#else
            if (m_PlatformTarget == PlatformTarget.STANDALONE)
            {
                if (Input.GetKeyDown(m_KeyboardToggleKey))
                {
                    if (m_IsOpened)
                    {
                        CloseMenu();
                    }
                    else
                    {
                        OpenMenu();
                    }
                }
            }
#endif
            if (m_CanvasGroup.alpha <= 0.9f)
            {
                return;
            }

            if (!m_IsOpened)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (m_PlatformTarget == PlatformTarget.STANDALONE || m_PlatformTarget == PlatformTarget.MOBILE)
            {
                Vector3 boundsScreen = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
                Vector3 relativeBounds = new Vector3(UnityEngine.InputSystem.Mouse.current.position.x.ReadValue(), 
                    UnityEngine.InputSystem.Mouse.current.position.y.ReadValue(), 0f) - boundsScreen;
                m_ButtonRotation = Mathf.Atan2(relativeBounds.x, relativeBounds.y) * 57.29578f;
            }
            else if (m_PlatformTarget == PlatformTarget.GAMEPAD)
            {
                Vector2 inputAxis = m_SelectInputReference.action.ReadValue<Vector2>();

                if (Mathf.Abs(inputAxis.x) > 0.25f || Mathf.Abs(inputAxis.y) > 0.25f)
                {
                    m_ButtonRotation = Mathf.Atan2(inputAxis.x, inputAxis.y) * 57.29578f;
                }
            }
            else if (m_PlatformTarget == PlatformTarget.XR)
            {
                UnityEngine.XR.InputDevice Device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
                Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 InputAxis);

                if (Mathf.Abs(InputAxis.x) > 0.25f || Mathf.Abs(InputAxis.y) > 0.25f)
                {
                    m_ButtonRotation = Mathf.Atan2(InputAxis.x, InputAxis.y) * 57.29578f;
                }
            }
#else

            Vector3 boundsScreen = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
            Vector3 relativeBounds = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f) - boundsScreen;
            m_ButtonRotation = Mathf.Atan2(relativeBounds.x, relativeBounds.y) * 57.29578f;

#endif

            m_UICircularSelectionFillImage.fillAmount = Mathf.Lerp(m_UICircularSelectionFillImage.fillAmount, m_ButtonFill, .2f);
            m_UICircularSelectionCenterFillImage.fillAmount = m_UICircularSelectionFillImage.fillAmount;

            if (m_ButtonRotation < 0f)
            {
                m_ButtonRotation += 360f;
            }

            float average = 9999;
            GameObject nearestButton = null;

            for (int i = 0; i < m_ButtonCount; i++)
            {
                GameObject instancedButton = m_CurrentCategory.InstancedButtons[i].gameObject;
                instancedButton.transform.localScale = Vector3.one;

                float rotationButton = Convert.ToSingle(instancedButton.name);

                if (Mathf.Abs(rotationButton - m_ButtonRotation) < average)
                {
                    nearestButton = instancedButton;
                    average = Mathf.Abs(rotationButton - m_ButtonRotation);
                }
            }

            m_SelectedButtonObject = nearestButton;

            if (m_SelectedButtonObject == null)
            {
                return;
            }

            float cursorRotation = -(Convert.ToSingle(m_SelectedButtonObject.name) - m_UICircularSelectionFillImage.fillAmount * 360f / 2f);
            m_UICircularSelectionFillImage.transform.localRotation =
                Quaternion.Slerp(m_UICircularSelectionFillImage.transform.localRotation, Quaternion.Euler(0, 0, cursorRotation), 15f * Time.deltaTime);

            for (int i = 0; i < m_ButtonCount; i++)
            {
                UICircularBuildingMenuButton elementButton = m_CurrentCategory.InstancedButtons[i].GetComponent<UICircularBuildingMenuButton>();

                if (elementButton.gameObject != m_SelectedButtonObject)
                {
                    elementButton.transform.localScale = Vector3.one * m_UICircularButtonNormalScale;
                    elementButton.UIIconImage.color = m_UICircularButtonNormalColor;
                }
                else
                {
                    elementButton.transform.localScale = Vector3.one * m_UICircularButtonHoverScale;
                    elementButton.UIIconImage.color = m_UICircularButtonHoverColor;
                }
            }

            UICircularBuildingMenuButton selectedButton = m_SelectedButtonObject.GetComponent<UICircularBuildingMenuButton>();

            m_UICircularSelectionIcon.texture = selectedButton.UIIconImage.texture;

            m_UICircularSelectionText.text = selectedButton.UIText;
            m_UICircularSelectionDescription.text = selectedButton.UIDescriptionText;

#if ENABLE_INPUT_SYSTEM
            if (m_PlatformTarget == PlatformTarget.GAMEPAD || m_PlatformTarget == PlatformTarget.XR || m_PlatformTarget == PlatformTarget.MOBILE)
            {
                if (m_ValidateInputReference.action.triggered)
                {
                    if (m_SelectedButtonObject != null)
                    {
                        selectedButton.Action.Invoke();
                    }
                }
            }
            else
            {
                if (m_ValidateInputReference.action.triggered)
                {
                    if (m_SelectedButtonObject != null)
                    {
                        selectedButton.Action.Invoke();
                    }
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (m_SelectedButtonObject != null)
                {
                    selectedButton.Action.Invoke();
                }
            }
#endif
        }

        #endregion

        #region Internal Methods

        public void RefreshMenu()
        {
            if (m_Categories.Count == 0)
            {
                return;
            }

            if (m_Categories.Count <= m_DefaultCategory)
            {
                m_DefaultCategory = 0;
            }

            if (m_DefaultCategory < 0)
            {
                m_DefaultCategory = m_Categories.Count - 1;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                for (int i = 0; i < m_Categories.Count; i++)
                {
                    if (!gameObject.activeSelf) continue;

                    m_Categories[i].Init(transform);

                    if (m_Categories[i].ContentTransform != null)
                    {
                        m_Categories[i].InstancedButtons = new List<UICircularBuildingMenuButton>();

                        UICircularBuildingMenuButton[] cacheButtons = m_Categories[i].ContentTransform.GetComponentsInChildren<UICircularBuildingMenuButton>(true);

                        for (int y = 0; y < cacheButtons.Length; y++)
                        {
                            if (cacheButtons[y].gameObject != null)
                            {
                                if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(transform))
                                {
                                    cacheButtons[y].SetButton(m_Categories[i].Buttons[y]);
                                }
                                else
                                {
                                    DestroyImmediate(cacheButtons[y].gameObject, true);
                                }
                            }
                        }

                        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(transform))
                        {

                        }
                        else if (gameObject.scene.IsValid())
                        {
                            for (int x = 0; x < m_Categories[i].Buttons.Count; x++)
                            {
                                UICircularBuildingMenuButton buttonInstance = Instantiate(m_UICircularButtonPrefab, m_Categories[i].ContentTransform.transform);
                                buttonInstance.gameObject.SetActive(true);

                                ((RectTransform)buttonInstance.transform).sizeDelta = new Vector2(m_UICircularButtonSpacing / 1.5f, m_UICircularButtonSpacing / 1.5f);

                                buttonInstance.gameObject.AddComponent<Image>().color = Color.clear;

                                buttonInstance.SetButton(m_Categories[i].Buttons[x]);
                                m_Categories[i].InstancedButtons.Add(buttonInstance);
                            }
                        }

                        m_Categories[i].InstancedButtons = m_Categories[i].ContentTransform.GetComponentsInChildren<UICircularBuildingMenuButton>(true).ToList();
                    }

                    m_Categories[i].ContentTransform.transform.localPosition = Vector3.zero;
                    m_Categories[i].ContentTransform.transform.localScale = Vector3.one;
                }

                ChangeCategory(m_DefaultCategory);
            };
#else
            for (int i = 0; i < m_Categories.Count; i++)
            {
                m_Categories[i].Init(transform);

                if (m_Categories[i].ContentTransform != null)
                {
                    m_Categories[i].InstancedButtons = new List<UICircularBuildingMenuButton>();

                    UICircularBuildingMenuButton[] cacheButtons = m_Categories[i].ContentTransform.GetComponentsInChildren<UICircularBuildingMenuButton>(true);

                    for (int y = 0; y < cacheButtons.Length; y++)
                    {
                        if (cacheButtons[y].gameObject != null)
                        {
                            cacheButtons[y].SetButton(m_Categories[i].Buttons[y]);
                        }
                    }

                    m_Categories[i].InstancedButtons = m_Categories[i].ContentTransform.GetComponentsInChildren<UICircularBuildingMenuButton>(true).ToList();
                }

                m_Categories[i].ContentTransform.transform.localPosition = Vector3.zero;
                m_Categories[i].ContentTransform.transform.localScale = Vector3.one;
            }

            ChangeCategory(m_DefaultCategory);
#endif
        }

        public void RefreshButtons()
        {
            m_ButtonCount = m_CurrentCategory.InstancedButtons.Count;

            if (m_ButtonCount > 0)
            {
                m_ButtonFill = 1f / (float)m_ButtonCount;

                float fillRadius = m_ButtonFill * 360f;
                float lastRotation = 0;

                for (int i = 0; i < m_ButtonCount; i++)
                {
                    if (m_CurrentCategory.InstancedButtons[i] != null)
                    {
                        GameObject instancedButton = m_CurrentCategory.InstancedButtons[i].gameObject;

                        float rotation = lastRotation + fillRadius / 2;
                        lastRotation = rotation + fillRadius / 2;

                        instancedButton.transform.localPosition = new Vector2(m_UICircularButtonSpacing * Mathf.Cos((rotation - 90) * Mathf.Deg2Rad),
                            -m_UICircularButtonSpacing * Mathf.Sin((rotation - 90) * Mathf.Deg2Rad));

                        instancedButton.transform.localScale = Vector3.one;

                        if (rotation > 360)
                        {
                            rotation -= 360;
                        }

                        instancedButton.name = rotation.ToString();

                        m_ButtonsRotation.Add(rotation);
                    }
                }
            }

            m_UICircularSelectionFillImage.fillAmount = m_ButtonFill;
            m_UICircularSelectionCenterFillImage.fillAmount = m_UICircularSelectionFillImage.fillAmount;
        }

        public void ChangeCategory(int index)
        {
            index = Mathf.Clamp(index, 0, m_Categories.Count - 1);

            m_DefaultCategory = index;
            m_DefaultCategoryIndex = index;

            if (m_DefaultCategoryIndex == -1)
            {
                return;
            }

            m_CurrentCategory = m_Categories[m_DefaultCategoryIndex];

            for (int i = 0; i < m_Categories.Count; i++)
            {
                if (m_Categories[i].ContentTransform != null)
                {
                    if (i != m_DefaultCategoryIndex)
                    {
                        m_Categories[i].ContentTransform.SetActive(false);
                    }
                    else
                    {
                        m_Categories[i].ContentTransform.SetActive(true);
                    }
                }
            }

            RefreshButtons();
        }

        public void SelectBuildingPart(string buildingName)
        {
            BuildingPlacer.Instance.SelectBuildingPart(BuildingManager.Instance.GetBuildingPartByName(buildingName));
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
            CloseMenu();
        }

        public void SelectPlacingMode()
        {
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
            CloseMenu();
        }

        public void SelectDestroyMode()
        {
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
            CloseMenu();
        }

        public void SelectEditingMode()
        {
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
            CloseMenu();
        }

        public void UpgradePart(int index)
        {
            BuildingPart buildingPart = BuildingPlacer.Instance.GetTargetBuildingPart();

            if (buildingPart == null)
            {
                CloseMenu();
                return;
            }

            buildingPart.ChangeModel(index);

            CloseMenu();
        }

        public void OpenMenu()
        {
            if (m_IsOpened)
            {
                CloseMenu();
                return;
            }

            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);

            m_IsOpened = true;

            for (int i = 0; i < m_DisableGameObjectsWhenOpen.Length; i++)
            {
                if (m_DisableGameObjectsWhenOpen[i] != null)
                {
                    m_DisableGameObjectsWhenOpen[i].SetActive(false);
                }
            }

            SetCursorState(false);
        }

        public void CloseMenu()
        {
            m_IsOpened = false;

            for (int i = 0; i < m_DisableGameObjectsWhenOpen.Length; i++)
            {
                if (m_DisableGameObjectsWhenOpen[i] != null)
                {
                    m_DisableGameObjectsWhenOpen[i].SetActive(true);
                }
            }

            SetCursorState(true);
        }

        void SetCursorState(bool newState)
        {
            if (m_PlatformTarget == PlatformTarget.MOBILE)
            {
                return;
            }

            if (!m_InputLockCursor)
            {
                return;
            }

            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !newState;
        }

        #endregion
    }
}