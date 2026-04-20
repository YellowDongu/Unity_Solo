using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class InSelectCamera : MonoBehaviour
{
    //===========================================
    // FrameCycle Methods
    //===========================================
    void Update()
    {
        if (Keyboard.current.leftArrowKey.isPressed)
            gameObject.transform.rotation *= Quaternion.Euler(0.0f, -rotationSpeed * Time.deltaTime, 0.0f);
        if (Keyboard.current.rightArrowKey.isPressed)
            gameObject.transform.rotation *= Quaternion.Euler(0.0f, rotationSpeed * Time.deltaTime, 0.0f);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    [SerializeField] private float rotationSpeed;
}
