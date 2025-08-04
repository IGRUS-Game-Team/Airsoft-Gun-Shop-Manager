using UnityEngine;


///<summary>
/// 장지원 8.3 버튼들의 기능을 구분하기 위해 enum을 사용하여 작성
/// 계산기 타입 구분
/// </summary>
public enum CalculatorButtonType
{
    Number, // 0~9 숫자버튼
    Dot, // 소수점
    Clear, //c : 초기화 버튼
    OK // 확인 버튼

}
