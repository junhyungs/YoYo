Shader "GPUMan/ConeOfSightURP" {
    Properties {
        _NonVisibleColor("Non Visible Color", Color) = (0, 0, 0, 1)
        _ViewAngle("Sight Angle", Range(0.01, 90)) = 45
        _Scale("Scale", Vector) = (1, 1, 1, 1)
    }

    Subshader {
        Tags {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        Pass {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata_base {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 ray : TEXCOORD1;
                float4 screenUV : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            TEXTURE2D_X_FLOAT(_ViewDepthTexture);
            SAMPLER(sampler_ViewDepthTexture);

            float4x4 _ViewSpaceMatrix;
            float3 _Test;
            half4 _NonVisibleColor;
            half _ViewAngle;
            float4 _Scale;

            v2f vert(appdata_base v) {
                v2f o;

                // Apply scaling
                float4 scaledVertex = v.vertex;
                scaledVertex.xyz *= _Scale.xyz;

                VertexPositionInputs vertInputs = GetVertexPositionInputs(scaledVertex.xyz);
                o.pos = vertInputs.positionCS;
                o.ray = vertInputs.positionVS * float3(1, 1, -1);
                o.screenUV = vertInputs.positionNDC;

                o.uv = v.uv;
                return o;
            }

            float getRadiusAlpha(float distFromCenter) {
                return max((0.5 - distFromCenter) * 10000, 0); // _CircleStrength 고정값 10000
            }

            float getAngleAlpha(float2 pos) {
                float sightAngleRadians = _ViewAngle / 2 * PI / 180;
                float2 npos = normalize(pos);
                float fwdDotPos = max(dot(float2(0, 1), npos), 0);
                float angle = acos(fwdDotPos);
                float angleF = angle / sightAngleRadians;
                return max(1.0 - pow(abs(angleF), 1.0), 0); // _AngleStrength 고정값 1
            }

            float getObstacleAlpha(float4 worldPos) {
                const float BIAS = 0.0001;
                float4 posViewSpace = mul(_ViewSpaceMatrix, worldPos);
                float3 projCoords = posViewSpace.xyz / posViewSpace.w; // ndc : -1 to 1
                projCoords = projCoords * 0.5 + 0.5; // 0 to 1
                float sampledDepth = (1.0 - SAMPLE_DEPTH_TEXTURE(_ViewDepthTexture, sampler_ViewDepthTexture, projCoords.xy));
                float depthDiff = (projCoords.z - BIAS) - sampledDepth;
                return saturate(depthDiff > 0 ? 0 : 1);
            }

            half4 frag(v2f i) : COLOR {
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z); // farPlane

                // ray impact point reconstruction from depth texture
                float depth = Linear01Depth(SampleSceneDepth(i.screenUV.xy / i.screenUV.w), _ZBufferParams); // depth
                float4 vpos = float4(i.ray * depth, 1);
                float4 wpos = mul(unity_CameraToWorld, vpos);

                // Discard point if is a vertical surface
                clip((dot(normalize(ddy(wpos.xyz)), float3(0, 1, 0)) > 0.45) ? -1 : 1);

                float3 opos = mul(unity_WorldToObject, wpos);
                opos.y = 0;

                // Discard hit point if it is outside the box
                clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));

                // Alpha calculation
                float2 pos2D = opos.xz;
                float distFromCenter = length(pos2D);
                float obstacleAlpha = getObstacleAlpha(wpos); // 0 if occluded, 1 if not
                float alpha = getRadiusAlpha(distFromCenter) * getAngleAlpha(pos2D);

                // Cone stripes
                float intervals = 0; // _ViewIntervals 고정값 0
                alpha *= step(0, intervals);

                half4 col = obstacleAlpha > 0 ? half4(1, 0, 0, 1) : _NonVisibleColor; // Set color to red
                return saturate(float4(col.rgb, alpha * col.a));
            }
            ENDHLSL
        }
    }
}
