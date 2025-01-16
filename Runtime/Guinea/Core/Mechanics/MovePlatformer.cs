using UnityEngine;
using DG.Tweening;


namespace Guinea.Core.Mechanics
{
    public class MovePlatformer : MonoBehaviour
    {
        [SerializeField] Vector3 m_delta;
        [SerializeField] float m_duration;
        [SerializeField] Ease m_ease;

        void Start()
        {
            Vector3 delta = transform.TransformDirection(m_delta);
            transform.DOMove(transform.position + delta, m_duration).SetEase(m_ease).SetLoops(-1, LoopType.Yoyo);
        }
    }
}