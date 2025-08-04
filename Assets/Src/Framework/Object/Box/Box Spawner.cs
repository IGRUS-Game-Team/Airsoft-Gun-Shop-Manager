using UnityEngine;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject deilveryBox;
    [SerializeField] SelectionManager selectionManager;
    Vector3 currentPosition;
    Vector3 spawnPoint;
    float randomDropRange;
    int spawnHeight = 7;

    const string PLAYER = "Player";
    private void Awake()
    {
        currentPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("스포너 닿음");

        if (other.CompareTag(PLAYER))
        {
            BoxDrop();
        }
    }

    private void BoxDrop()
    {
        Debug.Log("소환.");

        randomDropRange = Random.Range(0f, 3f);
        spawnPoint = new Vector3(currentPosition.x + randomDropRange, currentPosition.y + spawnHeight, currentPosition.z);
        var newBlock = Instantiate(deilveryBox, spawnPoint, Quaternion.identity);

        ////스포너에서 박스 아웃라이너를 관리하면 안될 것 같음
        // var blockOutLiner = newBlock.GetComponent<BlockOutLiner>();
        // if (blockOutLiner != null) {
        //     //allBlocks -> 이전 프젝 SelectionManager에 참조한거라 수정 필요함
        //     //selectionManager.allBlocks.Add(newBlock.GetComponent<BlockOutLiner>());
        // }
    }
}
