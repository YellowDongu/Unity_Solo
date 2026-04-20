using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    private void Awake()
    {
        if(fadeOutPrefab != null)
        {
            GameObject newInstnace = Instantiate(fadeOutPrefab);
            fadeOut = newInstnace.GetComponent<Image>();
        }

    }


    //===========================================
    // Methods
    //===========================================

    public void LoadScene(string nextSceneName, bool changeToTrigger)
    {
        if (changing)
            return;
        SceneChangeTrigger = !changeToTrigger;
        StartCoroutine(LoadSceneRoutine(nextSceneName));
    }

    public void ChangeScene(string nextSceneName)
    {
        if (changing)
            return;
        progress = 1.0f;
        SceneManager.LoadScene(nextSceneName);
    }


    private IEnumerator LoadSceneAsync(string nextSceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        //float progress = 0.0f;
        while (!operation.isDone)
        {
            //progress = Mathf.Clamp01(operation.progress / 0.9f);
            //Debug.Log($"·Îµù Áß... {progress * 100}%");

            yield return null;
        }
    }

    private IEnumerator LoadSceneRoutine(string nextSceneName)
    {
        progress = 0.0f;
        changing = true;
        preDone = false;
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;
        yield return null;
        while (!operation.isDone)
        {
            progress = operation.progress;
            if (operation.progress >= 0.9f)
            {
                preDone = true;
                if (SceneChangeTrigger)
                    operation.allowSceneActivation = true;
            }
            yield return null;
        }

        progress = 1.0f;
        SceneChangeTrigger = false;
        changing = false;
    }


    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void ActiveTrigger() { SceneChangeTrigger = true; }
    public bool LoadingDone() { return preDone; }
    public bool Loading() { return changing; }
    public float LoadingProgress() { return progress; }

    private float progress = 0.0f;
    private bool SceneChangeTrigger = false;
    private bool changing = false;
    private bool preDone = false;

    private Image fadeOut;
    [SerializeField] private GameObject fadeOutPrefab;
}
