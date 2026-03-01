// 创建新的Shader文件：RigidQuadShader.shader
Shader "Custom/RigidQuadShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EdgeSharpness ("Edge Sharpness", Range(0.1, 2)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        // 关键设置：关闭所有可能导致扭曲的渲染特性
        Cull Off          // 渲染双面
        ZWrite On         // 启用深度写入
        ZTest LEqual      // 标准深度测试
        Lighting Off      // 关闭光照（避免光照引起的视觉变化）
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _EdgeSharpness;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // 关键：不使用任何顶点动画或变形
                // 直接传递顶点位置，不做任何额外计算
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // 简单的UV变换
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 增强边缘锐度
                float edge = 1.0 - smoothstep(0.45, 0.55, length(i.uv - 0.5));
                col.rgb += edge * 0.05 * _EdgeSharpness;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Unlit/Texture"
}