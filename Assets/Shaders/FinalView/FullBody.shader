Shader "Unlit/FullBody"
{
    Properties
    {
        [NoScaleOffset]T2DR_Bodies("T2DR_Bodies", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Heads("T2DR_Heads", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Faces("T2DR_Faces", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_S("T2DR_Hair_S", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_M("T2DR_Hair_M", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_L("T2DR_Hair_L", 2DArray) = "" {}
        [NoScaleOffset]MainTexProp("MainTex", 2D) = "white" {}
        
        // Specifics
        _SkinColor("SkinColor", Color) = (1,1,1,1)
        _HairColor("HairColor", Color) = (1,1,1,1)
        _BodyColor("BodyColor", Color) = (1,1,1,1)
        _EyeColor("EyeColor", Color) = (1,1,1,1)

        _HairLength("HairLength", Float) = 0

        _BodyIdx("BodyIdx", Float) = 0
        _HairIdx("HairIdx", Float) = 0
        _HeadIdx("HeadIdx", Float) = 0
        _FaceIdx("FaceIdx", Float) = 0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEUNLIT
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 MainTexProp_TexelSize;

        float4 _SkinColor;
        float4 _HairColor;
        float4 _EyeColor;
        float4 _BodyColor;

        int _HairLength;
        int _BodyIdx;
        int _HeadIdx;
        int _HairIdx;
        int _FaceIdx;

        CBUFFER_END

        // Object and Global properties
        TEXTURE2D_ARRAY(T2DR_Bodies);
        SAMPLER(samplerT2DR_Bodies);
        TEXTURE2D_ARRAY(T2DR_Heads);
        SAMPLER(samplerT2DR_Heads);
        TEXTURE2D_ARRAY(T2DR_Faces);
        SAMPLER(samplerT2DR_Faces);
        TEXTURE2D_ARRAY(T2DR_Hair_S);
        SAMPLER(samplerT2DR_Hair_S);
        TEXTURE2D_ARRAY(T2DR_Hair_M);
        SAMPLER(samplerT2DR_Hair_M);
        TEXTURE2D_ARRAY(T2DR_Hair_L);
        SAMPLER(samplerT2DR_Hair_L);
        TEXTURE2D(MainTexProp);
        SAMPLER(samplerMainTexProp);
        SAMPLER(SamplerState_Linear_Repeat);

            // Graph Functions
            
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        // 3114e4fb6f435b044aee665411644ffd
        #include "Assets/Shaders/FinalView/ShaderLib.hlsl"

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;

            // Height of character
            float2 FullBodyOffset;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), float2 (0, 0.2), FullBodyOffset);

            // Hair-specific offset
            float2 HairOffset;
            Unity_TilingAndOffset_float(FullBodyOffset, float2 (1, 1), float2 (0, -0.2), HairOffset);

            UnityTexture2DArray T2DR_Bodies_Arr = UnityBuildTexture2DArrayStruct(T2DR_Bodies);
            float4 BodyChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Bodies_Arr.tex, T2DR_Bodies_Arr.samplerstate, FullBodyOffset, _BodyIdx);

            UnityTexture2DArray T2DR_Heads_Arr = UnityBuildTexture2DArrayStruct(T2DR_Heads);
            float4 HeadChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Heads_Arr.tex, T2DR_Heads_Arr.samplerstate, FullBodyOffset, _HeadIdx);

            UnityTexture2DArray T2DR_Faces_Arr = UnityBuildTexture2DArrayStruct(T2DR_Faces);
            float4 FaceChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Faces_Arr.tex, T2DR_Faces_Arr.samplerstate, FullBodyOffset, _FaceIdx);

            UnityTexture2DArray T2DR_Hair_Arr;
            float4 HairChoice;

            if(_HairLength == 0) {
                T2DR_Hair_Arr = UnityBuildTexture2DArrayStruct(T2DR_Hair_S);
                HairChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Hair_Arr.tex, T2DR_Hair_Arr.samplerstate, HairOffset, _HairIdx);
            } else if(_HairLength == 1) {
                T2DR_Hair_Arr = UnityBuildTexture2DArrayStruct(T2DR_Hair_M);
                HairChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Hair_Arr.tex, T2DR_Hair_Arr.samplerstate, HairOffset, _HairIdx);
            } else {
                T2DR_Hair_Arr = UnityBuildTexture2DArrayStruct(T2DR_Hair_L);
                HairChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Hair_Arr.tex, T2DR_Hair_Arr.samplerstate, HairOffset, _HairIdx);
            }

            float4 OverlayStep1;
            Overlay_float(BodyChoice, HeadChoice, OverlayStep1);
            float4 OverlayStep2;
            Overlay_float(OverlayStep1, FaceChoice, OverlayStep2);
            float4 OverlayStep3;
            OverlayHair_float(OverlayStep2, HairChoice, OverlayStep3);
            
            float4 Colorized;
            // TODO replace consts with actual variables
            Colorize_float(OverlayStep3, _SkinColor, _HairColor, _BodyColor, _EyeColor, Colorized);
            surface.BaseColor = (Colorized.xyz);
            surface.Alpha = 1;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}