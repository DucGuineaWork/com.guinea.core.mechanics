using Guinea.Core.Mechanics;
using UnityEngine;

namespace Guinea.Core.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]KinematicCharacterController m_kinematicCharacterController;

        void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_kinematicCharacterController.IsGrounded)
                {
                    m_kinematicCharacterController.FreeJump(2f);
                }
            }
            m_kinematicCharacterController.MoveRelative(direction.normalized);
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log($"Collide with: {collider.gameObject.name}");
        }
    }
}