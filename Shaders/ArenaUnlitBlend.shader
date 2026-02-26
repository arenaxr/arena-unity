// Custom unlit shader for ARENA flat materials with blending support
// Unlike Unity's built-in Unlit/Color, this exposes _SrcBlend, _DstBlend, _ZWrite

Shader "Hidden/Arena/UnlitBlend"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _SrcBlend ("Source Blend", Float) = 1 // BlendMode.One
        _DstBlend ("Dest Blend", Float) = 0   // BlendMode.Zero
        _ZWrite ("ZWrite", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

fixed4 _Color;

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    return _Color;
}
ENDCG
        }
    }
    Fallback "Unlit/Color"
}
