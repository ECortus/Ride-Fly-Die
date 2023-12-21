Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0,1)) = 0.3
        _Strength ("Strength", Range(0,1)) = 0.5
        _Color ("Color", Color) = (1,1,1,1)
        _Detail ("Detail", Range(0,1)) = 0.3
        _Highlight ("Highlight", Color) = (0,0,0,0)
        
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.1
        
    }
    SubShader
    {
        LOD 100
        
        Pass
        {
            Tags 
            { 
                "Queue"="Transparent" 
            }
            
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineWidth;

            float4 outline(float4 vertexPos, float outlineWidth)
            {
                float4x4 scale = float4x4
                (
                    1 + outlineWidth, 0, 0, 0,
                    0, 1 + outlineWidth, 0, 0,
                    0, 0, 1 + outlineWidth, 0,
                    0, 0, 0, 1 + outlineWidth
                );
                return mul(scale, vertexPos);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 vertexPos = outline(v.vertex, _OutlineWidth);
                o.vertex = UnityObjectToClipPos(vertexPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return float4(_OutlineColor.r, _OutlineColor.g, _OutlineColor.b, col.a);
            }
            ENDCG
        }
        
        Pass
        {
            Tags 
            { 
                "Queue"="Transparent+1" 
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Brightness;
            float _Strength;
            float _Detail;
            float4 _Color;

            float4 _Highlight;

            float Toon(float3 normal, float3 lightDir)
            {
                float NdotL = max(0.0,dot(normalize(normal), normalize(lightDir)));
                return floor(NdotL / _Detail);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= (Toon(i.worldNormal, _WorldSpaceLightPos0.xyz) * _Strength * _Color + _Brightness) * lerp(_Color, _Highlight, _Highlight.a);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
