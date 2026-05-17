using UnityEngine;

public class RollRootMotion : StateMachineBehaviour
{
    // 애니메이션 상태에 진입할 때 (구르기 시작 프레임)
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 루트 모션을 켜서 애니메이션의 이동을 실제 좌표에 적용
        animator.applyRootMotion = true;
    }

    // 애니메이션 상태를 빠져나올 때 (구르기 종료 프레임)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 구르기가 끝나면 루트 모션을 끄고 다시 스크립트 이동으로 복귀
        animator.applyRootMotion = false;
    }
}
