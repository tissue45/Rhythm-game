Shader "Custom/SimpleToon"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _RampThreshold ("Ramp Threshold", Range(0,1)) = 0.5
        _RampSmooth ("Ramp Smoothing", Range(0.01,1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200
        
        // Removed Outline Pass requested by user
        
        // Pass 1: Toon Shading
        CGPROGRAM
        #pragma surface surf ToonRamp alphatest:_Cutoff

        sampler2D _MainTex;
        fixed4 _Color;
        float _RampThreshold;
        float _RampSmooth;

        // Custom lighting model
        fixed4 LightingToonRamp (SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            float NdotL = dot(s.Normal, lightDir);
            float diff = smoothstep(_RampThreshold - _RampSmooth, _RampThreshold + _RampSmooth, NdotL);
            
            fixed4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * diff * atten;
            c.a = s.Alpha;
            return c;
        }

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
