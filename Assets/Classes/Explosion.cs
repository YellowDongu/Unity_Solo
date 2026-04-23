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
        GameMaster.Instance.Sound.PlayOnce("Explosion", gameObject.transform.position);
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

    public delegate void ReleaseMethod(Explosion explosion);
    public event ReleaseMethod release;

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    private GameObject target;
    private ParticleSystem.EmitParams emitParameter;
    [SerializeField] private ParticleSystem particleSystem = null;
}
