using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class IFFUIController : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        ObjectPool<IFFHud> newPool = new ObjectPool<IFFHud>(
            createFunc: () => Instantiate(prefabs[0], screen.transform).GetComponent<IFFHud>(),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Destroy(obj.gameObject),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
        );

        //changeColor += ColorChange;
        pool.Add("Default", (prefabs[0], newPool));
    }


    //void Start()
    //{
    //    HUDController controller = gameObject.GetComponent<HUDController>();
    //    controller.changeColor += (color) => changeColor?.Invoke(color);
    //    changeColor?.Invoke(controller.GetColor());
    //}

    //===========================================
    // Methods
    //===========================================

    public void Select(Vehicle target, bool select)
    {
        if(!targets.TryGetValue(target, out IFFHud hud))
            return;

        hud.SetTarget(select);
    }

    public IFFHud AttachIFF(Vehicle _target, Player _player, Aircraft playerAircraft)
    {
        if (targets.ContainsKey(_target))
            return null;

        ObjectPool<IFFHud> targetPool = pool["Default"].pool;
        IFFHud hud = targetPool.Get();
        hud.Attach(_target, _player, playerAircraft);
        targets.Add(_target, hud);
        hud.SetMaxDistance(maxDistance * 1.1f);
        //changeColor += hud.ChangeColor;
        hud.release += ReleaseMethod;
        hud.release += targetPool.Release;
        return hud;
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    //private void ColorChange(Color _color) { color = _color; }
    private void ReleaseMethod(IFFHud target) { /*changeColor -= target.ChangeColor;*/ targets.Remove(target.Target); }
    public void SetMaxDistance(float value) { maxDistance = value; }
    public IFFHud GetIFF(Vehicle target) { if(targets.TryGetValue(target, out IFFHud result)) return result; return null; }

    //public delegate void ChangeColor(Color color);
    //public event ChangeColor changeColor;

    private Dictionary<string, (GameObject prefab, ObjectPool<IFFHud> pool)> pool = new Dictionary<string, (GameObject prefab, ObjectPool<IFFHud> pool)>();

    Color color;
    float maxDistance = 1000.0f;
    Dictionary<Vehicle, IFFHud> targets = new Dictionary<Vehicle, IFFHud>();
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private GameObject screen;
    [SerializeField] private Rader playerRader;
}
