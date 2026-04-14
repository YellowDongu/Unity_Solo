using UnityEngine;

public class TankAnimator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(GroundControl mainControl)
    {
        turretYawBone.transform.rotation = Quaternion.identity;
        turretPitchBone.transform.rotation = Quaternion.identity;
        control = mainControl;
    }


    private GroundControl control;
    [SerializeField] private GameObject turretYawBone;
    [SerializeField] private GameObject turretPitchBone;
}
