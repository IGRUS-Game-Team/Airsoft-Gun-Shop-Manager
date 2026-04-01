using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Clerk Data")]
public class ClerkData : ScriptableObject
{
    public int clerkId;
    public string clerkName;
    public Sprite icon;
    public float hiringCost;
    public GameObject npcPrefab;
}
