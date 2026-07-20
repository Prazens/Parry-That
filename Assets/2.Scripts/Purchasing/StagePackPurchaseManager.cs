using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public sealed class StagePackPurchaseManager : MonoBehaviour, IDetailedStoreListener
{
    [Header("Commercial Settings")]
    [SerializeField] private bool isNonCommercialMode = true;
    
    // 무한 루프 및 모호성을 방지하기 위해 Awake 시점에 값을 복사해둘 static 변수
    private static bool isNonCommercialModeStatic = false;

    public const string ProductId = "stage_pack_567";
    private const string EntitlementKey = "iap_stage_pack_567_owned";

    public static event Action PurchaseStateChanged;

    private static StagePackPurchaseManager instance;
    private IStoreController storeController;
    private bool isInitializing;

    public static StagePackPurchaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StagePackPurchaseManager>();
            }

            if (instance == null)
            {
                GameObject managerObject = new GameObject(nameof(StagePackPurchaseManager));
                instance = managerObject.AddComponent<StagePackPurchaseManager>();
            }

            return instance;
        }
    }

    // Instance 호출을 유발하지 않는 안전한 static 프로퍼티
    public static bool IsNonCommercialMode => isNonCommercialModeStatic;

    // 중복 및 모호성을 완전히 제거한 최종 소유 확인 프로퍼티
    public static bool IsStagePackOwned 
    {
        get
        {
            if (isNonCommercialModeStatic) return true; // 비상업용 모드라면 항상 소유한 것으로 처리
            return PlayerPrefs.GetInt(EntitlementKey, 0) == 1;
        }
    }

    public bool IsStoreReady => storeController != null;
    public bool IsPurchaseInProgress { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 인스펙터 설정값을 안전하게 스태틱 변수로 복사 (싱글톤 상호 참조 데드락 방지)
        isNonCommercialModeStatic = this.isNonCommercialMode;

        InitializePurchasing();
    }

    public static bool IsPremiumStage(int stageId)
    {
        return stageId >= 5 && stageId <= 7;
    }

    public void InitializePurchasing()
    {
        // 비상업용 모드일 경우 결제 모듈 초기화를 스킵합니다.
        if (isNonCommercialModeStatic) return;
        if (storeController != null || isInitializing) return;

        isInitializing = true;
        StandardPurchasingModule module = StandardPurchasingModule.Instance();

#if UNITY_EDITOR
        module.useFakeStoreAlways = true;
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
        builder.AddProduct(ProductId, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    public void PurchaseStagePack()
    {
        if (IsStagePackOwned || IsPurchaseInProgress) return;

        Product product = storeController?.products.WithID(ProductId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogWarning("[IAP] Stage pack product is not ready to purchase.");
            PurchaseStateChanged?.Invoke();
            return;
        }

        IsPurchaseInProgress = true;
        PurchaseStateChanged?.Invoke();
        storeController.InitiatePurchase(product);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        isInitializing = false;

        Product product = controller.products.WithID(ProductId);
        SetEntitlement(product != null && product.hasReceipt);
        PurchaseStateChanged?.Invoke();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        IsPurchaseInProgress = false;

        if (purchaseEvent.purchasedProduct.definition.id != ProductId)
        {
            Debug.LogWarning($"[IAP] Unknown product received: {purchaseEvent.purchasedProduct.definition.id}");
            return PurchaseProcessingResult.Complete;
        }

        SetEntitlement(true);
        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, string.Empty);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        isInitializing = false;
        IsPurchaseInProgress = false;
        Debug.LogWarning($"[IAP] Initialization failed: {error} {message}");
        PurchaseStateChanged?.Invoke();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        HandlePurchaseFailure(product, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        HandlePurchaseFailure(product, $"{failureDescription.reason}: {failureDescription.message}");
    }

    private void HandlePurchaseFailure(Product product, string reason)
    {
        IsPurchaseInProgress = false;
        Debug.LogWarning($"[IAP] Purchase failed ({product?.definition.id}): {reason}");
        PurchaseStateChanged?.Invoke();
    }

    private static void SetEntitlement(bool isOwned)
    {
        bool changed = IsStagePackOwned != isOwned;
        PlayerPrefs.SetInt(EntitlementKey, isOwned ? 1 : 0);
        PlayerPrefs.Save();

        if (changed)
        {
            PurchaseStateChanged?.Invoke();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Parry That/IAP/Unlock Stage Pack 5-7")]
    private static void DebugUnlock()
    {
        SetEntitlement(true);
    }

    [UnityEditor.MenuItem("Tools/Parry That/IAP/Lock Stage Pack 5-7")]
    private static void DebugLock()
    {
        SetEntitlement(false);
    }

    [UnityEditor.MenuItem("Tools/Parry That/IAP/Toggle Non-Commercial Mode")]
    private static void ToggleNonCommercial()
    {
        if (Instance != null)
        {
            Instance.isNonCommercialMode = !Instance.isNonCommercialMode;
            isNonCommercialModeStatic = Instance.isNonCommercialMode;
            Debug.Log($"[IAP] Non-Commercial Mode changed to: {isNonCommercialModeStatic}");
            PurchaseStateChanged?.Invoke();
        }
    }
#endif
}