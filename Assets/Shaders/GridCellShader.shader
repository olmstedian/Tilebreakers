Shader "Custom/GridCell"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0.5,0.5,0.5,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.0
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
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
            float4 _OutlineColor;
            float _OutlineWidth;
            float _CornerRadius;
            float _GlowIntensity;
            float4 _GlowColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float roundedRectangle(float2 position, float2 size, float radius)
            {
                // Adjust UV to be centered (from 0-1 to -0.5 to 0.5)
                float2 pos = position - 0.5;
                
                // Get distance from edges
                float2 distFromEdge = abs(pos) - size*0.5 + radius;
                
                // Calculate smoothed rectangle
                float outsideDistance = length(max(distFromEdge, 0.0));
                float insideDistance = min(max(distFromEdge.x, distFromEdge.y), 0.0);
                
                return outsideDistance + insideDistance - radius;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate base shape
                float2 size = float2(1.0 - _OutlineWidth*2, 1.0 - _OutlineWidth*2);
                float shape = roundedRectangle(i.uv, size, _CornerRadius);
                float outline = roundedRectangle(i.uv, float2(1.0, 1.0), _CornerRadius);
                
                // Create the cell with outline
                float alpha = 1.0 - smoothstep(0.0, 0.01, shape);
                float4 color = lerp(_OutlineColor, _Color, alpha);
                
                // Add glow effect when enabled
                if (_GlowIntensity > 0.0) {
                    float glow = 1.0 - smoothstep(0.0, 0.1, outline);
                    glow *= _GlowIntensity;
                    color = lerp(color, _GlowColor, glow * 0.7);
                }
                
                // Apply shape cutout
                float finalAlpha = 1.0 - smoothstep(0.0, 0.01, outline);
                color.a *= finalAlpha;
                
                return color;
            }
            ENDCG
        }
    }
}
