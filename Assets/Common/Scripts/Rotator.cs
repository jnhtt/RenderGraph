using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float angleSpeed;

    void Update()
    {
        transform.Rotate(0f, Time.deltaTime * angleSpeed, 0f);
    }
}
