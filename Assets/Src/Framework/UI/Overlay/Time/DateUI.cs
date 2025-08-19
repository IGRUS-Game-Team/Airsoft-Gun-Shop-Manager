using System;
using System.ComponentModel;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DateUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI monthAndDate;
    [Header("일/월")]
    [SerializeField] int date = 30;
    [SerializeField] int month = 1;
    
    int[] dateNum = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    void Start()
    {
        UpdateUI();
    }

    public void UpdateDate()
    {
        //SocialEventManager.Instance.ExecuteStrategy();
        date++;
        
        // 리스트에서 현재 월의 최대 날짜 확인
        if (date > dateNum[month - 1])
        {
            month++;
            date = 1;
            // 12월을 넘어가면 1월로 초기화
            if (month > 12)
            {
                month = 1;
            }            
        }

        UpdateUI();
    }
    
    private void UpdateUI() //UI 업데이트
    {
        monthAndDate.text = string.Format("{0:00}/{1:00}", month, date);
    }
    
}