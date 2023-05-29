using UnityEngine;

public class UnitAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator = default;
    [SerializeField] private GameObject gunObject = default;

    private int RunAnimation = Animator.StringToHash("Run");
    private int LookingAnimation = Animator.StringToHash("looking");
    private int HappyAnimation = Animator.StringToHash("Happy");

    public Animator Animator => animator;

    public void SetAnimation(UnitState unitState)
    {
        if (animator != null)
        {
            animator.SetBool(RunAnimation, false);
            animator.SetBool(LookingAnimation, false);
            animator.SetBool(HappyAnimation, false);
            gunObject.SetActive(true);

            switch (unitState)
            {
                case UnitState.Move:
                    animator.SetBool(RunAnimation, true);
                    break;
                case UnitState.Looking:
                    animator.SetBool(LookingAnimation, true);
                    break;
                case UnitState.Happy:
                    animator.SetBool(HappyAnimation, true);
                    gunObject.SetActive(false);
                    break;
            }
        }
    }
}
