using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float angleXSpeed;
    [SerializeField] private float angleYSpeed;

    void Update()
    {
        transform.Rotate(Time.deltaTime * angleXSpeed, Time.deltaTime * angleYSpeed, 0f);
    }
}
