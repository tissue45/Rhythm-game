Shader "Custom/MMDWebGL"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.05
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.03)) = 0.005
        
        [Header(Lighting)]
        _ShadowProps ("Shadow (R=Threshold, G=Smooth, B=Brightness)", Vector) = (0.5, 0.1, 0.6, 0)
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200

        // Pas 1: Outline (Inverted Hull)
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="Always" }
            Cull Front
            ZWrite On
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v) {
                v2f o;
                // Simple expansion along normal
                float3 norm = normalize(v.normal);
                float4 pos = v.vertex;
                pos.xyz += norm * _OutlineWidth;
                
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }

        // Pass 2: Toon Render (Base)
        Pass
        {
            Name "ToonBase"
            Tags { "LightMode" = "ForwardBase" }
            Cull Off  // Changed from Back to Off (Double-sided rendering for Hair/Skirts)
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : NORMAL;
                LIGHTING_COORDS(1,2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            float4 _ShadowProps; // x=Threshold, y=Smooth, z=MinBrightness

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Alpha Cutoff (Lashes fix)
                clip(col.a - _Cutoff);

                // Simple Toon Lighting
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(normal, lightDir);
                
                // Ramp
                float shadow = smoothstep(_ShadowProps.x - _ShadowProps.y, _ShadowProps.x + _ShadowProps.y, NdotL);
                float lightIntensity = shadow * (1.0 - _ShadowProps.z) + _ShadowProps.z; // blending brightness

                // Apply lighting
                fixed3 lighting = lightIntensity * _LightColor0.rgb;
                
                // Add Ambient
                lighting += ShadeSH9(half4(normal,1));

                col.rgb *= lighting;
                
                return col;
            }
            ENDCG
        }
        
        // Pass 3: Cast Shadow Support
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    Fallback "Legacy Shaders/Transparent/Cutout/Diffuse"
}
