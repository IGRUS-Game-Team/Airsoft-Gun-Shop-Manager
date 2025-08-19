using System;
using UnityEngine;
using UnityEngine.UI;

public class NewsScreenManager : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] GameObject TvScreen;
    [SerializeField] Image newsImage; // 뉴스 이미지를 표시할 Image 컴포넌트
    
    [Header("이벤트별 이미지들(Prefabs -> Object -> TvScreen에 있다)")]
    [Header("호황 이미지")]
    [SerializeField] Sprite gunRegulationSprite;    // 총기 규제 완화 이미지
    [SerializeField] Sprite shootingGameSprite;     // 슈팅 게임 인기 이미지
    [SerializeField] Sprite actionMovieSprite;      // 건액션 영화 이미지
    [SerializeField] Sprite militaryFestivalSprite; // 밀리터리 페스티벌 이미지
    

    [Header("불황 이미지")]
    // Recession 이벤트들
    [SerializeField] Sprite shootingIncidentSprite; // 총기 난사 사건 이미지
    [SerializeField] Sprite economicCrisisSprite;   // 경제 악화 이미지
    [SerializeField] Sprite gunProtestSprite;       // 총기 규제 시위 이미지

    //총기 규제 이벤트
    private static event Action gunProtestEvent; //총기 규제 시위시 발생하는 npc 이벤트
    void Start()
    {
        //TvScreen.SetActive(false);
    }

    void OnEnable()
    {
        SocialEventManager.OnNewsScreenUpdate +=CheckString;
    }
    void OnDisable()
    {
        SocialEventManager.OnNewsScreenUpdate -=CheckString;
    }

//문자열 대조
    void CheckString(string eventName) {

        TvScreen.SetActive(true);
        switch (eventName)
        {
            //호황
            case "[ Relaxation of gun regulations ]":// 총기 규제 완화
                newsImage.sprite = gunRegulationSprite;
                break;
            case "[ Popularity of shooting game ]":// 슈팅 게임 인기
                newsImage.sprite = shootingGameSprite;
                break;
            case "[ Popularity of gun-action movies ]":// 건액션 영화
                newsImage.sprite = actionMovieSprite;
                break;
            case "[ Military festival opens ]":// 밀리터리 페스티벌
                newsImage.sprite = militaryFestivalSprite;
                break;

            //불황
            case "[ Gun control following a shooting incident ]":// 총기 난사 사건
                newsImage.sprite = shootingIncidentSprite;
                break;
            case "[ Economic recession ]":// 경제 악화
                newsImage.sprite = economicCrisisSprite;
                break;
            case "[ Gun control protest ]": // 총기 규제 시위
                newsImage.sprite = gunProtestSprite;
                gunProtestEvent?.Invoke();//이벤트 발생
                break;

            //평범
            case "Day":
                TvScreen.SetActive(false);
                break;
        }
    }
}
