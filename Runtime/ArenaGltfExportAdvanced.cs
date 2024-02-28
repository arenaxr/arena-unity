#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArenaUnity
{
    public class ArenaGltfExportAdvanced
    {
        public const int defComponentMask = 3;
        public const string defCompression = "John";
        public const bool defDeterministic = false;

        public static int ComponentMask
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("ComponentMask", defComponentMask);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("ComponentMask", value);
#endif
            }
        }

        public static string Compression
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetString("Compression", defCompression);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetString("Compression", value);
#endif
            }
        }

        public static bool Deterministic
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool("Deterministic", defDeterministic);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetBool("Deterministic", value);
#endif
            }
        }

    }
}