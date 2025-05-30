using GLTFast.Export;
using UnityEditor;

namespace ArenaUnity.Editor
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
                return EditorPrefs.GetInt("ComponentMask", (int)defES.ComponentMask);
            }

            set
            {
                EditorPrefs.SetInt("ComponentMask", value);
            }
        }

        public static int Compression
        {
            get
            {
                return EditorPrefs.GetInt("Compression", (int)defES.Compression);
            }

            set
            {
                EditorPrefs.SetInt("Compression", value);
            }
        }

        public static bool Deterministic
        {
            get
            {
                return EditorPrefs.GetBool("Deterministic", defES.Deterministic);
            }

            set
            {
                EditorPrefs.SetBool("Deterministic", value);
            }
        }

        public static float LightIntensityFactor
        {
            get
            {
                return EditorPrefs.GetFloat("LightIntensityFactor", defES.LightIntensityFactor);
            }

            set
            {
                EditorPrefs.SetFloat("LightIntensityFactor", value);
            }
        }

        public static bool DracoSettings
        {
            get
            {
                return EditorPrefs.GetBool("DracoSettings", defES.DracoSettings != null);
            }

            set
            {
                EditorPrefs.SetBool("DracoSettings", value);
            }
        }

        public static int DracoColorQuantization
        {
            get
            {
                return EditorPrefs.GetInt("DracoColorQuantization", defDES.colorQuantization);
            }

            set
            {
                EditorPrefs.SetInt("DracoColorQuantization", value);
            }
        }

        public static int DracoDecodingSpeed
        {
            get
            {
                return EditorPrefs.GetInt("DracoDecodingSpeed", defDES.decodingSpeed);
            }

            set
            {
                EditorPrefs.SetInt("DracoDecodingSpeed", value);
            }
        }

        public static int DracoEncodingSpeed
        {
            get
            {
                return EditorPrefs.GetInt("DracoEncodingSpeed", defDES.encodingSpeed);
            }

            set
            {
                EditorPrefs.SetInt("DracoEncodingSpeed", value);
            }
        }

        public static int DracoNormalQuantization
        {
            get
            {
                return EditorPrefs.GetInt("DracoNormalQuantization", defDES.normalQuantization);
            }

            set
            {
                EditorPrefs.SetInt("DracoNormalQuantization", value);
            }
        }

        public static int DracoPositionQuantization
        {
            get
            {
                return EditorPrefs.GetInt("DracoPositionQuantization", defDES.positionQuantization);
            }

            set
            {
                EditorPrefs.SetInt("DracoPositionQuantization", value);
            }
        }

        public static int DracoTexCoordQuantization
        {
            get
            {
                return EditorPrefs.GetInt("DracoTexCoordQuantization", defDES.texCoordQuantization);
            }

            set
            {
                EditorPrefs.SetInt("DracoTexCoordQuantization", value);
            }
        }

        public static bool DisabledComponents
        {
            get
            {
                return EditorPrefs.GetBool("DisabledComponents", defGOES.DisabledComponents);
            }

            set
            {
                EditorPrefs.SetBool("DisabledComponents", value);
            }
        }


        public static bool OnlyActiveInHierarchy
        {
            get
            {
                return EditorPrefs.GetBool("OnlyActiveInHierarchy", defGOES.OnlyActiveInHierarchy);
            }

            set
            {
                EditorPrefs.SetBool("OnlyActiveInHierarchy", value);
            }
        }

        public static int LayerMask
        {
            get
            {
                return EditorPrefs.GetInt("LayerMask", (int)defGOES.LayerMask);
            }

            set
            {
                EditorPrefs.SetInt("LayerMask", value);
            }
        }

    }
}
