using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{   //총 플레이타임을 초로 누적시켜서 계산
    [SerializeField] TextMeshProUGUI hourAndMinute;
    [Header("현실 1초 게임 1분")]
    [SerializeField] public int totalGameMinutes = 480; //현실 1초, 게임 1분
    float timer = 0f;//현실 1초를 재기 위한 타이머

    //08.12 이지연 시간 멈추는 bool 값 설정
    public bool isTimePaused = false; 

    const int MINUTES_PER_HOUR = 60; //하루 분
    const int HOURS_PER_DAY = 24; //하루 시간


    [Header("다음날로 넘어갈 때 사용되는 스크립트")]
    public UnityEvent OnDayChanged; // Inspector에서 설정 가능

    void Start()
    {
        UpdateTimeDisplay();
    }

    void Update()
    {
        if (isTimePaused) return;

        timer += Time.deltaTime;//0.016초

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
            OnDayChanged.Invoke();
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
        return totalGameMinutes * 1f; // 현실 1초 = 게임 1분일 경우
    }
}