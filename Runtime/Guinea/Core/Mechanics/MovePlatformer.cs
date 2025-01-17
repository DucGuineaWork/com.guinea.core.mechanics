using UnityEngine;


namespace Guinea.Core.Mechanics
{
    public class MovePlatformer : MonoBehaviour
    {
        [SerializeField] Vector3 m_delta;
        [SerializeField] float m_duration;
        private Vector3 m_startPosition;
        private Vector3 m_delta_;
        

        void Start()
        {
            m_delta_ = transform.TransformDirection(m_delta);
            m_startPosition = transform.position;
        }

        void Update()
        {
            float factor = Mathf.PingPong(Time.time / m_duration, 1f);          
            Vector3 movement = Vector3.Lerp(Vector3.zero, m_delta_, factor);
            transform.position = m_startPosition + movement;
        }
    }
}