Shader "Custom/GridCellShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0.5,0.5,1,0.2)
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.2
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
        _BorderColor ("Border Color", Color) = (1,1,1,0.5)
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _CornerRadius;
            float _BorderWidth;
            float4 _BorderColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float roundedRectangle(float2 uv, float2 size, float radius)
            {
                // Convert UV from 0-1 to -0.5 to 0.5
                float2 pos = uv - 0.5;
                
                // Calculate distance from edge
                float2 edge = abs(pos) - size/2 + radius;
                float outsideDistance = length(max(edge, 0));
                float insideDistance = min(max(edge.x, edge.y), 0);
                
                return outsideDistance + insideDistance - radius;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance to rounded rectangle
                float distance = roundedRectangle(i.uv, float2(0.9, 0.9), _CornerRadius);
                
                // Add glow effect
                float glow = smoothstep(0.05, 0.0, distance) * _GlowIntensity;
                
                // Add border
                float border = smoothstep(_BorderWidth + 0.01, _BorderWidth, abs(distance));
                
                // Combine colors
                fixed4 col = _Color;
                col = lerp(col, _BorderColor, border);
                col = lerp(col, _GlowColor, glow);
                
                // Apply alpha based on distance
                col.a *= smoothstep(0.01, 0.0, distance);
                
                // Add subtle pattern
                float pattern = frac(sin(dot(i.uv * 20, float2(12.9898, 78.233))) * 43758.5453) * 0.05;
                col.rgb += pattern;
                
                return col;
            }
            ENDCG
        }
    }
}
