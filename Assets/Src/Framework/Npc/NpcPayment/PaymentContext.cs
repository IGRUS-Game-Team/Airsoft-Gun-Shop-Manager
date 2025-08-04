// 결제 수단 열거형
public enum PaymentType
{
    Cash,     // 현금
    Card      // 카드
}

// NPC 한 명의 결제 정보를 담는 데이터 구조
public class PaymentContext
{
    public float totalPrice;          // 지불해야 할 총 금액
    public PaymentType method;        // 결제 수단(Cash 또는 Card)
    public NpcController payer;       // 돈을 내는 NPC

    // 추후: 상품 목록·세금·할인 등을 계산해 totalPrice를 갱신
    public void Calculate()
    {
        // TODO: 실제 계산 로직을 넣는다.
        // 예) totalPrice = itemPrice * quantity - discount + tax;
    }
}
