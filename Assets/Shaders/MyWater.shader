Shader "Custom/MyWater"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1

        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)

        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
        _FoamMinDistance("Foam Minimum Distance", Float) = 0.04

        _WaveA("Wave A (dir, steepness, wavelength)", Vector) = (1,1,0.5,50)
        _WaveB("Wave B", Vector) = (0,1,0.25,20)
        _WaveAttenuationStrength("Wave Attenuation Strength", Range(0, 5)) = 2

        _ReflectionDistortion("Reflection Distortion", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 300

        GrabPass { "_GrabTexture" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define SMOOTHSTEP_AA 0.01

            sampler2D _CameraDepthTexture;
            sampler2D _CameraNormalsTexture;
            sampler2D _SurfaceNoise;
            sampler2D _SurfaceDistortion;
            sampler2D _GrabTexture;

            float4 _SurfaceNoise_ST;
            float4 _SurfaceDistortion_ST;

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _FoamColor;

            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _FoamMinDistance;
            float _SurfaceNoiseCutoff;
            float2 _SurfaceNoiseScroll;
            float _SurfaceDistortionAmount;

            float4 _WaveA, _WaveB;
            float _WaveAttenuationStrength;
            float _ReflectionDistortion;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPosition : TEXCOORD2;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float3 viewNormal : NORMAL;
                float2 grabUV : TEXCOORD3;
                float waveHeight : TEXCOORD4;
            };

            float3 GerstnerWave(float4 wave, float3 p, float attenuation, inout float3 tangent, inout float3 binormal,out float height )
            {
                float steepness = wave.z * attenuation;
                float wavelength = wave.w;
                float k = 2 * UNITY_PI / wavelength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = steepness / k;

                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                binormal += float3(
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.y * d.y * (steepness * sin(f))
                );
                height = a * sin(f);
                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                );
            }
            

            v2f vert(appdata v)
            {
                v2f o;
                float3 p = v.vertex.xyz;
                float waveHeightA, waveHeightB;

                float centerX = 0.5;
                float distFromSide = abs(v.uv.y - centerX) * 2.0;
                float waveAttenuation = pow(saturate(1.0 - distFromSide), _WaveAttenuationStrength);

                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);

                p += GerstnerWave(_WaveA, p, waveAttenuation, tangent, binormal,waveHeightA);
                p += GerstnerWave(_WaveB, p, waveAttenuation, tangent, binormal,waveHeightB);

                float3 normal = normalize(cross(binormal, tangent));
                float4 worldPos = float4(p, 1.0);

                o.waveHeight = waveHeightA + waveHeightB;
                o.vertex = UnityObjectToClipPos(worldPos);
                o.screenPosition = ComputeScreenPos(o.vertex);

                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, normal);

                float2 grabUV = o.screenPosition.xy / o.screenPosition.w;
                o.grabUV = grabUV;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);
                float depthDifference = existingDepthLinear - i.screenPosition.w;

                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV = float2(
                    i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x + distortSample.x,
                    i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y + distortSample.y
                );

                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

                float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));
                float3 normalDot = saturate(dot(existingNormal, i.viewNormal));

                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepthDifference01 = saturate(depthDifference / foamDistance);
                float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

                float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);
                float4 surfaceNoiseColor = _FoamColor * surfaceNoise;

                float2 reflectionUV = i.grabUV + distortSample * _ReflectionDistortion;
                float4 reflection = tex2D(_GrabTexture, reflectionUV);

                float4 finalColor = waterColor + surfaceNoiseColor + reflection * 0.3;
                finalColor.a = saturate(waterColor.a + surfaceNoise);

                return finalColor;
            }

            ENDCG
        }
    }
}
