using FishNet.Managing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class SceneBootstrapper
{
    public static string StartingSceneName { get; private set; }
    public static bool OnBootstrapped { get; private set; } = true;
    public const string EditorPrefKey = "ProjectRAID.SceneBootstrapper.Enabled";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Bootstrap()
    {
        #if UNITY_EDITOR
        if (!UnityEditor.EditorPrefs.GetBool(EditorPrefKey, true))
        {
            Debug.Log("SceneBootstrapper disabled via Editor.");
            return;
        }
        #endif
        if(!OnBootstrapped) return;

        if(string.IsNullOrEmpty(StartingSceneName))
        {
            StartingSceneName = SceneManager.GetActiveScene().name;
        }
        else
        {
            Debug.Log($"Starting Scene Already Set: {StartingSceneName}");
        }

        if(StartingSceneName != "00_Bootstrap")
        {
            Debug.Log("Non-Title Scene Detected. Forcing Initialization...");
            
            CreateManager("NetworkManager");
            CreateManager("MasterAudio");
            var managers = CreateManager<Managers>("Managers");

            managers.DebugMode = true;
        }
        else
        {
            Debug.Log("Bootstrap Scene Detected. No Forced Initialization Required.");
        }
    }

    private static void CreateManager(string tag)
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(tag);
        prefab.WaitForCompletion();
        var instance = Object.Instantiate(prefab.Result);
        Object.DontDestroyOnLoad(instance);
        Debug.Log($"{instance.name} dynamically created.");
    }

    private static T CreateManager<T>(string tag)
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(tag);
        prefab.WaitForCompletion();
        var instance = Object.Instantiate(prefab.Result);
        Object.DontDestroyOnLoad(instance);
        Debug.Log($"{instance.name} dynamically created.");
        return instance.GetComponent<T>();
    }
}
