using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공격 예고 이펙트를 출력하고 제거하는 로직.
/// </summary>
public class AttackNoticeController : MonoBehaviour
{
    [SerializeField] private Transform noticeParent; // 예고 표시 위치
    [SerializeField] private GameObject noticePrefab; // 공통 예고 프리팹
    [SerializeField] private Sprite[] noticeSprites; // 공격 타입별 예고 이미지 배열
    private List<GameObject> noticeInstances = new(); // 예고 인스턴스 저장
    private HoldExclamation holdExclamation; // 홀드 느낌표
    private float noticeSpacing => 30f; // 예고 인스턴스 사이 간격

    public void ShowNewNotice(AttackType attackType, Direction direction, float durationSec)
    {
        // HoldExclamation 호출 후 종료
        if (attackType == AttackType.HoldStart)
        {
            holdExclamation.Appear(durationSec);
            return;
        }
        if (attackType == AttackType.HoldFinishStrong)
        {
            holdExclamation.Disappear(durationSec);
            return;
        }

        // Notice 오브젝트 생성
        int currentIndex = noticeInstances.Count;
        GameObject newNotice = Instantiate(noticePrefab, noticeParent);
        Vector3 newNoticePosition = new Vector3(currentIndex * noticeSpacing, 0, 0);
        newNotice.transform.localPosition = newNoticePosition;

        // Sprite 변경 (AttackType에 따라)
        Image image = newNotice.GetComponent<Image>();
        if (image != null)
        {
            // attackType이 noticeSprites 범위 내에 있는지 확인
            if (attackType >= 0 && (int)attackType < noticeSprites.Length)
            {
                // 배열에서 해당 타입에 맞는 스프라이트 적용
                image.sprite = noticeSprites[(int)attackType];
            }
            else
            {
                // 기본값
                image.sprite = noticeSprites[0];
            }
        }

        noticeInstances.Add(newNotice);
    }

    public void DestroyFirstNotice(AttackType attackType)
    {
        if (attackType == AttackType.HoldStart)
        {
            return;
        }
        if (attackType == AttackType.HoldFinishStrong)
        {
            holdExclamation.ForceStop();
            return;
        }

        if (noticeInstances.Count > 0)
        {
            // 가장 오래된 예고 인스턴스 제거
            Destroy(noticeInstances[0]);
            noticeInstances.RemoveAt(0);

            // 남은 예고 인스턴스 위치 재배치
            for (int i = 0; i < noticeInstances.Count; i++)
            {
                noticeInstances[i].transform.localPosition = new Vector3(i * noticeSpacing, 0, 0);
            }
        }
    }

    private void DestroyAllNotices()
    {
        while (noticeInstances.Count > 0)
        {
            // 가장 오래된 예고 인스턴스 제거
            Destroy(noticeInstances[0]);
            noticeInstances.RemoveAt(0);
        }
    }
}
