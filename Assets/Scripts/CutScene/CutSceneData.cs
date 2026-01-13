using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//summary: 컷씬의 데이터를 저장하는 역할(Scriptable Object 사용)

//컷씬 연출 및 동작 종류
public enum CutSceneActionType
{
    ShowPanel, //패널 보이기
    HidePanel, //패널 숨기기
    FadeIn, //패널 페이드인
    FadeOut, // 패널 페이트아웃
    ShowTextAppend, // 이전 텍스트 유지 후 다음 텍스트 생성
    ShowTextReset, // 이전 텍스트 모두 없앤 후 다음 텍스트 생성
    TriggerAnimator // 애니메이터 트리거
}

//컷씬에서 취할 액션
[Serializable]
public class CutSceneAction
{
    public CutSceneActionType actionType; //액션 종류
    public int panelIndex; //조작 혹은 애니메이션을 진행할 패널의 인덱스
    public float waitseconds = 0; //액션 후 대기시간
    public string animatorTrigger; //애니메이터 전용
}

//패널 타입(단순 스프라이트 or 애니메이터 포함)
public enum CutScenePanelType
{
    Sprite,
    Animator
}

[Serializable]
public class CutScenePanels
{
    //컷씬 패널의 타입
    public CutScenePanelType type = CutScenePanelType.Sprite;

    //스프라이트의 앵커
    public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
    public Vector2 pivot     = new Vector2(0.5f, 0.5f);

    //앵커 기준 위치 및 크기
    public Vector2 anchoredPosition = Vector2.zero;
    public Vector2 size = new Vector2(100, 100);
    public Vector3 scale = Vector3.one;

    // Sprite 전용
    public Sprite sprite;

    // Animator 전용
    public RuntimeAnimatorController animatorcontroller;

}

//컷씬 데이터 SO
[CreateAssetMenu(menuName = "Cutscene/CutSceneData")]
public class CutSceneData : ScriptableObject
{
    //컷씬 대사
    [TextArea(1, 10)]
    public string[] textSet;

    //컷씬 이미지 및 애니메이터
    public List<CutScenePanels> panels = new List<CutScenePanels>();

    //클릭 시 나올 다음 액션(예: 다음 대사 출력, 다음 컷씬 페이드인)
    public List<CutSceneClickStep> clickSteps = new List<CutSceneClickStep>();
    
    //컷씬의 챕터(프롤로그, stage1,2...)
}

//클릭 시 진행될 액션 리스트
[Serializable]
public class CutSceneClickStep
{
    public List<CutSceneAction> actions = new List<CutSceneAction>();
}
