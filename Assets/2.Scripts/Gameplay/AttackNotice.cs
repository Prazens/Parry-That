using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격 예고 이펙트를 출력하고 제거하는 로직.
/// </summary>
public class AttackNotice : MonoBehaviour
{
    //투사체 발사 시의 !관련
    [SerializeField] private GameObject exclamationPrefab; // 공통 느낌표 프리팹
    // 🔹 `List<Sprite>`로 변경 (느낌표 타입별 이미지 저장)
    [SerializeField] private List<Sprite> exclamationSprites = new List<Sprite>();
    private Transform exclamationParent; // 느낌표 표시 위치
    private List<GameObject> prepareExclamation = new List<GameObject>(); // 느낌표 오브젝트 저장
    public GameObject holdExclamation; // 홀드 느낌표

    /*
    // 느낌표 생성 관련 함수
    private void SetupExclamationParent()
    {
        // `exclamationParent`가 없으면 자동 생성
        if (exclamationParent == null)
        {
            GameObject newParent = new GameObject("ExclamationHolder");
            newParent.transform.SetParent(this.transform);
            // **🔹 느낌표 기본 위치 설정**
            switch (location)
            {
                case Direction.Up:
                    newParent.transform.localPosition = new Vector3(1.5f, 0, 0);
                    break;
                case Direction.Down:
                    newParent.transform.localPosition = new Vector3(1.5f, 0, 0);
                    break;
                case Direction.Left:
                case Direction.Right:
                    newParent.transform.localPosition = new Vector3(0, -1.5f, 0);
                    break;
            }
            exclamationParent = newParent.transform;
        }
    }

    private void ShowExclamation(AttackType type)
    {
        //** 기존 느낌표 지우고 다시 생성**
        foreach (GameObject ex in prepareExclamation)
        {
            Destroy(ex);
        }
        int count = prepareQueue.Count; // 현재 준비된 공격 개수
        prepareExclamation.Clear();

        if (type == AttackType.HoldStart || type == AttackType.StreamStart)
        {
            if (SceneManager.GetActiveScene().name == "Stage4" ||
                SceneManager.GetActiveScene().name == "Stage5" ||
                SceneManager.GetActiveScene().name == "BossHoldTest")
                holdExclamation.GetComponent<holdExclamation>().Appear(bpm, 2);
            else
                holdExclamation.GetComponent<holdExclamation>().Appear(bpm, 1);
        }
        else if (type == AttackType.HoldFinishStrong || type == AttackType.StreamFinish)
        {
            if (SceneManager.GetActiveScene().name == "Stage4" ||
                SceneManager.GetActiveScene().name == "Stage5" ||
                SceneManager.GetActiveScene().name == "BossHoldTest")
                holdExclamation.GetComponent<holdExclamation>().Disappear(bpm, 2);
            else
                holdExclamation.GetComponent<holdExclamation>().Disappear(bpm, 1);

        }

        List<PrepareEntry> tempList = new List<PrepareEntry>(prepareQueue); // 현재 큐를 리스트로 변환 (순서 유지)


        for (int i = 0; i < count; i++)
        {
            Vector3 exclamationPosition = new Vector3((i + 1) * spacing, 0, 0); //(i - (count - 1) / 2f) 
            GameObject newExclamation = Instantiate(exclamationPrefab, exclamationParent);
            newExclamation.transform.localPosition = exclamationPosition; // **🔹 `exclamationParent` 기준 정렬**


            // **🔹 색상 변경 (공격 유형에 따라)**
            SpriteRenderer exclamationSprite = newExclamation.GetComponent<SpriteRenderer>();
            if (exclamationSprite != null)
            {
                int noteColor = (int)tempList[i].attackType;
                // 🔹 `type`이 `exclamationSprites` 범위 내에 있는지 확인
                if (noteColor >= 0 && noteColor < exclamationSprites.Count)
                {
                    exclamationSprite.sprite = exclamationSprites[noteColor]; // 리스트에서 해당 타입에 맞는 스프라이트 적용
                }
                else
                {
                    // Debug.LogWarning($"Unknown attack type {noteColor}! Defaulting to first sprite.");
                    exclamationSprite.sprite = exclamationSprites[0]; // 기본값
                }
            }

            prepareExclamation.Add(newExclamation);
        }
    }

    private void exclamationRelocation()
    {
        if (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]); // 가장 오래된 느낌표 제거
            prepareExclamation.RemoveAt(0);
            int count = prepareExclamation.Count;
            // 남은 느낌표 위치 재배치
            for (int i = 0; i < count; i++)
            {
                prepareExclamation[i].transform.localPosition = new Vector3((i + 1) * spacing, 0, 0);
            }
        }
    }
    */
}
