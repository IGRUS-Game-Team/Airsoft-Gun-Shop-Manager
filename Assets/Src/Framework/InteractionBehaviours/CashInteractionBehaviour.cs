using UnityEngine;

/// <summary>
/// 돈 오브젝트 클릭 시 CashRegisterUI에 전달
/// </summary>
public class CashInteractionBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField] private float value;
    [SerializeField] private CashRegisterUI cashUI;

    public void Interact()
    {
        if (cashUI != null)
        {
            cashUI.AddGivenAmount(value);
        }
        else
        {
            Debug.LogWarning("cashUI가 연결되지 않았습니다");
        }
    }
}
