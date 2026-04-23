using UnityEngine;

public class GroundMovement : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================

    void Start()
    {
        CalibrateGround();
    }

    //===========================================
    // Methods
    //===========================================
    private void CalibrateGround()
    {
        int terrain = LayerMask.GetMask("Terrain");
        int layer = LayerMask.NameToLayer("Terrain");
        float distance = 5000.0f;
        if (Physics.Raycast(gameObject.transform.position + Vector3.up * distance * 0.5f, Vector3.down, out RaycastHit hit, distance, terrain))
        {
            if (hit.collider.gameObject.layer == layer)
            {
                gameObject.transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public GroundControl control;
}
