using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonAttackNoticeHandler : MonoBehaviour, IAttackHandler<AttackNoticeContext>
{
    [SerializeField] private Transform noticeParent; // 예고 표시 위치
    [SerializeField] private GameObject noticePrefab; // 공통 예고 프리팹
    [SerializeField] private Sprite[] noticeSprites; // 공격 타입별 예고 이미지 배열
    private List<GameObject> noticeInstances = new(); // 예고 인스턴스 저장
    private float noticeSpacing => 30f; // 예고 인스턴스 사이 간격

    public void OnNotice(AttackNoticeContext context)
    {
        GameObject newNotice = AddNotice((AttackType)context.note.type, (Direction)(context.note.strikerIndex + 1));
        context.judgeables?[0]?.AddOnDestroy(_ => RemoveNotice(newNotice));
    }

    void IAttackHandler.OnNotice(IAttackContext context)
        => OnNotice((AttackNoticeContext)context);

    public void OnAttackStart(AttackNoticeContext context)
    {

    }

    void IAttackHandler.OnAttackStart(IAttackContext context)
        => OnAttackStart((AttackNoticeContext)context);

    public void OnJudge(JudgeContext context)
    {

    }

    private GameObject AddNotice(AttackType attackType, Direction direction)
    {
        // Notice 오브젝트 생성
        int currentIndex = noticeInstances.Count;
        GameObject newNotice = Instantiate(noticePrefab, noticeParent);
        Vector3 newNoticePosition = new Vector3(currentIndex * noticeSpacing, 0, 0);
        newNotice.transform.localPosition = newNoticePosition;

        // 회전 (Direction에 따라)
        float rotationAngle = 0f;
        switch (direction)
        {
            case Direction.Up: rotationAngle = 0f; break;
            case Direction.Down: rotationAngle = 180f; break;
            case Direction.Left: rotationAngle = 90f; break;
            case Direction.Right: rotationAngle = -90f; break;
        }
        newNotice.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

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
        return newNotice;
    }

    private void RemoveNotice(GameObject noticeInstance)
    {
        if (noticeInstance == null) return;

        noticeInstances.Remove(noticeInstance);
        Destroy(noticeInstance);

        if (noticeInstances.Count > 0)
        {
            // 남은 예고 인스턴스 위치 재배치
            for (int i = 0; i < noticeInstances.Count; i++)
            {
                noticeInstances[i].transform.localPosition = new Vector3(i * noticeSpacing, 0, 0);
            }
        }
    }

    private void RemoveAll()
    {
        while (noticeInstances.Count > 0)
        {
            Destroy(noticeInstances[0]);
            noticeInstances.RemoveAt(0);
        }
    }
}
