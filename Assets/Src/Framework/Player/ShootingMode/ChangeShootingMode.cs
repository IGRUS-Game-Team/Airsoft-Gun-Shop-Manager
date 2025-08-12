using UnityEngine;

public class ChangeShootingMode : MonoBehaviour
{
    [SerializeField] GameObject Player;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnCollisionEnter(Collision Player)
    {
        Debug.Log("사격을 진행하려면 T키를 누르십시오");
    }
}
