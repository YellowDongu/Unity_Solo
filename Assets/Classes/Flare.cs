using System;
using UnityEngine;

public class Flare : MonoBehaviour
{
    //===========================================
    // FrameCycle Methods
    //===========================================
    private void Update()
    {
        if (timer < 0.0f)
        {
            particleTimer -= Time.deltaTime;
            if (particleTimer < 0.0f)
                Release();
            return;
        }

        timer -= Time.deltaTime;
        //body.transform.forward = rigidBody.linearVelocity.normalized;
        if (timer < 0.0f)
        {
            body.SetActive(false);
        }
    }

    //===========================================
    // Methods
    //===========================================
    public void Shoot(Vector3 position, Quaternion rotation, float velocity)
    {
        trail.emitting = false;
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        rigidBody.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);
        timer = lifeTime;
        particleTimer = trail.time;
        trail.Clear();
        trail.emitting = true;
    }

    public void Release()
    {
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        if (!gameObject.activeInHierarchy)
            return;
        onRelease?.Invoke(this);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void InjectReleaseMethod(Action<Flare> method) { onRelease = method; }
    public Vehicle Distruption() { return dummy; }

    private float lifeTime = 5.0f;
    private float timer;
    private float particleTimer;
    private Action<Flare> onRelease;
    [SerializeField] private GameObject body;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Vehicle dummy;
}
