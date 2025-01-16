using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Guinea.Core.Mechanics
{
    public class KinematicCharacterController: MonoBehaviour
    {
        [SerializeField]Rigidbody m_rb;
        [Header("Wall Detection")]
        [SerializeField]CapsuleCollider m_capsuleCollider;
        private Vector3 m_point0;
        private Vector3 m_point1;
        [Header("Ground Detection")]
        [SerializeField]Transform m_checkPoint;
        [SerializeField]float m_maxLength;
        [SerializeField]LayerMask m_layerMask;
        private Vector3 m_moveDir;
        [SerializeField]float m_speed;
        [SerializeField]float m_acceleration;
        private bool m_isGrounded;
        private float m_yVelocityAdjustment;
        private Vector3 m_currentVelocity;

        void Start()
        {
            m_point0 = m_capsuleCollider.center + Vector3.up * (m_capsuleCollider.height / 2 - m_capsuleCollider.radius);
            m_point1 = m_capsuleCollider.center - Vector3.up * (m_capsuleCollider.height / 2 - m_capsuleCollider.radius);
        }

        void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
            MoveRelative(direction.normalized);
        }

        void FixedUpdate()
        {
            ApplyHoveringForce();
            ApplyLocomotion();
            m_rb.MovePosition(m_rb.position + m_currentVelocity * Time.fixedDeltaTime);
        }

        public void Move(Vector3 moveDir)
        {
            m_moveDir = moveDir;
        }

        public void MoveRelative(Vector3 moveDir)
        {
            m_moveDir = transform.TransformDirection(moveDir);
        }

        private void ApplyLocomotion()
        {
            if(!CastRay(m_moveDir,out RaycastHit hit, 0.2f))
            {
                Vector3 velocity = m_moveDir * m_speed;
                Vector3 currentVelocity = m_currentVelocity;
                currentVelocity.y = 0f;
                velocity = Vector3.MoveTowards(currentVelocity, velocity, m_acceleration * Time.fixedDeltaTime);
                m_currentVelocity.x = velocity.x;
                m_currentVelocity.z = velocity.z;
            }
            else
            {
                m_currentVelocity.x = m_currentVelocity.z = 0f;
            }
        }

        private bool CastRay(Vector3 direction,out RaycastHit hit, float maxDistance=Mathf.Infinity)
        {
            Vector3 point0 = m_point0 + m_capsuleCollider.transform.position;
            Vector3 point1 = m_point1 + m_capsuleCollider.transform.position;
            return Physics.CapsuleCast(point0, point1, m_capsuleCollider.radius, direction, out hit, maxDistance, m_layerMask);
        }

        private void ApplyHoveringForce()
        {
            m_isGrounded = Physics.Raycast(m_checkPoint.position, -m_checkPoint.up, out RaycastHit hit, m_maxLength, m_layerMask);
            if(m_isGrounded)
            {
                m_yVelocityAdjustment = 0.5f * (m_maxLength - hit.distance)/ Time.fixedDeltaTime; // TODO: Adjustment with threshold
                m_currentVelocity.y = 0f;
            }
            else
            {
                m_yVelocityAdjustment = Physics.gravity.y * Time.fixedDeltaTime;
            }
            m_currentVelocity.y += m_yVelocityAdjustment;
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
            if(Application.isPlaying)
            {
                Vector3 point0 =m_point0 + m_capsuleCollider.transform.position + m_moveDir * m_speed * Time.fixedDeltaTime;
                Vector3 point1 =m_point1 + m_capsuleCollider.transform.position + m_moveDir * m_speed * Time.fixedDeltaTime;
                DrawWireCapsule(point0, point1, m_capsuleCollider.radius, m_capsuleCollider.height, Color.yellow);
            }
#endif
        }

#if UNITY_EDITOR
    public static void DrawWireCapsule(Vector3 _pos, Vector3 _pos2, float _radius, float _height, Color _color = default)
    {
        if (_color != default) Handles.color = _color;

        var forward = _pos2 - _pos;
        var _rot = Quaternion.LookRotation(forward);
        var pointOffset = _radius/2f;
        var length = forward.magnitude;
        var center2 = new Vector3(0f,0,length);
       
        Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
       
        using (new Handles.DrawingScope(angleMatrix))
        {
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.left * pointOffset, -180f, _radius);
            Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.down * pointOffset, -180f, _radius);
            Handles.DrawWireDisc(center2, Vector3.forward, _radius);
            Handles.DrawWireArc(center2, Vector3.up, Vector3.right * pointOffset, -180f, _radius);
            Handles.DrawWireArc(center2, Vector3.left, Vector3.up * pointOffset, -180f, _radius);
           
            DrawLine(_radius,0f,length);
            DrawLine(-_radius,0f,length);
            DrawLine(0f,_radius,length);
            DrawLine(0f,-_radius,length);
        }
    }

    private static void DrawLine(float arg1,float arg2,float forward)
    {
        Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward));
    }
#endif 
        
    }
}