using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SelectionManager.cs 박정민
/// Player에 붙는 스크립트입니다.
/// 오브젝트(ex box, monitor 등)의 선택됨 유무를 판단하는 클래스입니다.
/// </summary>

// public class SelectionManager : MonoBehaviour
// {
//     public List<BlockOutLiner> allBlocks = new List<BlockOutLiner>(); //BlockOutLiner.cs가 적용된 오브젝트들을 담는 리스트입니다.
//     [SerializeField] public float pickupRange = 5f;
//     [SerializeField] public LayerMask pickupLayer;
//     void Start()
//     {
//         BlockOutLiner[] found = FindObjectsOfType<BlockOutLiner>();
//         // 게임 시작하면 맵에 있는 모든 BloutOutLiner가 적용된 오브젝트를 찾습니다.
//         allBlocks.AddRange(found);
//     }

//     void Update()
//     {
//         foreach (BlockOutLiner block in allBlocks)
//         {
//             block.selected = false;
//         }

//         GameObject obj = RaycastDetector.Instance.HitObject;
//         if (obj == null) return;

//         BlockOutLiner hitBlock = obj.GetComponent<BlockOutLiner>();
//         if (hitBlock != null)
//         {
//             hitBlock.selected = true;
//         }
//         // 1. 모든 블록들 selected = false
//         // 2. ray 맞은 애만 true
//         // 3. 1, 2 반복
//         // 인게임에서는 ray맞은애만 true, 다른애들은 false로 유지되지만 실제 로직상에선
//         // ray맞은애도 매프레임 true, false가 반복됨.
//         // Todo - 이 불안정한 로직 구조 변경
//     }
// }

public class SelectionManager : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] float range = 5f;

    void Update()
    {
        GameObject hitObj = RaycastDetector.Instance.HitObject;

        foreach (var block in FindObjectsOfType<BlockOutLiner>())
        {
            block.SetSelected(false);
        }

        if (hitObj != null && hitObj.GetComponentInParent<BlockOutLiner>() is BlockOutLiner blockOutLiner)
        {
            blockOutLiner.SetSelected(true);
        }
    }
}
