using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Guinea.Core.Mechanics
{
    public class KinematicCharacterController : MonoBehaviour
    {
        [SerializeField] Rigidbody m_rb;
        [Header("Wall Detection")]
        [SerializeField] CapsuleCollider m_capsuleCollider;
        private Vector3 m_point0;
        private Vector3 m_point1;
        [Header("Ground Detection")]
        [SerializeField] Transform m_checkPoint;
        [SerializeField] float m_maxLength;
        [SerializeField] LayerMask m_layerMask;
        [SerializeField] string m_platformerTag;
        private Vector3 m_moveDir;
        [SerializeField] float m_speed;
        [SerializeField] float m_acceleration;
        private bool m_isGrounded;
        private bool m_isOverlapped;

        private float m_yVelocityAdjustment;
        private Vector3 m_currentVelocity;
        // private Transform m_platformer;
        private float m_jumpVelocity;

        void Start()
        {
            m_point0 = m_capsuleCollider.center + Vector3.up * (m_capsuleCollider.height / 2 - m_capsuleCollider.radius);
            m_point1 = m_capsuleCollider.center - Vector3.up * (m_capsuleCollider.height / 2 - m_capsuleCollider.radius);
        }

        void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_isGrounded)
                {
                    FreeJump(2f);
                }
            }
            MoveRelative(direction.normalized);
        }

        void FixedUpdate()
        {
            m_isGrounded = ApplyHoveringForce(out RaycastHit hit);
            m_isOverlapped = CheckCapsule();
            FreeLocomotion();
            if (m_isGrounded)
            {
                m_currentVelocity.y = 0f;
                if (hit.collider.gameObject.CompareTag(m_platformerTag))
                {
                    transform.parent = hit.transform;
                }
            }
            else
            {
                transform.parent = null;
            }

            if (transform.parent != null)
            {
                if (!m_isOverlapped)
                {
                    m_currentVelocity.y += m_jumpVelocity;
                    transform.localPosition += transform.InverseTransformDirection(m_currentVelocity) * Time.fixedDeltaTime;
                }
            }
            else
            {
                if (!m_isOverlapped)
                {
                    m_currentVelocity.y += m_yVelocityAdjustment + m_jumpVelocity;
                    m_rb.MovePosition(m_rb.position + m_currentVelocity * Time.fixedDeltaTime);
                }
            }
            m_jumpVelocity = 0f;
        }

        public void FreeJump(float height)
        {
            m_jumpVelocity = Mathf.Sqrt(-2f * Physics.gravity.y * height);
        }

        public void Move(Vector3 moveDir)
        {
            m_moveDir = moveDir;
        }

        public void MoveRelative(Vector3 moveDir)
        {
            m_moveDir = transform.TransformDirection(moveDir);
        }

        private void FreeLocomotion()
        {
            Vector3 currentVelocity = m_currentVelocity;
            currentVelocity.y = 0f;
            if (!m_isOverlapped)
            {
                if (m_isGrounded)
                {
                    Vector3 velocity = m_moveDir * m_speed;
                    velocity = Vector3.MoveTowards(currentVelocity, velocity, m_acceleration * Time.fixedDeltaTime);
                    m_currentVelocity.x = velocity.x;
                    m_currentVelocity.z = velocity.z;
                }
            }
            else
            {
                m_currentVelocity.x = 0f;
                m_currentVelocity.z = 0f;
                m_rb.position = m_rb.position - currentVelocity.normalized * m_capsuleCollider.radius * 0.2f;
            }
        }

        private bool CheckCapsule()
        {
            Vector3 point0 = m_point0 + m_rb.position;
            Vector3 point1 = m_point1 + m_rb.position;
            return Physics.CheckCapsule(point0 + m_moveDir * 0.1f, point1 + m_moveDir * 0.1f, m_capsuleCollider.radius, m_layerMask);
        }

        private bool ApplyHoveringForce(out RaycastHit hit)
        {
            bool isGrounded = Physics.Raycast(m_checkPoint.position, -m_checkPoint.up, out hit, m_maxLength, m_layerMask);
            if (isGrounded)
            {
                m_yVelocityAdjustment = 0.5f * (m_maxLength - hit.distance) / Time.fixedDeltaTime; // TODO: Adjustment with threshold
            }
            else
            {
                m_yVelocityAdjustment = Physics.gravity.y * Time.fixedDeltaTime;
            }
            return isGrounded;
        }

        void OnDrawGizmos()
        {
            if (m_checkPoint == null)
            {
                return;
            }
#if UNITY_EDITOR
            Handles.color = Color.green;
            Handles.DrawLine(m_checkPoint.position, m_checkPoint.position - m_maxLength * Vector3.up, 8.0f);
            if (Application.isPlaying)
            {
                Vector3 point0 = m_point0 + m_rb.position;
                Vector3 point1 = m_point1 + m_rb.position;
                DrawWireCapsule(point0 + m_moveDir * m_capsuleCollider.radius * 0.1f, point1 + m_moveDir * m_capsuleCollider.radius * 0.1f, m_capsuleCollider.radius, Color.yellow);
            }
#endif
        }

#if UNITY_EDITOR
        public static void DrawWireCapsule(Vector3 pos, Vector3 pos2, float radius, Color color = default)
        {
            Handles.color = color;

            var forward = pos2 - pos;
            var rot = Quaternion.LookRotation(forward);
            var pointOffset = radius / 2f;
            var length = forward.magnitude;
            var center2 = new Vector3(0f, 0, length);

            Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, Handles.matrix.lossyScale);

            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
                Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.left * pointOffset, -180f, radius);
                Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.down * pointOffset, -180f, radius);
                Handles.DrawWireDisc(center2, Vector3.forward, radius);
                Handles.DrawWireArc(center2, Vector3.up, Vector3.right * pointOffset, -180f, radius);
                Handles.DrawWireArc(center2, Vector3.left, Vector3.up * pointOffset, -180f, radius);

                DrawLine(radius, 0f, length);
                DrawLine(-radius, 0f, length);
                DrawLine(0f, radius, length);
                DrawLine(0f, -radius, length);
            }
        }

        private static void DrawLine(float arg1, float arg2, float forward)
        {
            Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward));
        }
#endif
    }
}