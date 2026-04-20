using System.Collections;
using UnityEngine;

public class UISelector : MonoBehaviour
{
    //===========================================
    // Initializer/Destructor
    //===========================================
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        maxCount = selectPanel.transform.childCount;
        children = new RectTransform[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            children[i] = selectPanel.transform.GetChild(i) as RectTransform;
        }
        maxCount--;
    }

    //===========================================
    // Methods
    //===========================================

    public void SetInitialSelectPosition(int index)
    {
        Initialize();
        if (index < 0 || index > maxCount)
            return;
        baseSelect = selectedIndex = index;
        MoveHighlight();
    }

    public void SetActive(bool value)
    {
        if(value)
        {
            selectHighlight.gameObject.SetActive(true);
            selectedIndex = baseSelect;
            MoveHighlight();
        }
        else
        {
            selectHighlight.gameObject.SetActive(false);
            selectedIndex = baseSelect;
            //MoveHighlight();
        }
    }

    public void MovePosition(float targetZ, float StartZ, float time) { StartCoroutine(Move(targetZ, StartZ, time)); }
    public void MovePosition(float targetZ, float time) { StartCoroutine(Move(targetZ, baseRectTransform.anchoredPosition3D.z, time)); }
    private IEnumerator Move(float targetZ, float StartZ, float time)
    {
        Vector3 departure = baseRectTransform.anchoredPosition3D;
        departure.z = StartZ;
        baseRectTransform.anchoredPosition3D = departure;

        Vector3 arrival = departure;
        arrival.z = targetZ;

        float timer = time;

        while (timer >= 0.0f)
        {
            timer -= Time.deltaTime;
            float percentage = 1.0f - timer / time;

            baseRectTransform.anchoredPosition3D = Vector3.Lerp(departure, arrival, EaseInOutQuad(percentage));
            yield return null;
        }

    }

    static float EaseInOutQuad(float time)
    {
        return time < 0.5f ? 2.0f * time * time : 1.0f - Mathf.Pow(-2.0f * time + 2.0f, 2.0f) * 0.5f;
    }


    public void SelectNext()
    {
        selectedIndex++;
        if (selectedIndex > maxCount)
            selectedIndex = maxCount;
        MoveHighlight();

    }

    public void SelectPrevious()
    {
        if (selectedIndex == -1)
            selectedIndex = maxCount;
        else
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = 0;
        }

        MoveHighlight();
    }

    private void MoveHighlight()
    {
        if(selectedIndex < 0 || selectedIndex > maxCount)
        {
            selectHighlight.gameObject.SetActive(false);
            return;
        }

        selectHighlight.gameObject.SetActive(true);
        selectHighlight.SetParent(children[selectedIndex]);
        selectHighlight.anchoredPosition = new Vector2(selectHighlight.anchoredPosition.x, 0.0f);
    }

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public int Selected { get { return selectedIndex; } private set { } }

    private bool initialized = false;

    private int selectedIndex = -1;
    private int baseSelect = -1;
    private int maxCount = 0;

    //[SerializeField] private float height;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private RectTransform selectHighlight;
    [SerializeField] private RectTransform baseRectTransform;

    private RectTransform[] children;
}
