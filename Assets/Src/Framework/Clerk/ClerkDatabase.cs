using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Clerk Database", fileName = "ClerkDatabase")]
public class ClerkDatabase : ScriptableObject
{
    public List<ClerkData> clerks = new List<ClerkData>();

    public ClerkData GetById(int id)
    {
        foreach (var c in clerks)
        {
            if (c != null && c.clerkId == id)
                return c;
        }
        return null;
    }
}
