using System;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    void Start()
    {
        //layerMask = LayerMask.GetMask("MovingVehicle");
        targetLayer = LayerMask.NameToLayer("MovingVehicle");
        terrainLayer = LayerMask.NameToLayer("Terrain");

    }

    private void Update()
    {
        if(timer < 0.0f || gameObject.transform.position.y < 0.0f)
        {
            onRelease?.Invoke(this);
            return;
        }

        timer -= Time.deltaTime;
    }

    public void Shoot(Vector3 position, Quaternion rotation, float velocity, Vehicle _shooted, ObjectPool<Bullet> objectPool)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        shooted = _shooted;
        rigidBody.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);
        onRelease = objectPool.Release;
        timer = 5.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == terrainLayer)
            onRelease?.Invoke(this);
        else if (collision.gameObject.layer == targetLayer)
        {
            Aircraft aircraft = collision.gameObject.GetComponent<Aircraft>();
            if (shooted.Team == aircraft.Team)
                return;

            aircraft.TakeDamage(5);

            shooted = null;
            onRelease?.Invoke(this);
        }

    }

    private float timer;
    private int layerMask;
    private int targetLayer;
    private int terrainLayer;
    private Action<Bullet> onRelease;
    private Vehicle shooted;
    [SerializeField] private Rigidbody rigidBody;
}
