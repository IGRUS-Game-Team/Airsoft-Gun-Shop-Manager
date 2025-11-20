using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    Vector3 spawnPos;
    Quaternion spawnRot;

    void Awake()
    {
        // 게임 시작할 때 현재 위치/회전을 시작 위치로 저장
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        Debug.Log($"[PlayerSpawn] 시작 위치 저장: {spawnPos}");
    }

    public void ResetToSpawn()
    {
        Debug.Log($"[PlayerSpawn] 리셋 호출, 현재 위치: {transform.position}");

        // CharacterController 잠깐 꺼주기 (순간이동 버그 방지)
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (cc != null) cc.enabled = true;

        Debug.Log($"[PlayerSpawn] 리셋 후 위치: {transform.position}");
    }
}