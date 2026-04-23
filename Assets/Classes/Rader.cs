using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Rader : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        layerMask = LayerMask.GetMask("MovingVehicle");
        targetLayer = LayerMask.NameToLayer("MovingVehicle");
        raderDistance = GameMaster.ConvertWorldScale(raderDistance);
        collider.radius = raderDistance;
        rpm = 1.0f / (rpm / 60.0f);
    }

    //===========================================
    // Methods
    //===========================================
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != targetLayer)
            return;

        float distance = Vector3.Distance(gameObject.transform.position, other.gameObject.transform.position);

        Vehicle component = null;
        if (other.gameObject.transform.parent != null)
            component = other.gameObject.transform.parent.gameObject.GetComponent<Vehicle>();
        else
            component = other.gameObject.GetComponent<Vehicle>();

        if (component == null)
            return;

        inDistance.Add(component);
        enterEvent?.Invoke(component);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != targetLayer)
            return;

        Vehicle component = null;
        if (other.gameObject.transform.parent != null)
            component = other.gameObject.transform.parent.gameObject.GetComponent<Vehicle>();
        else
            component = other.gameObject.GetComponent<Vehicle>();

        if (component == null)
            return;

        if (inDistance.Remove(component))
            exitEvent?.Invoke(component);
    }

    public void Remove(List<Vehicle> targets)
    {
        foreach (Vehicle current in targets)
        {
            int max = inDistance.Count;
            for (int i = 0; i < max; i++)
            {
                if (inDistance[i] != current)
                    continue;

                inDistance.RemoveAt(i);
                break;
            }
        }
    }

    public void DeployFlare(AudioSource source, AudioClip sound)
    {
        if (deploying)
            return;

        if (flareCount <= 0)
            return;

        StartCoroutine(Deploy(source, sound));
        StartCoroutine(FlareCoolTime());
    }

    public IEnumerator Deploy(AudioSource source, AudioClip sound)
    {
        deploying = true;
        int Count = deployCount;
        float Timer = 0;
        while (Count > 0)
        {
            Timer += Time.deltaTime;
            if (Timer < rpm)
            {
                yield return null;
                continue;
            }
            Timer = 0.0f;
            Count--;

            if (tracing.Count > 0)
            {
                int count = (tracing.Count + flarePod.Length - 1) / flarePod.Length; // ºü¸¥ ³ª´°¼À ¿Ã¸²
                int i = 0, length;
                Missile[] collection = tracing.ToArray();
                tracing.Clear();

                foreach (var item in flarePod)
                {
                    Flare newInstance = GameMaster.Instance.Factory.ShootFlare(item);
                    length = Mathf.Clamp(i + count, 0, collection.Length);
                    for (; i < length; i++)
                    {
                        collection[i].ChangeTarget(newInstance.Distruption());
                        collection[i].MissileWarning -= Warning;
                    }
                    i += count;
                }
                source.PlayOneShot(sound);// GameMaster.GetInstance().Sound().PlayOnce(source, sound);
            }
            else
            {
                foreach (var item in flarePod)
                    GameMaster.Instance.Factory.ShootFlare(item);
                source.PlayOneShot(sound);// GameMaster.GetInstance().Sound().PlayOnce(source, sound);
            }

            yield return null;
        }
        deploying = false;
    }

    public IEnumerator Deploy()
    {
        deploying = true;
        int Count = deployCount;
        float Timer = 0;
        while (Count > 0)
        {
            Timer += Time.deltaTime;
            if (Timer < rpm)
            {
                yield return null;
                continue;
            }
            Timer = 0.0f;
            Count--;

            if(tracing.Count > 0)
            {
                int count = (tracing.Count + flarePod.Length - 1) / flarePod.Length; // ºü¸¥ ³ª´°¼À ¿Ã¸²
                int i = 0, length;
                Missile[] collection = tracing.ToArray();
                tracing.Clear();

                foreach (var item in flarePod)
                {
                    Flare newInstance = GameMaster.Instance.Factory.ShootFlare(item);
                    length = Mathf.Clamp(i + count, 0, collection.Length);
                    for (; i < length; i++)
                    {
                        collection[i].ChangeTarget(newInstance.Distruption());
                        collection[i].MissileWarning -= Warning;
                    }
                    i += count;
                }
            }
            else
                foreach (var item in flarePod)
                    GameMaster.Instance.Factory.ShootFlare(item);

            yield return null;
        }
        deploying = false;
    }

    public void AutoDeployFlare()
    {
        if (!autoFlare || deploying)
            return;

        if (flareCount <= 0)
            return;

        if (tracing.Count >= 1)
        {
            StartCoroutine(Deploy());
            StartCoroutine(FlareCoolTime());
        }
    }

    public IEnumerator FlareCoolTime(Action<float> coolTimeMethod = null)
    {
        flareCount--;
        float timer = flareCoolTime;

        if (coolTimeMethod == null)
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        else
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                coolTimeMethod(timer);
                yield return null;
            }

        flareCount++;
    }

    public void Warning(float sqrDistance)
    {
        uiWarningEvent?.Invoke(sqrDistance < 10000.0f);
    }
    public void Trace(Missile target) { tracing.Add(target); target.MissileWarning += Warning; AutoDeployFlare(); }
    public void TraceEnd(Missile target) { tracing.Remove(target); target.MissileWarning -= Warning; }

    public delegate void RaderEvent(Vehicle target);
    public delegate void WarningUIEvent(bool inDistance);
    public event RaderEvent enterEvent;
    public event RaderEvent exitEvent;
    public event WarningUIEvent uiWarningEvent;
    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public bool MissileAlart { get { return tracing.Count != 0; } set { } }
    public float RaderDistance { get { return raderDistance; } set { raderDistance = value; collider.radius = raderDistance; } }
    public ReadOnlyCollection<Vehicle> InRangeTarget => inDistance.AsReadOnly();
    public void SetTeam(int value) { team = value; }
    public void SetHP(int value) { hp = value; }
    public void SetFlareCoolTime(float value) { flareCoolTime = value; }
    public void SetAutoFlare() { autoFlare = true; }


    private bool deploying = false;
    private bool autoFlare = false;
    private int flareCount = 2;
    private int team;
    private int hp;
    private int layerMask, targetLayer;
    private HashSet<Missile> tracing = new HashSet<Missile>(50);
    private List<Vehicle> inDistance = new List<Vehicle>(10);

    [SerializeField] private float flareCoolTime = 50.0f;
    [SerializeField] private float rpm = 550.0f;
    [SerializeField] private int deployCount = 15;
    [SerializeField] private float raderDistance = 1000.0f;
    [SerializeField] private SphereCollider collider = null;
    [SerializeField] private GameObject[] flarePod;
}
