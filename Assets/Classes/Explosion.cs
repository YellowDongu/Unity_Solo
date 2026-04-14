using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        emitParameter = new ParticleSystem.EmitParams();
        emitParameter.applyShapeToPosition = false;
    }

    //===========================================
    // Methods
    //===========================================

    public IEnumerator UpdateParticle()
    {
        particleSystem.Play();
        GameMaster.GetInstance().Sound().PlayOnce("Explosion", gameObject.transform.position);
        while (true)
        {
            if (!particleSystem.IsAlive())
            {
                Release();
                break;
            }
            gameObject.transform.position = target.transform.position;

            yield return null;
        }
    }

    public void Release()
    {
        particleSystem.Stop();
        release?.Invoke(this);
    }

    //public void Emit(GameObject target, float size = 5.0f)
    public void Emit(GameObject target)
    {
        if (target == null)
            return;

        StartCoroutine(UpdateParticle());
    }

    public void Emit(Vector3 worldPos, float size = 5.0f)
    {
        if (particleSystem.particleCount >= particleSystem.main.maxParticles)
            particleSystem.Clear();

        emitParameter.position = worldPos;
        emitParameter.startSize = size;

        particleSystem.Emit(emitParameter, 1);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    private GameObject target;

    ParticleSystem.EmitParams emitParameter;
    public delegate void ReleaseMethod(Explosion explosion);
    public event ReleaseMethod release;

    [SerializeField] private ParticleSystem particleSystem = null;


}
