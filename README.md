# [C#/Unity] 3D PVE Quarter-View Action System

**게임명:** Just Fight
**개발 환경:** C#, Unity
**개발 기간 & 인원:** 2026/05 (5일), 1인 개발  
**핵심 역할:** FSM 기반 보스 AI 설계, 플레이어 상태/물리 동기화 로직, 타임라인 컷씬 및 전투 시스템 전반 구현  

## 기술적 핵심
단순한 기능 구현을 넘어 컴포넌트 간의 결합도를 낮추고, 5일이라는 짧은 기간 내에서도 유지보수가 용이한 객체 지향적이고 안정적인 시스템 아키텍처를 설계하는 데 집중했습니다.  

[[Just Fight 시연 영상]](https://youtu.be/_jYzGusobCA)

</br>

### 1. 인터페이스 분리와 이벤트 리시버 (Interface Segregation & Event Decoupling)
애니메이션 이벤트(Animation Event)가 메인 캐릭터 컨트롤러에 직접 엮여 있을 경우, 추후 캐릭터 모델이나 애니메이션이 변경될 때마다 핵심 로직(Controller)을 수정해야 하는 유지보수 문제가 발생합니다. 이를 해결하기 위해 기능별 인터페이스를 분리하고 브릿지(Bridge) 역할을 하는 리시버를 도입했습니다.  

#### 핵심 구현 내용  

- IDamageable, ICombatEvents, IMovementEvents 등의 인터페이스를 정의하여 전투, 이동, 피격 시스템의 규약을 명확히 분리했습니다.

- AnimationEventReceiver 클래스를 모델(View) 계층에 부착하여, 애니메이션 프레임에서 발생하는 이벤트를 내부 시스템(Controller/Logic)으로 델리게이트(Delegate) 하도록 의존성을 분리(Decoupling)했습니다.

<details>
<summary>AnimationEventReceiver 클래스</summary>
<div markdown="1">

```C++
#pragma once
#include "ICombatEvents.h"
#include "IMovementEvents.h"

// 인터페이스 정의
class IDamageable {
public:
    virtual void TakeDamage(float amount, FVector hitPoint, FVector hitDirection, float knockbackPower) = 0;
    virtual ~IDamageable() = default;
};

// 애니메이션과 시스템을 연결하는 브릿지 클래스
class UAnimationEventReceiver : public UActorComponent {
private:
    IMovementEvents* MovementComp;
    ICombatEvents* CombatComp;

public:
    void Awake() {
        // 상위 액터나 컴포넌트에서 인터페이스를 가져와 캐싱
        MovementComp = GetOwner()->FindComponentByClass<IMovementEvents>();
        CombatComp = GetOwner()->FindComponentByClass<ICombatEvents>();
    }

    // 전투 관련 애니메이션 이벤트 브릿지
    void OnCheckCombo() { if(CombatComp) CombatComp->OnCheckCombo(); }
    void OnAttackEnd() { if(CombatComp) CombatComp->OnAttackEnd(); }
    void EnableAttack(const std::string& parameters) { if(CombatComp) CombatComp->EnableAttack(parameters); }
    void DisableAttack() { if(CombatComp) CombatComp->DisableAttack(); }

    // 이동 관련 애니메이션 이벤트 브릿지
    void OnRollEnd() { if(MovementComp) MovementComp->OnRollEnd(); }
    void StartRollInvincible() { if(MovementComp) MovementComp->StartRollInvincible(); }
    void RollInvincibleEnd() { if(MovementComp) MovementComp->RollInvincibleEnd(); }
};

```

</div>
</details>  
</br>

#### 구현 성과

**1. 재사용성 및 확장성 확보:** 전투 로직이 뷰(View)와 완전히 분리되어, 추후 새로운 무기 모션이나 몬스터 스켈레톤이 추가되더라도 AnimationEventReceiver만 연결하면 로직 수정 없이 그대로 작동합니다.  
**2. 코드 응집도 향상:** 거대한 PlayerController 클래스가 모든 기능을 통제하지 않고, 전투(PlayerCombat)와 이동(PlayerMovement) 모듈이 각자의 역할에만 집중할 수 있게 되었습니다.

</br>

### 2. FSM 기반의 보스 AI 설계 (State Machine & Stance System)
단순한 추적/공격이 아닌, 거리(Distance)와 난수(Random)에 기반한 전술적 움직임을 구현하기 위해 FSM(Finite State Machine)을 설계했습니다.

#### 핵심 구현 내용
- Idle, Chase, CombatStance, Attack, Groggy, Dead 상태를 정의하여 각 상태별 업데이트 로직을 격리했습니다.

- 강인도(Stance) 시스템: 단순 체력 게이지와 별도로 보스의 강인도 게이지(maxStance)를 도입했습니다. 지속적인 타격으로 강인도가 0이 되면 Groggy 상태로 전환되어 플레이어에게 시각적 쾌감과 역습의 기회를 제공합니다.

- NavMeshAgent의 SetDestination을 활용하여, 전투 대치(CombatStance) 중 게걸음(Strafe), 접근(Approach), 물러나기(BackStep) 등 다채로운 움직임을 조합했습니다.

<details> 
<summary>보스의 전술적 의사결정 코드</summary> 
<div markdown="1">

```C++

// 보스의 전술적 의사결정 함수
void UBossCombat::DecideNextAction(float distance) {
    NextDecisionTime = GetWorld()->GetTimeSeconds() + FMath::RandRange(1.5f, 3.0f);
    float randomVal = FMath::FRand();

    // 1. 공격 범위 내 진입 시 (우선 처리)
    if (distance <= Stat->AttackRange) {
        if (randomVal < 0.3f) {
            CurrentStanceAction = EStanceAction::BackStep;
            return;
        }
        ExecuteAttack(EAttackType::Melee);
        return;
    }
    // 2. 대치 범위 내 진입 시 돌진 공격
    else if (distance <= Stat->CombatRange && FMath::FRand() > 0.3f) {
        ExecuteAttack(EAttackType::Dash);
        return;
    }

    // 3. 공격 거리가 아닐 때의 위치 선점 전술
    if (randomVal > 0.8f) {
        CurrentStanceAction = EStanceAction::Wait;
    }
    else if (randomVal > 0.4f) {
        CurrentStanceAction = EStanceAction::Strafe;
        Movement->ToggleStrafeDirection();
    }
    else {
        CurrentStanceAction = EStanceAction::Approach;
    }
}

// 전투 대치 상태 업데이트
void ABossController::UpdateCombatStance(float distance) {
    if (distance > Stat->CombatRange + 2.0f) {
        ChangeState(EBossState::Chase);
        return;
    }

    Movement->LookAt(Player->GetActorLocation());

    if (GetWorld()->GetTimeSeconds() >= Combat->NextDecisionTime) {
        Combat->DecideNextAction(distance);
    }

    // 결정된 전술에 따른 이동 실행
    switch (Combat->CurrentStanceAction) {
        case EStanceAction::Strafe:
            Movement->Strafe(Player->GetActorLocation(), Stat->CombatRange, Stat->StrafeSpeed);
            break;
        case EStanceAction::BackStep:
            Movement->ExecuteBackstep(Player->GetActorLocation(), Stat->StrafeSpeed);
            break;
        // ... (생략)
    }
}
```
</div> 
</details>
</br>

</br>

### 3. 선입력(Input Reservation)과 물리 기반 콤보 시스템
플레이어의 조작감을 높이기 위해, 애니메이션 재생 중에 다음 동작을 예약할 수 있는 시스템과 물리적 관성을 제어하는 전투 로직을 구현했습니다.

#### 핵심 구현 내용
- bInputReserved 플래그를 두어, 애니메이션 진행률의 30%가 지난 시점부터 다음 클릭을 미리 인식합니다. 애니메이션 종료 이벤트가 호출될 때 예약된 입력이 있으면 콤보 인덱스를 증가시켜 타격이 매끄럽게 이어집니다.

- 공격 및 구르기 시 Rigidbody::linearVelocity와 angularVelocity를 0으로 강제 초기화하여, 전투 중 물리 엔진의 관성에 의해 캐릭터가 미끄러지거나 의도치 않게 회전하는 현상을 차단했습니다.

<details> 
<summary>콤보 어택 코드</summary> 
<div markdown="1">

```C++
// --- 콤보 어택 로직 ---
public void OnCheckCombo()
{
    // 예약된 입력이 있다면 다음 콤보로 이행
    if (inputReserved)
    {
        comboIndex = (comboIndex + 1) % 3;
        ExecuteAttack();
    }
}

public void OnAttackEnd()
{
    // 예약된 입력이 없다면 공격 상태 완전히 종료
    if (!inputReserved)
    {
        isAttacking = false;
        anim.ResetTrigger("StandAttack");
        comboIndex = 0;
    }
}

private void ExecuteAttack()
{
    // 콤보 제한 시간이 지났다면 처음부터 다시 시작
    if (Time.time - lastAttackTime > comboResetTime)
    {
        comboIndex = 0;
    }

    isAttacking = true;
    inputReserved = false; // 예약 초기화

    anim.SetInteger("ComboIndex", comboIndex);
    anim.SetTrigger("StandAttack");

    // 물리 관성을 제거하여 공격 시 미끄러짐 방지
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
    }

    lastAttackTime = Time.time;
}

```
</div> 
</details>
</br>

</br>

## 트러블 슈팅 및 성능 최적화 (Troubleshooting & Optimization)  

### 1. 상태 전환 시점의 무적 버그 및 애니메이션 이벤트 유실 해결

- 문제 상황: 피격 애니메이션이나 넉백 효과가 재생되면서, 구르기 애니메이션의 종료 이벤트(OnRollEnd)가 호출되지 않아 플레이어의 무적 상태(bIsInvincible)와 구르기 상태(bIsRolling)가 영구적으로 지속되는 치명적인 상태 꼬임 버그가 발생했습니다.

- 해결 과정 (안전장치 구축): 
    1. 애니메이션 이벤트에만 의존하던 기존 구조를 탈피하여, StartRoll 진입 시 타이머(Coroutine/TimerManager) 기반의 강제 종료 안전장치(Safety Timeout) 를 동작시켰습니다.  

    2. 데미지 처리 함수(TakeDamage)가 호출되는 즉시 bIsInvincible = false와 CancelAttack()을 강제 수행하도록 하여 최우선 순위 상태 동기화를 보장했습니다.  

- 결과: 어떠한 프레임 드랍이나 강제 피격 상황에서도 구르기와 전투 상태가 정확히 초기화되어 상태 제어 안정성을 확보했습니다.

<details> 
<summary>구르기 코드</summary> 
<div markdown="1">

```C++
void APlayerMovement::StartRoll(float h, float v) {
    bIsRolling = true;
    Combat->CancelAttack();
    
    // 1. 물리 관성 완벽 제거
    Rb->SetAngularVelocity(FVector::ZeroVector);
    Rb->SetLinearVelocity(FVector::ZeroVector);

    FVector inputDir = FVector(h, 0, v).GetSafeNormal();
    if (inputDir.SizeSquared() > 0.01f) {
        SetActorRotation(inputDir.Rotation());
    }

    Controller->Stats->bIsStunned = true;
    Anim->SetTrigger("Roll");

    // 2. 애니메이션 이벤트 유실 대비 강제 타이머 설정 (예: 1.0초 후 무조건 복구)
    GetWorld()->GetTimerManager().ClearTimer(RollTimerHandle);
    GetWorld()->GetTimerManager().SetTimer(
        RollTimerHandle, 
        this, 
        &APlayerMovement::ForceEndRollRoutine, 
        1.0f, 
        false
    );
}

void APlayerMovement::ForceEndRollRoutine() {
    if (bIsRolling) {
        UE_LOG(LogTemp, Warning, TEXT("구르기 시간 초과 - 안전장치 발동 및 강제 종료"));
        OnRollEnd(); 
    }
}

```
</div> 
</details>
</br>

</br>

### 2. 마우스 레이캐스트 타겟팅 오류 및 회전 튀는 현상 해결

- 문제 상황: 플레이어가 공격을 할 때 종종 마우스를 바라보지 않고 빙글빙글 돌거나 허공으로 꺾이는 현상이 발생했습니다. 원인은 마우스 레이캐스트가 몬스터나 플레이어 자신의 캡슐 콜라이더(Capsule Collider) 표면에 충돌하여 목표 좌표(targetPoint)가 허공으로 설정되었기 때문이었습니다.

- 해결 과정:

    1. LayerMask 비트 연산을 적용하여 레이캐스트가 바닥(Ground) 충돌체만 판별하도록 타겟팅 뎁스를 통제했습니다.

    2. 구르기나 공격 등 '방향 고정'이 필요한 상태(IsAttacking, IsRolling)에서는 FixedUpdate 내의 마우스 추적(LookAtMouse) 회전 로직이 호출되지 않도록 방어 코드를 적용했습니다.

- 결과: 다수의 적이 겹쳐 있는 상황에서도 마우스 조준이 바닥 좌표에 정확히 고정되어, 공격 방향의 신뢰도를 회복하고 전투 조작감이 상승했습니다.

<details> 
<summary>LookAtMouse 함수</summary> 
<div markdown="1">

```C++

void LookAtMouse(float rotationSpeed)
{
    if (combat.IsAttacking || isRolling) return;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
    {
        Vector3 targetPoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        Vector3 lookDir = (targetPoint - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            // 마우스 바라보기
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}

```
</div> 
</details>
</br>

## 회고

### 1. 상태(State) 관리와 동기화의 중요성
이전 'Brotato' 모작 프로젝트에서는 다수의 객체 충돌과 메모리 최적화에 집중했다면, 이번 프로젝트에서는 "단일 객체 내부의 복잡한 상태(공격, 회피, 피격, 기절)를 어떻게 안전하게 제어할 것인가"에 대해 고민했습니다. 이벤트 유실과 물리 엔진의 보간 충돌 등을 겪으며, 애니메이션과 시스템 로직 간의 상태 동기화가 액션 게임의 생명임을 체감했습니다.

### 2. 5일이라는 기한 내의 선택과 집중
단 5일의 짧은 개발 기간이었기에, 불필요하게 복잡한 다중 상속이나 방대한 기능을 구현하기보다는 핵심 재미(전투의 타격감과 조작 반응성)와 코드의 안정성(Crash 및 버그 제로)을 달성하는 데 목표를 두었습니다. 발생한 이슈를 임시방편(Hardcoding)으로 넘기지 않고, 객체 지향적 분리와 타이머 안전장치 같은 구조적 해법으로 대응하며 견고한 프로그래밍 역량을 한 단계 성장시킬 수 있었습니다.

## 마치며

이번 3D PVE 액션 게임 프로젝트는 5일이라는 짧은 기한 내에 "어떻게 하면 가장 안정적이고 확장 가능한 전투 시스템을 구축할 것인가"에 대한 치열한 고민의 결과물입니다.

- AnimationEventReceiver와 인터페이스를 도입하여 뷰(View)와 로직(Logic)을 완벽히 분리하는 객체 지향적 설계의 강점을 체감했고,

- FSM 기반의 AI 설계와 물리/애니메이션 상태 강제 동기화를 통해, 복잡한 전투 상황에서도 예외 없이 안전하게 작동하는 시스템을 구축했으며,

- 짧은 시간의 압박 속에서도 하드코딩의 유혹을 뿌리치고 유지보수가 용이한 견고한 아키텍처를 고집하는 결단력을 길렀습니다.

개발 과정에서 마주친 '구르기 도중 애니메이션 유실로 인한 상태 고착화'같은 이슈들을 단순한 땜질식 처리가 아닌 구조적인 안전장치(Safety Mechanism) 로 해결해 내면서, 기능만 작동하게 만드는 코더(Coder)를 넘어 "문제를 근본적으로 해결하고 견고한 시스템을 설계하는 프로그래머"로 한 단계 성장할 수 있었습니다.

앞으로도 화려한 겉모습에 얽매이기보다는, 흔들리지 않는 튼튼한 코드 뼈대를 설계하고 안정적인 게임플레이 경험을 제공하는 개발자가 되겠습니다.
