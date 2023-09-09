// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FX/Water (Basic)" {
Properties {
    _horizonColor ("Horizon color", COLOR)  = ( .172 , .463 , .435 , 0)
    _WaveScale ("Wave scale", Range (0.02,0.15)) = .07
    [NoScaleOffset] _ColorControl ("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
    [NoScaleOffset] _BumpMap ("Waves Normalmap ", 2D) = "" { }
    WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
    _WaveSources ("Wave Sources", Vector) = (0, 0, 0, 0)
    _WaveParametersArray ("Wave Parameters Array (Distance, Amplitude, Speed, Decay)", Vector) = (1, 1, 1, 1)
    _NumWaveSources ("Number of Wave Sources", Range(0, 32)) = 1
    _WaveLifeTimes ("Wave Life Times", Vector) = (0, 0, 0, 0)
}

CGINCLUDE

#include "UnityCG.cginc"

uniform float4 _horizonColor;

uniform float4 WaveSpeed;
uniform float _WaveScale;
uniform float _NumWaveSources;
uniform float4 _WaveOffset;
//uniform float4 _WaveLifeTimes[32];

struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f {
    float4 pos : SV_POSITION;
    float2 bumpuv[2] : TEXCOORD0;
    float3 viewDir : TEXCOORD2;
    float3 worldPos : TEXCOORD3;
    UNITY_FOG_COORDS(4)
};

v2f vert(appdata v)
{
    v2f o;
    float4 s;

    o.pos = UnityObjectToClipPos (v.vertex);

    float4 wpos = mul (unity_ObjectToWorld, v.vertex);
    o.worldPos = wpos.xyz;

    float4 temp;
    temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
    o.bumpuv[0] = temp.xy * float2(.4, .45);
    o.bumpuv[1] = temp.wz;

    o.viewDir.xzy = normalize( WorldSpaceViewDir(v.vertex) );

    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}

ENDCG


Subshader {
    Tags { "RenderType"="Opaque" }
    Pass {

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

sampler2D _BumpMap;
sampler2D _ColorControl;

// uniform float4 _WaveSources[32];
// float4 _WaveParametersArray[32];

uniform float4 _WaveSources[32];
uniform float4 _WaveParametersArray[32];
uniform float4 _WaveLifeTimes[32];

half4 frag( v2f i ) : COLOR
{
    half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv[0] )).rgb;
    half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv[1] )).rgb;
    half3 bump = (bump1 + bump2) * 0.5;
    
for (int j = 0; j < _NumWaveSources; j++) {
    if (_Time.y < _WaveLifeTimes[j].x)
    {
        float3 toWaveSource = _WaveSources[j].xyz - i.worldPos.xyz;
        float distanceToWaveSource = length(toWaveSource);
        float attenuation = exp(-distanceToWaveSource * _WaveParametersArray[j].w);
        
        float wave = _WaveParametersArray[j].y * sin(distanceToWaveSource * _WaveParametersArray[j].x - _Time.y * _WaveParametersArray[j].z) * attenuation;
        
        bump.y += wave;
    }
}

    half fresnel = dot( i.viewDir, bump );
    half4 water = tex2D( _ColorControl, float2(fresnel,fresnel) );

    half4 col;
    col.rgb = lerp( water.rgb, _horizonColor.rgb, water.a );
    col.a = _horizonColor.a;

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}
ENDCG
    }
}
}
