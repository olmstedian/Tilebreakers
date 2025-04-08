Shader "Custom/TileShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        _OutlineColor ("Outline Color", Color) = (1,1,1,0.5)
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0
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
            float _CornerRadius;
            float _OutlineWidth;
            float4 _OutlineColor;
            float _GlowIntensity;
            float4 _GlowColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float roundedBoxSDF(float2 CenterPosition, float2 Size, float Radius)
            {
                return length(max(abs(CenterPosition) - Size + Radius, 0.0)) - Radius;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Remap UV from 0-1 to -0.5 to 0.5
                float2 uv = i.uv - 0.5;
                
                // Calculate distance to rounded rectangle edge
                float distance = roundedBoxSDF(uv, float2(0.5, 0.5), _CornerRadius);
                
                // Base color with rounded corners
                fixed4 col = _Color;
                
                // Core shape
                float mask = 1 - smoothstep(0, 0.003, distance);
                
                // Outline
                float outlineMask = 1 - smoothstep(-_OutlineWidth, -_OutlineWidth + 0.003, distance);
                outlineMask = outlineMask - mask; // Subtract interior to get just the outline
                
                // Glow effect
                float glowSize = 0.1;
                float glowMask = 1 - smoothstep(-glowSize, 0, distance);
                glowMask = glowMask - outlineMask - mask; // Only the area outside the shape
                float glow = glowMask * _GlowIntensity;
                
                // Combine layers
                fixed4 finalColor = col * mask; // Base color inside shape
                finalColor += _OutlineColor * outlineMask; // Add outline
                finalColor += _GlowColor * glow; // Add glow
                
                return finalColor;
            }
            ENDCG
        }
    }
}
