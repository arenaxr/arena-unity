using UnityEditor;
using UnityEditor.SceneManagement;

// This class must live in an Editor folder
public static class ArenaHeadless
{
    public static bool isBatchMode;

    public static void Play()
    {
        isBatchMode = true;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        ArenaClientScene scene = ArenaClientScene.Instance;
        scene.hostAddress = "mqtt.arenaxr.org";
        scene.authType = ArenaMqttClient.Auth.Anonymous;
        scene.namespaceName = "public";
        scene.sceneName = "example";
        scene.ConnectArena();

        EditorApplication.EnterPlaymode();
    }
}
