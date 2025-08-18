using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    private NewsPaperController newsPaperController;
    //총 플레이타임을 초로 누적시켜서 계산
    [SerializeField] TextMeshProUGUI hourAndMinute;
    [Header("현실 1초 게임 1분")]
    [SerializeField] public int totalGameMinutes = 480; //현실 1초, 게임 1분
    [Header("개발자용 date 넘어가기위한 시간 조절")]
    [SerializeField] private bool DeveloperMode = false; //개발자 모드
    [SerializeField] private int startTime = 1430; //시작 시간조정 : 개발자 모드 키면 23:50 으로 워프 -> 날 넘어가는거 확인용
    [SerializeField] private float timeSpeed = 1f; // 시간 속도 기본 
    float timer = 0f;//현실 1초를 재기 위한 타이머

    //08.12 이지연 시간 멈추는 bool 값 설정
    [Header("시간 멈추기")]
    public bool isTimePaused = false;

    [Header("하루 시작 시간 설정")]
    [SerializeField] private int dayStartHour = 8; // 오전 8시부터 시작
    
    [Header("다음날로 넘어갈 때 사용되는 스크립트")]
    public UnityEvent OnDayChanged; // Inspector에서 설정 가능

    const int MINUTES_PER_HOUR = 60; //하루 분
    const int HOURS_PER_DAY = 24; //하루 시간



    void Start()
    {
        if (DeveloperMode) totalGameMinutes = startTime;
        CheckAndExecute8AM();
        
    }

    void Update()
    {
        if (isTimePaused) return;

        timer += Time.deltaTime * timeSpeed; ;//0.016초

        if (timer >= 1f)
        {
            totalGameMinutes++;
            timer -= 1f; // 1과 맞아 떨어지지 않는 순간을 대비하여
            UpdateTimeDisplay();
        }
        

    }

    void UpdateTimeDisplay()
    {
        //총 minutes를 세고 상수를 나눠 구함
        int hours = (totalGameMinutes / MINUTES_PER_HOUR) % HOURS_PER_DAY;
        int minutes = totalGameMinutes % MINUTES_PER_HOUR;

        //hours 가 00,00에 도달하는 순간 DateUI라는 스크립트에 신호를 보낸다.
        if (hours == 0 && minutes == 0)
        {
            OnDayChanged.Invoke(); // date +1되는 것
            Debug.Log("00:00시 도달");

            //8시로 스킵
            totalGameMinutes += (dayStartHour * MINUTES_PER_HOUR); // 480분 추가

            CheckAndExecute8AM();
        }
        // {인덱스:디폴트 숫자}
        hourAndMinute.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    public void ForceUpdate()
    {
        UpdateTimeDisplay();
    }
    public float GetTotalPlayTimeInRealSeconds()
    {
        return totalGameMinutes; // 현실 1초 = 게임 1분일 경우
    }
    
    private void CheckAndExecute8AM() //이벤트 시작 시간인 8시인지 확인하는 메서드
    {
        int hours = (totalGameMinutes / MINUTES_PER_HOUR) % HOURS_PER_DAY;
        int minutes = totalGameMinutes % MINUTES_PER_HOUR;

        if (hours == 8 && minutes == 0)
        {
            SocialEventManager.Instance.ExecuteStrategy();
            Debug.Log("8시 정각 - 이벤트 실행");
        }
    }

    

    
}