#if UNITY_EDITOR
using UnityEditor;
#endif
using GLTFast.Export;

namespace ArenaUnity
{
    public class ArenaGltfExportAdvanced
    {
        // defaults
        public static ExportSettings defES = new() { };
        public static GameObjectExportSettings defGOES = new() { };
        public static DracoExportSettings defDES = new() { };

        public static int ComponentMask
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("ComponentMask", (int)defES.ComponentMask);
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

        public static int Compression
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("Compression", (int)defES.Compression);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("Compression", value);
#endif
            }
        }

        public static bool Deterministic
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool("Deterministic", defES.Deterministic);
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

        public static float LightIntensityFactor
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetFloat("LightIntensityFactor", defES.LightIntensityFactor);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetFloat("LightIntensityFactor", value);
#endif
            }
        }

    }
}
