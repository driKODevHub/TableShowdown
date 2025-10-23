using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] Vector3 rotationAxis = Vector3.up;

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(rotationAxis, rotationSpeed * Time.fixedDeltaTime);
    }
}
