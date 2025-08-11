/// <summary>
/// Project : Easy Build System
/// Class : FractureData.cs
/// Namespace : EasyBuildSystem.Packages.Addons.SurvivalBuildings
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Packages.Addons.SurvivalBuildings
{
    public class FractureData : MonoBehaviour
    {
        [SerializeField, Range(0.05f, 1f)] float m_ShatterMultiplier = 1f;
        [SerializeField] float m_ShatterDepenetrationMaxVelocity = 1f;

        void Awake()
        {
            foreach (Rigidbody child in gameObject.GetComponentsInChildren<Rigidbody>())
            {
                child.maxDepenetrationVelocity = m_ShatterDepenetrationMaxVelocity;
            }

            Transform parentTransform = transform;
            int childCount = parentTransform.childCount;
            int numberToRemove = Mathf.RoundToInt(childCount * m_ShatterMultiplier);

            for (int i = 0; i < numberToRemove; i++)
            {
                int randomChildIndex = Random.Range(0, parentTransform.childCount);
                Transform childToRemove = parentTransform.GetChild(randomChildIndex);
                Destroy(childToRemove.gameObject);
            }
        }
    }
}