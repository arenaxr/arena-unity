using UnityEditor;
using UnityEditor.SceneManagement;

// This class must live in an Editor folder
public static class ArenaCLITest
{
    public static bool isBatchMode;

    public static void Play()
    {
        isBatchMode = true;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.EnterPlaymode();
    }
}
