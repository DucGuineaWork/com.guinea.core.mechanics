// #define GUINEA_CORE_MECHANICS_USE_DOUBLE_JUMP
using UnityEngine;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Guinea.Core.Mechanics
{
    public class DynamicCharacterController : MonoBehaviour
    {
        [SerializeField]Rigidbody m_rb;
        [Header("Hovering")]
        [SerializeField]Transform m_springStartPoint;
        [Tooltip("Length hovering above surface")]
        [SerializeField]float m_springMaxLength;
        [SerializeField]LayerMask m_layerGround;
        [SerializeField]float m_springForce;
        [SerializeField]float m_dampingForce;
        [Header("Upright")]
        [SerializeField]float m_uprightSpringStrength;
        [SerializeField]float m_uprightSpringDamper;
        [Header("Locomotion")]
        [SerializeField]float m_YEuler;
        [SerializeField]float m_maxSpeed;
        [SerializeField]float m_acceleration;
        [SerializeField]AnimationCurve m_accelerationCurveFactor;
        [Header("Jump")]
        [SerializeField]float m_jumpForce;
#if GUINEA_CORE_MECHANICS_USE_DOUBLE_JUMP
        [SerializeField]float m_timeForDoubleJump;
        [SerializeField]float m_doubleJumpForce;
#endif

        private bool m_isGrounded;
        private Vector3 m_moveDir;
        private Vector3 m_goalVel;
        private Vector3 m_hitVel;
        [SerializeField]private bool m_jump;
#if GUINEA_CORE_MECHANICS_USE_DOUBLE_JUMP
        private int m_jumpCount;
        private float m_inTimeForDoubleJump;
#endif
        public event Action OnJump;
        public event Action OnGrounded;
        void Update()
        {
            MoveRelative(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")));
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }

        void FixedUpdate()
        {
            ApplyHoveringForce();
            ApplyUprightForce();
            ApplyLocomotion();
            if(m_jump && m_isGrounded)
            {
                m_jump = false;
                ApplyJump();
            }
        }

        public void Jump()
        {
            if(m_isGrounded)
            {
                m_jump = true;
            }
        }

        public void Move(Vector3 moveDir)
        {
            m_moveDir = moveDir;
        }

        public void MoveRelative(Vector3 moveDir)
        {
            m_moveDir = transform.TransformDirection(moveDir);
        }

        private void ApplyHoveringForce()
        {
            Vector3 rayDir  = -Vector3.up;
            bool isGrounded = Physics.Raycast(m_springStartPoint.position, rayDir, out RaycastHit hit, m_springMaxLength, m_layerGround);
            
            if(isGrounded && !m_isGrounded && m_rb.velocity.y < -5f)
            {
                OnGrounded?.Invoke();
            }
            m_isGrounded = isGrounded;
            if(m_isGrounded)
            {
                m_hitVel = Vector3.zero;
                Rigidbody hitBody = hit.rigidbody;
                if(hitBody!=null)
                {
                    m_hitVel = hitBody.velocity;
                }

                float relVel = Vector3.Dot(m_rb.velocity-m_hitVel, rayDir);
                float springForce = (hit.distance-m_springMaxLength)*m_springForce - relVel * m_dampingForce;

                m_rb.AddForce(rayDir * springForce);

                if(hitBody!=null)
                {
                    hitBody.AddForceAtPosition(-rayDir * springForce, hit.point);
                }
            }
        }

        private void ApplyLocomotion()
        {
            Vector3 goalVel = m_moveDir * m_maxSpeed;
            Vector3 currentVel = m_rb.velocity;
            currentVel.y = m_hitVel.y = 0f;
            float accelerationFactor = m_accelerationCurveFactor.Evaluate(Vector3.Dot(m_moveDir, currentVel.normalized));
            m_goalVel = Vector3.MoveTowards(m_goalVel, goalVel - m_hitVel, accelerationFactor * m_acceleration * Time.fixedDeltaTime);
            Vector3 needAccel = (m_goalVel - currentVel) / Time.fixedDeltaTime;
            if(m_isGrounded)
            {
                m_rb.AddForce(needAccel, ForceMode.Acceleration);
            }
        }

        private void ApplyUprightForce()
        {
            Quaternion targetRot = Quaternion.AngleAxis(m_YEuler, Vector3.up);
            Quaternion rot = ShortestRotation(targetRot, m_rb.rotation);

            Vector3 rotAxis;
            float rotDegrees;

            rot.ToAngleAxis(out rotDegrees, out rotAxis);
            rotAxis.Normalize();

            float rotRad = rotDegrees * Mathf.Deg2Rad;
            m_rb.AddTorque(rotAxis*rotRad * m_uprightSpringStrength - m_rb.angularVelocity * m_uprightSpringDamper);
        }

        private void  ApplyJump()
        {
            OnJump?.Invoke(); 
#if GUINEA_CORE_MECHANICS_USE_DOUBLE_JUMP
            if(m_jumpCount==1 && Time.time < m_inTimeForDoubleJump)
            {
                m_rb.AddForce(m_doubleJumpForce * Vector3.up, ForceMode.VelocityChange);
                m_jumpCount = 0;
            }
            else
            {
                m_rb.AddForce(m_jumpForce * Vector3.up, ForceMode.VelocityChange);
                m_jumpCount = 1;
                m_inTimeForDoubleJump = Time.time + m_timeForDoubleJump;
            }
#else
            m_rb.AddForce(m_jumpForce * Vector3.up, ForceMode.VelocityChange);
#endif
        }

        void OnDrawGizmos()
        {
            if (m_springStartPoint == null)
            {
                return;
            }
#if UNITY_EDITOR
            Handles.color = Color.green;
            Handles.DrawLine(m_springStartPoint.position, m_springStartPoint.position - m_springMaxLength * Vector3.up, 8.0f);
#endif
        }

        public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }
            else return a * Quaternion.Inverse(b);
        }

        public static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
