# Google Play 스테이지 5~7 결제 설정

## 구현된 동작

- 스테이지 5, 6, 7은 `stage_pack_567` 하나를 구매하면 모두 해제됩니다.
- 구매 전에도 디스크를 좌우로 넘기며 스테이지를 볼 수 있습니다.
- 구매 전 5~7을 선택하면 `PurchasePanel`이 표시되고 스테이지에는 진입할 수 없습니다.
- PurchasePanel이 보여도 좌우 스와이프와 설정 버튼 입력은 가능합니다.
- 설정 화면을 열면 PurchasePanel은 숨고, 설정을 닫으면 선택 스테이지에 따라 다시 표시됩니다.
- Google Play 구매 성공 또는 기존 구매 복원 후에는 패널이 사라지고 5~7에 진입할 수 있습니다.
- 상품은 비소모성 상품이므로 한 번만 구매합니다.

## Unity에서 확인할 설정

1. Unity를 열고 Package Manager가 패키지를 복원할 때까지 기다립니다.
   - 프로젝트에는 `com.unity.purchasing` 4.14.2가 추가되어 있습니다.
2. `Edit > Project Settings > Services > In-App Purchasing`에서 IAP를 활성화합니다.
3. `Assets/1.Scenes/testMain.unity`의 `Menu` 오브젝트를 선택합니다.
4. `MenuManager > Stage 5~7 Purchase > Purchase Panel Prefab`에 아래 프리팹이 연결됐는지 확인합니다.
   - `Assets/5.Prefabs/Panel/PurchasePanel.prefab`
5. PurchasePanel 내부 이름을 유지합니다.
   - 루트: `PurchasePanel`
   - 구매 버튼: `PurchaseButton`
   - 홍보 이미지: `PurchaseImage`
6. `PurchaseImage`에 홍보용 Sprite를 지정합니다.
7. 구매 버튼의 문구와 디자인을 완성합니다. 실제 결제 가격은 Google Play에서 내려오는 상품 가격이 기준입니다.

## Google Play Console 상품 설정

1. Google Play Console에서 이 게임 앱을 선택합니다.
2. `수익 창출 > 제품 > 인앱 상품` 또는 `일회성 제품` 메뉴로 이동합니다.
3. 상품을 생성합니다.
   - 상품 ID: `stage_pack_567`
   - 상품 유형: 일회성 상품
   - Unity 상품 유형: `NonConsumable`로 이미 구현됨
   - 기본 가격: 대한민국 `₩2,900`
4. 상품 이름과 설명을 작성하고 상품을 활성화합니다.
5. Unity의 Android Package Name과 Play Console 앱의 패키지 이름이 정확히 같은지 확인합니다.
6. 서명된 AAB를 내부 테스트 트랙에 업로드하고 출시합니다.

상품 ID는 코드의 `StagePackPurchaseManager.ProductId`와 반드시 일치해야 합니다.

## 실제 Google Play 결제 테스트

1. Play Console의 라이선스 테스트 계정과 내부 테스트 사용자를 등록합니다.
2. 테스트 사용자가 내부 테스트 참여 링크를 수락하게 합니다.
3. 기기에 같은 Google 계정으로 로그인합니다.
4. APK를 직접 설치하지 말고 Google Play 내부 테스트 페이지에서 앱을 설치합니다.
5. 스테이지 5, 6, 7 중 하나로 이동합니다.
6. PurchasePanel이 표시되고 디스크 스와이프와 설정 진입은 가능한지 확인합니다.
7. 디스크 또는 스테이지 시작 입력으로 게임에 진입되지 않는지 확인합니다.
8. 구매 버튼을 눌러 Google Play 테스트 결제를 완료합니다.
9. PurchasePanel이 즉시 사라지고 5~7 모두 진입 가능한지 확인합니다.
10. 앱을 삭제 후 재설치해 기존 구매가 자동 복원되는지 확인합니다.

Google Play의 비소모성 구매는 Unity IAP 초기화 시 영수증으로 복원됩니다. 초기화가 성공하면 로컬 캐시도 스토어 영수증 상태와 다시 동기화됩니다.

## Unity Editor 테스트

Editor에서는 Unity IAP Fake Store를 강제로 사용합니다.

1. Play Mode에서 5~7로 이동합니다.
2. PurchasePanel의 버튼을 누릅니다.
3. Fake Store 창에서 성공을 선택합니다.
4. 패널이 사라지고 진입 가능한지 확인합니다.

구매 여부를 강제로 바꾸려면 Unity 상단 메뉴를 사용합니다.

- `Tools > Parry That > IAP > Unlock Stage Pack 5-7`
- `Tools > Parry That > IAP > Lock Stage Pack 5-7`

Play Mode 진입 직후 Fake Store 초기화가 완료되면 스토어 영수증 상태가 우선 적용됩니다. 잠금/해제 메뉴 테스트는 초기화 후 사용하세요.

## 출시 전 필수 확인

- `stage_pack_567` 상품이 활성 상태인지 확인합니다.
- 한국 가격이 ₩2,900인지 확인합니다.
- 내부 테스트가 아니라 실제 결제가 발생할 계정으로 테스트하지 않도록 주의합니다.
- 환불 후 앱을 온라인 상태로 재실행했을 때 권한이 다시 잠기는지 확인합니다.
- 현재 구현은 Google Play 영수증과 로컬 캐시를 사용하는 클라이언트 방식입니다. 변조 방지가 중요한 경우 Google Play Developer API 또는 서버 영수증 검증을 추가해야 합니다.

## 공식 문서

- [Unity IAP 설정](https://docs.unity3d.com/2022.3/Documentation/Manual/UnityIAPSettingUp.html)
- [Unity IAP 구매 처리](https://docs.unity3d.com/Manual/UnityIAPProcessingPurchases.html)
- [Google Play 결제 테스트](https://developer.android.com/google/play/billing/test)
