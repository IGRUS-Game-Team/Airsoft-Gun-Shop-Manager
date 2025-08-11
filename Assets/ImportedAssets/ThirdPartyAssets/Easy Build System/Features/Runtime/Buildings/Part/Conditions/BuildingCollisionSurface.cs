/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionSurface.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [ExecuteInEditMode]
    public class BuildingCollisionSurface : MonoBehaviour
    {
        #region Fields

        [SerializeField] string m_Tag;
        public string Tag { get { return m_Tag; } set { m_Tag = value; } }

        #endregion
    }
}