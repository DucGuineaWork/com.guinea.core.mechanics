using UnityEngine;


namespace Guinea.Core.Mechanics
{
    public class MovePlatformer : MonoBehaviour
    {
        [SerializeField] Vector3 m_delta;
        [SerializeField] float m_duration;
        private Vector3 m_startPosition;
        private float m_startTime;
        private Vector3 m_delta_;
        

        void Start()
        {
            m_delta_ = transform.TransformDirection(m_delta);
            m_startTime = Time.time;
            m_startPosition = transform.position;
        }

        void Update()
        {
            float timeElapsed = (Time.time - m_startTime) % m_duration; 
            float factor = timeElapsed / m_duration;             

            Vector3 movement = Vector3.Lerp(Vector3.zero, m_delta_, Mathf.PingPong(factor, 1f));
            transform.position = m_startPosition + movement;
        }
    }
}