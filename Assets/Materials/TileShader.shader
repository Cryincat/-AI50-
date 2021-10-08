Shader "Custom/TileShader"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float3 vertexColor;
        };

        struct v2f {
            float4 pos : SV_POSITION;
            fixed4 color : COLOR;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
        }

        float inverseLerp(float a,float b, float value){
            return saturate((value-a)/(b-a));
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            /*float heightPercent = inverseLerp(minHeight,maxHeight,IN.worldPos.y);
            o.Albedo = .2;//heightPercent;//IN.worldPos.y / maxHeight;
            o.Albedo *= 2*IN.worldNormal.y*IN.worldNormal.y;*/
            o.Albedo = (1, 1, 1, 1);
            o.Albedo *= IN.vertexColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
