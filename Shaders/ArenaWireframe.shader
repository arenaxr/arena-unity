// Wireframe shader for ARENA material wireframe support
// Cross-platform approach: uses barycentric coords stored in UV2 (no geometry shader needed)
// Compatible with Metal, Vulkan, and all Unity render pipelines

Shader "Hidden/Arena/Wireframe"
{
    Properties
    {
        _Color ("Wire Color", Color) = (1, 1, 1, 1)
        _WireThickness ("Wire Thickness", Range(0, 10)) = 1.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off

        Pass
        {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

fixed4 _Color;
float _WireThickness;

struct appdata
{
    float4 vertex : POSITION;
    float3 uv2 : TEXCOORD1; // barycentric coordinates stored in UV2
};

struct v2f
{
    float4 pos : SV_POSITION;
    float3 bary : TEXCOORD0;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.bary = v.uv2;
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    float minBary = min(i.bary.x, min(i.bary.y, i.bary.z));

    // Scale threshold by screen-space derivative for consistent width
    float threshold = _WireThickness * fwidth(minBary);

    // Anti-aliased wire edge
    float wire = 1.0 - smoothstep(0, threshold, minBary);

    // Discard interior pixels (fully transparent inside)
    if (wire < 0.01)
        discard;

    return fixed4(_Color.rgb, _Color.a * wire);
}
ENDCG
        }
    }
    Fallback "Unlit/Color"
}
