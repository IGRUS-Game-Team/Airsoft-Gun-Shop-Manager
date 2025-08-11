using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding.UI
{
    public class BuildableObjectUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_CanvasGroup;

        [SerializeField] RectTransform m_RectTransform;

        [SerializeField] ResourceSlotUI m_Slot;
        public ResourceSlotUI Slot { get { return m_Slot; } }

        [SerializeField] RectTransform m_Container;

        [SerializeField] Vector3 m_CustomItemOffset;

        List<string> m_InstantiatedResource = new List<string>();

        void Update()
        {
            if (InteractionController.Instance.Interactable == null)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            if (InteractionController.Instance.Interactable.InteractableType != InteractableType.BUILDABLE)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            BuildableObject carriableObject = (BuildableObject)InteractionController.Instance.Interactable;

            List<BuildableObject.RequiredResource> slots = carriableObject.RequiredResources.ToList();

            for (int i = 0; i < m_Container.childCount; i++)
            {
                Destroy(m_Container.GetChild(i).gameObject);
            }

            m_InstantiatedResource.Clear();

            for (int i = 0; i < slots.Count; i++)
            {
                AdvancedBuildingManager.ResourceData resourceData = AdvancedBuildingManager.Instance.GetResource(slots[i].ResourceType);

                if (resourceData == null)
                    continue;

                if (m_InstantiatedResource.Contains(resourceData.Name))
                    continue;

                if (resourceData != null)
                {
                    ResourceSlotUI slot = Instantiate(Slot, m_Container);

                    slot.gameObject.SetActive(true);

                    slot.Icon.sprite = resourceData.Icon;

                    int currentAmount = carriableObject.GetCurrentResourceCount(slots[i].ResourceType.ToString());
                    int requiredAmount = carriableObject.GetResourceCount(slots[i].ResourceType.ToString());

                    slot.Text.text = currentAmount + "/" + requiredAmount;

                    m_InstantiatedResource.Add(resourceData.Name);
                }
            }

            m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 1f, 15f * Time.deltaTime);

            if (carriableObject == null)
            {
                return;
            }

            float boundsMedian = GetMedian(carriableObject.MeshBounds.extents.x, carriableObject.MeshBounds.extents.y, carriableObject.MeshBounds.extents.z);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(carriableObject.MeshBounds.center + Camera.main.transform.right * boundsMedian);
            PositionAtScreenPoint(m_RectTransform, screenPosition + m_CustomItemOffset);
        }

        float GetMedian(params float[] values)
        {
            float sum = 0f;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum / values.Length;
        }

        void PositionAtScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, screenPosition, null, out Vector2 position))
                rectTransform.anchoredPosition = position;
        }
    }
}