using Guinea.Core.Mechanics;
using UnityEngine;

public class DynamicAnimatorController : MonoBehaviour
{
    [SerializeField]Rigidbody m_rb;
    [SerializeField]Animator m_animator;
    [SerializeField]Vector3 m_localVelocity;
    [SerializeField]DynamicCharacterController m_dynamicCharacterController;
    private static readonly int s_forwardHash = Animator.StringToHash("Forward");
    private static readonly int s_turnHash = Animator.StringToHash("Turn");
    private static readonly int s_speedHash = Animator.StringToHash("Speed");
    private static readonly int s_jumpHash = Animator.StringToHash("Jump");
    private static readonly int s_landingHash = Animator.StringToHash("Landing");
    private static readonly int s_locomotionHash = Animator.StringToHash("Locomotion");

    void OnEnable()
    {
        m_dynamicCharacterController.OnJump += OnJump;
        m_dynamicCharacterController.OnGrounded += OnGrounded;
    }

    void OnDisable()
    {
        m_dynamicCharacterController.OnJump -= OnJump;
        m_dynamicCharacterController.OnGrounded -= OnGrounded;
    }

    private void OnGrounded()
    {
        m_animator.SetTrigger(s_landingHash);
    }

    private void OnJump()
    {
        m_animator.SetTrigger(s_jumpHash);
    }

    void Update()
    {
        m_localVelocity = transform.InverseTransformVector(m_rb.velocity);
        float forward = m_localVelocity.z;
        float turn = m_localVelocity.x;
        m_animator.SetFloat(s_forwardHash, forward);
        m_animator.SetFloat(s_turnHash, turn);
        Vector3 velWithoutY = new Vector3(turn, 0f, forward);
        bool isLocomotion = Mathf.Abs(velWithoutY.sqrMagnitude) > 0.1f;
        if (!m_animator.GetBool(s_locomotionHash) && isLocomotion)
        {
            m_animator.SetTrigger(s_locomotionHash);
        }
        m_animator.SetFloat(s_speedHash, isLocomotion ? velWithoutY.magnitude/2f:1f);
    }
}
