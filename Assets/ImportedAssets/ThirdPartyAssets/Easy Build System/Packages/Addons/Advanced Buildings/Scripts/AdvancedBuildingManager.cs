using System;
using System.Collections.Generic;

using UnityEngine;

public class AdvancedBuildingManager : MonoBehaviour
{
    public static AdvancedBuildingManager Instance;

    [Serializable]
    public class ResourceData
    {
        public string Name;
        public Sprite Icon;
    }

    [SerializeField] List<ResourceData> m_ResourceTypes = new List<ResourceData>();
    public List<ResourceData> ResourceTypes { get { return m_ResourceTypes; } }

    void Awake()
    {
        Instance = this;
    }

    public ResourceData GetResource(string resourceType)
    {
        for (int i = 0; i < m_ResourceTypes.Count; i++)
        {
            if (m_ResourceTypes[i].Name == resourceType)
            {
                return m_ResourceTypes[i];
            }
        }

        return null;
    }
}