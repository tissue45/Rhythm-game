using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(30, 30, 30);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
