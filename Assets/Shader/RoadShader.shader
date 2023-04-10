Shader "Unlit/RoadShader" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Thickness ("Thickness", Range(0.01, 1)) = 1
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            float _Thickness;
            float4 _Color;

            v2f vert (appdata v) {
                v2f o;
                v.vertex.y *= _Thickness;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _Color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}