using UnityEngine;
using UnityEngine.AI;

namespace Guinea.Core.Mechanics
{
    public class AgentController: MonoBehaviour
    {
        [SerializeField]KinematicCharacterController m_kinematicCharacterController;
        [SerializeField]NavMeshAgent m_agent;
        [SerializeField]Rigidbody m_rb;
        [SerializeField]LayerMask m_layer;

#if UNITY_EDITOR
        private Vector3 m_destination;
#endif
        void Start()
        {
            m_agent.updatePosition = false;
            m_agent.updateRotation = false;
            m_agent.updateUpAxis = false;
            m_agent.nextPosition = m_rb.position;
        }

        void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(MousePositionHit(out RaycastHit hit, 100f,m_layer, Camera.main))
                {
                    SetDestination(hit.point);
                }
            }

            if(!m_agent.enabled || m_agent.isStopped)
            {
                return;
            }

            if(m_agent.hasPath)
            {
                m_kinematicCharacterController.SetDesiredVelocity(m_agent.velocity);
                // Vector3 direction  = m_agent.nextPosition - m_rb.position;
                // direction.y = 0f;
                // m_kinematicCharacterController.Move(direction.normalized);
                m_agent.nextPosition = m_rb.position;
            }

            if (!m_agent.pathPending && m_agent.remainingDistance <= m_agent.stoppingDistance)
            {
                Stop();
            }
        }

        public void SetDestination(Vector3 destination)
        {
            m_agent.isStopped = false;
            m_agent.nextPosition = m_rb.position;
            m_agent.destination = destination;
#if UNITY_EDITOR
            m_destination = destination;
#endif
        }

        public void Stop()
        {
            Debug.Log($"Stop Agent: {gameObject.name}");
            m_kinematicCharacterController.SetDesiredVelocity(Vector3.zero);
            m_agent.ResetPath();
            m_agent.nextPosition = m_rb.position;
            m_agent.isStopped = true;
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_destination, 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_agent.nextPosition, 0.2f);
#endif
        }

        public static bool MousePositionHit(out RaycastHit hit, float maxDistance, LayerMask layer, Camera cam=null)
        {
            Ray ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, maxDistance, layer);
        }
    }
}