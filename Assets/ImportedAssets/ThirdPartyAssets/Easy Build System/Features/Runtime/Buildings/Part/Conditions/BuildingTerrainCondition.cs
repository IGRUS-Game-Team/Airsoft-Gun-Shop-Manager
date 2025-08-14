/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Terrain Condition",
        "This condition checks if the Building Part avoids collisions with trees and allows for terrain modifications.\n\n" +
        "You can find more information on the Building Terrain Condition component in the documentation.", 4)]
    public class BuildingTerrainCondition : BuildingCondition
    {
        #region Fields

        class TerrainModificationData
        {
            public int XHeightIndex { get; }

            public int YHeightIndex { get; }

            public int XAlphaIndex { get; }

            public int YAlphaIndex { get; }

            public int XHeightScale { get; }

            public int YHeightScale { get; }

            public int XAlphaScale { get; }

            public int YAlphaScale { get; }

            public int[,] Details { get; }

            public float[,] Heights;

            public float[,,] Clone { get; }

            public float[,,] Original { get; }

            public int Layer { get; }

            public Terrain Terrain { get; }

            public TerrainModificationData(int _xIndex, int _yIndex, int _detailLayer, int[,] _details, Terrain _terrain)
            {
                XHeightIndex = _xIndex;
                YHeightIndex = _yIndex;

                Details = _details;

                Layer = _detailLayer;
                Terrain = _terrain;
            }

            public TerrainModificationData(int _xIndex, int _yIndex, int _xIndex_Alpha, int _yIndex_Alpha,
                float[,] _heights, float[,,] _clone, int xScale_Height, int zScale_Height, int xScale_Alpha, int zScale_Alpha, Terrain _terrain)
            {
                XHeightIndex = _xIndex;
                YHeightIndex = _yIndex;

                XAlphaIndex = _xIndex_Alpha;
                YAlphaIndex = _yIndex_Alpha;

                XHeightScale = xScale_Height;
                YHeightScale = zScale_Height;

                XAlphaScale = xScale_Alpha;
                YAlphaScale = zScale_Alpha;

                Heights = _heights;
                Clone = _clone;

                if (Clone != null)
                {
                    Original = (float[,,])Clone.Clone();
                }

                Terrain = _terrain;
            }
        }

        [SerializeField] bool m_ClearGrassDetails = true;

        [SerializeField] float m_ClearGrassRadius = 2f;

        [SerializeField] bool m_ShowGizmos = true;

        readonly List<TerrainModificationData> m_SavedData = new List<TerrainModificationData>();

        #endregion

        #region Unity Methods

        void Start()
        {
            if (GetBuildingPart != null)
            {
                if (GetBuildingPart.State == BuildingPart.StateType.PLACED)
                {
                    HandleTerrainModifications();
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_ClearGrassRadius);
        }

        #endregion

        #region Internal Methods

        void HandleTerrainModifications()
        {
            if (TerrainManager.Instance == null)
            {
                return;
            }

            RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position + Vector3.up, Vector3.down), m_ClearGrassRadius);

            for (int hitsIndex = 0; hitsIndex < hits.Length; hitsIndex++)
            {
                RaycastHit hit = hits[hitsIndex];

                Terrain terrain = hit.transform.GetComponent<Terrain>();

                if (terrain != null)
                {
                    if (m_ClearGrassDetails)
                    {
                        ClearGrassDetails(terrain);
                    }

                    break;
                }
            }
        }

        void ClearGrassDetails(Terrain terrain)
        {
            TerrainData terrainData = terrain.terrainData;

            Vector3 multiplyScale = transform.lossyScale;

            int normalizedRes = Mathf.FloorToInt(terrainData.detailResolution / terrainData.size.x);

            int actualXScale = Mathf.CeilToInt((m_ClearGrassRadius * terrainData.detailResolution * multiplyScale.x) / terrainData.size.x) + normalizedRes;
            int actualZScale = Mathf.CeilToInt((m_ClearGrassRadius * terrainData.detailResolution * multiplyScale.z) / terrainData.size.z) + normalizedRes;

            Vector3 terrainPoint = transform.position - (multiplyScale * 1.5f);

            Vector3 terrainLocalPosition = terrainPoint - terrain.transform.position;

            Vector3 normalizedPos = new Vector3(Mathf.InverseLerp(0, terrainData.size.x, terrainLocalPosition.x),
                                                Mathf.InverseLerp(0, terrainData.size.y, terrainLocalPosition.y),
                                                Mathf.InverseLerp(0, terrainData.size.z, terrainLocalPosition.z));

            int xBase = (int)(normalizedPos.x * terrainData.detailResolution);
            int zBase = (int)(normalizedPos.z * terrainData.detailResolution);

            for (int detailIndex = 0; detailIndex < terrainData.detailPrototypes.Length; detailIndex++)
            {
                TerrainManager.Instance.AddDetailsModifications(terrain,
                    terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailIndex), detailIndex);

                m_SavedData.Add(new TerrainModificationData(xBase, zBase, detailIndex,
                    terrainData.GetDetailLayer(xBase, zBase, actualXScale, actualZScale, detailIndex), terrain));
            }

            int[,] _details;

            TerrainModificationData data;

            for (int i = 0; i < m_SavedData.Count; i++)
            {
                data = m_SavedData[i];

                _details = (int[,])data.Details.Clone();

                for (int x = 0; x < actualXScale; x++)
                {
                    for (int z = 0; z < actualZScale; z++)
                    {
                        _details[x, z] = 0;
                    }
                }

                terrainData.SetDetailLayer(data.XHeightIndex, data.YHeightIndex, data.Layer, _details);

                terrain.Flush();
            }
        }

        #endregion
    }
}
