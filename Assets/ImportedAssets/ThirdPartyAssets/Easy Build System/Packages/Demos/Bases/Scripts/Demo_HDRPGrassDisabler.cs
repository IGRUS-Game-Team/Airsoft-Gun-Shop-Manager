using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Demo_HDRPGrassDisabler : MonoBehaviour
{
    void OnEnable()
    {
        if (GraphicsSettings.currentRenderPipeline)
        {
            if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
            {
                GetComponent<Terrain>().detailObjectDensity = 0;
            }
        }
    }
}
