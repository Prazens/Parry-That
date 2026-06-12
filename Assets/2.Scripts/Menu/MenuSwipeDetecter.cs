using UnityEngine;

/// <summary>
/// 타이틀 및 스테이지 선택 화면에서 스와이프 입력 감지
/// </summary>
public class MenuSwipeDetecter : Singleton<MenuSwipeDetecter>
{
    protected override bool DontDestroy => false;

    [SerializeField] private float swipeThreshold =    10f;
    private float weightedSwipeThreshold;
    private Vector2 startPos = Vector2.zero;  // 기본값

    void Start()
    {
        weightedSwipeThreshold = swipeThreshold * Screen.height / 1920f;  // 화면 크기에 따른 가중치 적용
    }

    // Update is called once per frame
    void Update()
    {
        if (MenuManager.Instance.currentState == MenuManager.MenuState.Title
            || MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect)
        {
            SwipeDetect();
        }
    }

    /// <summary>
    /// 스와이프 감지 (마우스)
    /// </summary>
    private void SwipeDetect()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            float swipeDistance = startPos.y - Input.mousePosition.y;

            // 스와이프 업 감지
            if (swipeDistance < -weightedSwipeThreshold)
            {
                OnSwipeUp();
            }

            // 스와이프 다운 감지
            if (swipeDistance > weightedSwipeThreshold)
            {
                // OnSwipeDown();
            }
            return;
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float swipeDistance = startPos.y - touch.position.y;

                // 스와이프 업 감지
                if (swipeDistance < -weightedSwipeThreshold)
                {
                    OnSwipeUp();
                }

                // 스와이프 다운 감지
                if (swipeDistance > weightedSwipeThreshold)
                {
                    // OnSwipeDown();
                }
                return;
            }
        }
    }

    /// <summary>
    /// 스와이프 업 이벤트 처리
    /// </summary>
    private void OnSwipeUp()
    {
        // 스테이지 선택 화면에서 : 스테이지 시작
        if (MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect)
        {
            MenuManager.Instance.StartStage();
        }

        // 타이틀 화면에서 : 스테이지 선택 화면으로 전환
        else if (MenuManager.Instance.currentState == MenuManager.MenuState.Title
                 && StageDBManager.Instance.isFirstLaunch == true)
        {
            MenuManager.Instance.titleUI.OnSwipeUp();
        }
    }
}
