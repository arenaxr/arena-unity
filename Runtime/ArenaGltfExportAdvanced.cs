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

        public static bool DisabledComponents
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool("DisabledComponents", defGOES.DisabledComponents);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetBool("DisabledComponents", value);
#endif
            }
        }


        public static bool OnlyActiveInHierarchy
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool("OnlyActiveInHierarchy", defGOES.OnlyActiveInHierarchy);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetBool("OnlyActiveInHierarchy", value);
#endif
            }
        }

        public static int LayerMask
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("LayerMask", (int)defGOES.LayerMask);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("LayerMask", value);
#endif
            }
        }

    }
}
