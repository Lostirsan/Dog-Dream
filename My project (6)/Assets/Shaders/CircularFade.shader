Shader "UI/CircularFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Radius ("Radius", Float) = 1.0
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Softness ("Softness", Float) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
            float4 _Color;
            float _Radius;
            float2 _Center;
            float _Softness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = _Center;
                
                // Adjust for aspect ratio
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 diff = uv - center;
                diff.x *= aspect;
                
                float dist = length(diff);
                
                // Create circular mask - inside radius is transparent, outside is black
                float alpha = smoothstep(_Radius - _Softness, _Radius + _Softness, dist);
                
                return float4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
