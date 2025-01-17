Shader "Custom/IridescentShader"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _IridescentColor("Iridescent Color", Color) = (0,1,1,1)
        _Mode("Iridescent Mode", Float) = 0 // 0 for default, 1 for iridescent
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float3 viewDir;
        };

        float _Mode;
        fixed4 _Color;
        fixed4 _IridescentColor;

        void surf(Input IN, inout SurfaceOutput o)
        {
            if (_Mode > 0.5)
            {
                o.Albedo = lerp(_Color.rgb, _IridescentColor.rgb, dot(normalize(IN.viewDir), float3(0, 1, 0)));
            }
            else
            {
                o.Albedo = _Color.rgb;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
