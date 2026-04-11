using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Rader : MonoBehaviour
{
    private void Awake()
    {
        layerMask = LayerMask.GetMask("MovingVehicle");
        targetLayer = LayerMask.NameToLayer("MovingVehicle");
        collider.radius = raderDistance;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != targetLayer)
            return;

        float distance = Vector3.Distance(gameObject.transform.position, other.gameObject.transform.position);

        Vehicle component = null;
        if (other.gameObject.transform.parent != null)
            component = other.gameObject.transform.parent.gameObject.GetComponent<Vehicle>();
        if (component == null)
        {
            component = other.gameObject.GetComponent<Vehicle>();
            if (component == null)
                return;
        }

        inDistance.Add(component);
        enterEvent?.Invoke(component);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != targetLayer)
            return;

        Vehicle component = other.gameObject.transform.parent.gameObject.GetComponent<Vehicle>();
        if (component == null)
        {
            component = other.gameObject.GetComponent<Vehicle>();
            if (component == null)
                return;
        }
        if (inDistance.Remove(component))
            exitEvent?.Invoke(component);
    }

    public void Remove(List<int> targets)
    {
        foreach (int index in targets)
            inDistance.RemoveAt(index);

    }

    public delegate void EnterEvent(Vehicle target);
    public delegate void ExitEvent(Vehicle target);

    public void Trace(Missile target) { tracing.Add(target); }
    public void TraceEnd(Missile target) { tracing.Remove(target); }
    public float RaderDistance() { return raderDistance; }
    public void ChangeRaderDistance(float value) { raderDistance = value; collider.radius = raderDistance; }
    public ReadOnlyCollection<Vehicle> InRangeTarget => inDistance.AsReadOnly();
    public void SetTeam(int value) { team = value; }

    public event EnterEvent enterEvent;
    public event EnterEvent exitEvent;

    private int team;
    private int layerMask, targetLayer;
    private HashSet<Missile> tracing = new HashSet<Missile>(50);
    private List<Vehicle> inDistance = new List<Vehicle>(10);
    [SerializeField] float raderDistance = 1000.0f;
    [SerializeField] private SphereCollider collider = null;

}
