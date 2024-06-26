Shader "Custom/WaterScroll" {
    Properties {
        _MainTex ("Water Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _ScrollX ("Scroll X", Float) = 0.1
        _ScrollY ("Scroll Y", Float) = 0.1
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        sampler2D _OverlayTex;
        float _ScrollX;
        float _ScrollY;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed2 scrolledUV = IN.uv_MainTex;
            fixed scrollX = _ScrollX * _Time;
            fixed scrollY = _ScrollY * _Time;
            scrolledUV += fixed2(scrollX, scrollY);

            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            half4 overlay = tex2D (_OverlayTex, scrolledUV);
            
            o.Albedo = c.rgb * overlay.rgb;
            o.Alpha = c.a * overlay.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}