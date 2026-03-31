using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageDialogManager : Singleton<StageDialogManager>
{
    
    private Vector2 dialogSize;
    private Vector2 dialogSpacing = new Vector2(20f, 10f); // 대화창 내부 여백 (좌우, 상하), 임시 하드코딩
    private DialogueData currentDialogueData = null;
    private int maxCharsPerLine = 0;

    public float playSpeed = 1f; // 대화 재생 속도 (1f = 정상 속도)
    public float typingInterval = 0.05f; // 글자 하나당 표시되는 시간 간격 (초)
    public float lineDisplayInterval = 0.5f; // 한 줄이 완성된 후 다음 줄로 넘어가기 전 대기 시간 (초)
    public float lineDecayInterval = 0.1f; // 한 줄이 완성된 후 사라지는 시간 간격 (초)
    public float lineUpSpeed = 1f; // 줄이 올라가는 속도 (1f = 정상 속도)

    public float dialogTimer = 0f; // 대화용 타이머

    public void InitDialog()
    {
        dialogSize = GetComponent<RectTransform>().sizeDelta;
        CalcMaxChar();
    }

    /// <summary>
    /// 화면 크기에 따라 최대 글자 수 계산
    /// <para> 폰트 크기와 줄 간격도 고려해야 하지만, 임시로 고정값 사용 </para>
    /// </summary>
    public void CalcMaxChar()
    {
        maxCharsPerLine = 20; // 임시 값, 실제로는 dialogSize와 폰트 크기를 고려하여 계산해야 함
    }

    public void StartDialog(DialogueData dialogueData)
    {
        currentDialogueData = dialogueData;
        if (currentDialogueData == null)
        {
            Debug.LogError("StageDialogManager: currentDialogueData is null. Cannot start dialog.");
            return;
        }

        dialogTimer = 0f; // 타이머 초기화
    }

    public void FinishDialog()
    {
        // 대화 종료 처리
    }

    IEnumerator DisplayDialogCoroutine()
    {

        TMP_Text myText = GetComponent<TMP_Text>();

// 이 값은 인스펙터에 노출되어 있지 않지만, 코드상에서는 언제든 접근 가능합니다.
        myText.maxVisibleCharacters = 5;


        yield return null;
    } 

}
