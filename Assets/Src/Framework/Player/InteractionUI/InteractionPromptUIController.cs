using System.Collections.Generic;
using UnityEngine;

public class InteractionPromptUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private PromptEntryUI entryPrefab;
    [SerializeField] private RectTransform contentRoot;

    [Header("Refs")]
    [SerializeField] private PlayerInteractionRouter router;

    private readonly List<PromptEntryUI> pool = new();

    private void Awake()
    {
        if (router == null) router = FindFirstObjectByType<PlayerInteractionRouter>();
        if (router != null) router.OnPromptsChanged += Render;
    }

    private void OnDestroy()
    {
        if (router != null) router.OnPromptsChanged -= Render;
    }

    private void Render(List<InteractionPrompt> prompts)
    {
        if (root != null) root.SetActive(prompts != null && prompts.Count > 0);

        EnsurePool(prompts != null ? prompts.Count : 0);

        for (int i = 0; i < pool.Count; i++)
        {
            bool active = (prompts != null && i < prompts.Count);
            pool[i].gameObject.SetActive(active);
            if (!active) continue;

            pool[i].Setup(prompts[i]);
        }
    }

    private void EnsurePool(int need)
    {
        while (pool.Count < need)
        {
            var ui = Instantiate(entryPrefab, contentRoot);
            pool.Add(ui);
        }
    }
}
