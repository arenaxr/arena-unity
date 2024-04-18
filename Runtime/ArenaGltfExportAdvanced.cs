#if UNITY_EDITOR
using UnityEditor;
#endif
using GLTFast.Export;

namespace ArenaUnity
{
    public class ArenaGltfExportAdvanced
    {
        // defaults
        public static ExportSettings defES = new ExportSettings() { };
        public static GameObjectExportSettings defGOES = new GameObjectExportSettings() { };
        public static DracoExportSettings defDES = new DracoExportSettings() { };

        public static int ComponentMask
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("ComponentMask", (int)defES.ComponentMask);
#else
                return (int)defES.ComponentMask;
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
                return (int)defES.Compression;
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
                return defES.Deterministic;
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
                return defES.LightIntensityFactor;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetFloat("LightIntensityFactor", value);
#endif
            }
        }

        public static bool DracoSettings
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool("DracoSettings", defES.DracoSettings != null);
#else
                return defES.DracoSettings != null;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetBool("DracoSettings", value);
#endif
            }
        }

        public static int DracoColorQuantization
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoColorQuantization", defDES.colorQuantization);
#else
                return defDES.colorQuantization;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoColorQuantization", value);
#endif
            }
        }

        public static int DracoDecodingSpeed
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoDecodingSpeed", defDES.decodingSpeed);
#else
                return defDES.decodingSpeed;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoDecodingSpeed", value);
#endif
            }
        }

        public static int DracoEncodingSpeed
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoEncodingSpeed", defDES.encodingSpeed);
#else
                return defDES.encodingSpeed;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoEncodingSpeed", value);
#endif
            }
        }

        public static int DracoNormalQuantization
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoNormalQuantization", defDES.normalQuantization);
#else
                return defDES.normalQuantization;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoNormalQuantization", value);
#endif
            }
        }

        public static int DracoPositionQuantization
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoPositionQuantization", defDES.positionQuantization);
#else
                return defDES.positionQuantization;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoPositionQuantization", value);
#endif
            }
        }

        public static int DracoTexCoordQuantization
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetInt("DracoTexCoordQuantization", defDES.texCoordQuantization);
#else
                return defDES.texCoordQuantization;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetInt("DracoTexCoordQuantization", value);
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
                return defGOES.DisabledComponents;
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
                return defGOES.OnlyActiveInHierarchy;
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
                return (int)defGOES.LayerMask;
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
