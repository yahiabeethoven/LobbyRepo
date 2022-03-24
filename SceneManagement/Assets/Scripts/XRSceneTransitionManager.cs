using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

[DisallowMultipleComponent]

public class XRSceneTransitionManager : MonoBehaviour
{
    public static XRSceneTransitionManager Instance;

    public Material transitionMaterial;
    public float transitionSpeed = 1.0f;

    public string initialScene;

    public bool isLoading { get; private set; } = false;

    Scene xrScene;
    Scene currentScene;
    float currentTransitionAmount = 0.0f;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Detected rouge XRSceneTransitionManager. Deleting it.");
            Destroy(this.gameObject);
            return;
        }

        xrScene = SceneManager.GetActiveScene();
        SceneManager.sceneLoaded += OnNewSceneAdded;

        if (!Application.isEditor)
        {
            TransitionTo(initialScene);
        }
    }

    public void TransitionTo(string scene)
    {
        if (!isLoading)
        {
            StartCoroutine(Load(scene));
        }
    }

    void OnNewSceneAdded(Scene newScene, LoadSceneMode mode)
    {
        if (newScene != xrScene)
        {
            currentScene = newScene;
            SceneManager.SetActiveScene(currentScene);
            PlaceXRRig(xrScene, currentScene);
        }
        
    }

    IEnumerator Load(string scene)
    {
        isLoading = true;
        yield return StartCoroutine(UnloadCurrentScene());

        yield return StartCoroutine(LoadNewScene(scene));
        isLoading = false;
    }
    IEnumerator UnloadCurrentScene()
    {
        AsyncOperation unload = SceneManager.UnloadSceneAsync(currentScene);
        while(!unload.isDone)
        {
            yield return null;
        }
    }
    IEnumerator LoadNewScene(string scene)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (!load.isDone)
        {
            yield return null;
        }        
    }

    static public void PlaceXRRig(Scene xrScene, Scene newScene)
    {
        GameObject[] xrObjects = xrScene.GetRootGameObjects();
        GameObject[] newSceneObjects = newScene.GetRootGameObjects();

        GameObject xrRig = xrObjects.First((obj) => { return obj.CompareTag("XRRig"); });
        GameObject xrRigOrigin = newSceneObjects.First((obj) => { return obj.CompareTag("XRRigOrigin"); });

        if (xrRig && xrRigOrigin)
        {
            xrRig.transform.position = xrRigOrigin.transform.position;
            xrRig.transform.rotation = xrRigOrigin.transform.rotation;
        }
    }
}
