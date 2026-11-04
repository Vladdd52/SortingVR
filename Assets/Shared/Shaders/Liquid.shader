// Unlit liquid with a world-space surface plane (fake fluid for VR).
// Everything above the plane is clipped. Front faces show the liquid color, with a thin lighter band
// just under the surface (meniscus). Back faces, seen through the cut, show the surface color, so the
// cut reads as the liquid's top. Plane point/normal are set per renderer by SortingVR.Core.LiquidWobble.
// With a zero normal nothing is clipped, so the material is also safe on plain closed meshes.
Shader "SortingVR/Liquid"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _SurfaceColor("Surface Color", Color) = (1, 1, 1, 1)
        _RimWidth("Surface Band Width (m)", Float) = 0.003
        [HideInInspector] _PlanePoint("Plane Point (world)", Vector) = (0, 0, 0, 0)
        [HideInInspector] _PlaneNormal("Plane Normal (world)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _SurfaceColor;
                float _RimWidth;
                float4 _PlanePoint;
                float4 _PlaneNormal;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Signed distance above the surface plane (0 everywhere when the normal is zero).
                float d = dot(input.positionWS - _PlanePoint.xyz, _PlaneNormal.xyz);
                clip(-d);

                if (!IS_FRONT_VFACE(face, true, false))
                {
                    return _SurfaceColor;
                }

                half hasPlane = step(1e-6, dot(_PlaneNormal.xyz, _PlaneNormal.xyz));
                half band = step(-_RimWidth, d) * hasPlane;
                return lerp(_BaseColor, _SurfaceColor, band);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
