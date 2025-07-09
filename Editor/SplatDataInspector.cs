// Original source: https://github.com/keijiro/SplatVFX/tree/main/jp.keijiro.splat-vfx

using UnityEngine;
using UnityEditor;

namespace ArenaUnity.Editor
{

    [CustomEditor(typeof(SplatData))]
    public sealed class SplatDataInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var count = ((SplatData)target).SplatCount;
            EditorGUILayout.LabelField("Splat Count", $"{count:N0}");
        }
    }

}
