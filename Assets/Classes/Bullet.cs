using System;
using UnityEngine;
using UnityEngine.Pool;


public class Bullet : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    void Start()
    {
        //layerMask = LayerMask.GetMask("MovingVehicle");
        targetLayer = LayerMask.NameToLayer("MovingVehicle");
        terrainLayer = LayerMask.NameToLayer("Terrain");

    }

    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if(timer < 0.0f || gameObject.transform.position.y < 0.0f)
        {
            Release();
            return;
        }

        timer -= Time.deltaTime;
    }

    //===========================================
    // Methods
    //===========================================
    public void Shoot(Vector3 position, Quaternion rotation, float velocity, Vehicle _shooted, ObjectPool<Bullet> objectPool)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        shooted = _shooted;
        rigidBody.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);
        rigidBody.useGravity = true;
        onRelease = objectPool.Release;
        timer = 5.0f;
    }

    public void ShootNonGravity(Vector3 position, Quaternion rotation, float velocity, Vehicle _shooted, ObjectPool<Bullet> objectPool)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        shooted = _shooted;
        rigidBody.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);
        rigidBody.useGravity = false;
        onRelease = objectPool.Release;
        timer = 5.0f;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!gameObject.activeInHierarchy)
            return;
        int layer = collision.collider.gameObject.layer;
        if (layer == targetLayer)
        {
            Vehicle vehicle = collision.gameObject.GetComponent<Vehicle>();

            if(vehicle != null && shooted != null)
            {
                if (shooted == vehicle || shooted.Team == vehicle.Team)
                    return;

                vehicle.TakeDamage(5);
            }

            Release();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeInHierarchy)
            return;
        int layer = other.gameObject.layer;
        if (layer == targetLayer)
        {
            Vehicle vehicle = other.attachedRigidbody.gameObject.GetComponent<Vehicle>();

            if (vehicle != null && shooted != null)
            {
                if (shooted == vehicle || shooted.Team == vehicle.Team)
                    return;

                vehicle.TakeDamage(5);
            }

            Release();
        }
    }

    public void Release()
    {
        if (!gameObject.activeInHierarchy)
            return;
        timer = 5.0f;
        shooted = null;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        onRelease?.Invoke(this);
    }
    //===========================================
    // Variable & GetSet Methods
    //===========================================

    public void InjectReleaseMethod(Action<Bullet> method) { onRelease = method; }

    private float timer;
    private int layerMask;
    private int targetLayer;
    private int terrainLayer;
    private Action<Bullet> onRelease;
    private Vehicle shooted;
    [SerializeField] private Rigidbody rigidBody;
}
