using UnityEngine;

public class RemoveXZRotation : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
}
