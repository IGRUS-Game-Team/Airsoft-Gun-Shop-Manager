/// <summary>
/// Project : Easy Build System
/// Class : RenderExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class RenderExtension
    {
        public static void ChangeMaterialColorRecursively(Renderer[] renderers, Color color, Renderer[] ignoreRenderers)
        {
            bool isPlaying = Application.isPlaying;

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && !ignoreRenderers.Contains(renderer))
                {
                    Material[] materials = isPlaying ? renderer.materials : renderer.sharedMaterials;

                    foreach (Material material in materials)
                    {
                        string colorPropertyName = GraphicsSettings.currentRenderPipeline ? "_BaseColor" : "_Color";
                        material.SetColor(colorPropertyName, color);
                    }
                }
            }
        }

        public static void ChangeMaterialRecursively(Renderer[] renderers, Material material, Renderer[] ignoreRenderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer != null && renderer.enabled && !ignoreRenderers.Contains(renderer))
                {
                    Material[] materials = new Material[renderer.sharedMaterials.Length];

                    for (int x = 0; x < materials.Length; x++)
                    {
                        materials[x] = material;
                    }

                    renderer.sharedMaterials = materials;
                }
            }
        }

        public static void ChangeMaterialRecursively(Renderer[] renderers, Dictionary<Renderer, Material[]> materials, Renderer[] ignoreRenderers)
        {
            bool isPlaying = Application.isPlaying;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer != null && renderer.enabled && !ignoreRenderers.Contains(renderer))
                {
                    Material[] copySharedMaterials = isPlaying ? renderer.materials : renderer.sharedMaterials;
                    Material[] newMaterials = materials[renderer];

                    for (int x = 0; x < copySharedMaterials.Length; x++)
                    {
                        copySharedMaterials[x] = newMaterials[x];
                    }

                    if (isPlaying)
                    {
                        renderer.materials = copySharedMaterials;
                    }
                    else
                    {
                        renderer.sharedMaterials = copySharedMaterials;
                    }
                }
            }
        }
    }
}