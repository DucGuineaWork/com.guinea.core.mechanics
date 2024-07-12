using System;
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
        float forward = Mathf.Clamp(m_localVelocity.z, -1f, 1f);
        float turn = m_localVelocity.x;
        m_animator.SetFloat(s_forwardHash, forward);
        m_animator.SetFloat(s_turnHash, turn);
        Vector3 velWithoutY = new Vector3(m_rb.velocity.x, 0f, m_rb.velocity.z);
        m_animator.SetFloat(s_speedHash, Mathf.Abs(velWithoutY.sqrMagnitude)< Mathf.Epsilon ? 1f: velWithoutY.magnitude/2f);
    }
}
