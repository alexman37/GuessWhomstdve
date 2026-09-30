Shader "Unlit/TitleBground"
{
    Properties
    {
        [NoScaleOffset]T2DR_Bodies("T2DR_Bodies", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Heads("T2DR_Heads", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Faces("T2DR_Faces", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_S("T2DR_Hair_S", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_M("T2DR_Hair_M", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Hair_L("T2DR_Hair_L", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Jobs("T2DR_Jobs", 2DArray) = "" {}

        [NoScaleOffset]T2DR_Staches("T2DR_Staches", 2DArray) = "" {}
        [NoScaleOffset]T2DR_Beards("T2DR_Beards", 2DArray) = "" {}

        [NoScaleOffset]MainTexProp("MainTex", 2D) = "white" {}
        
        // Specifics
        _SkinColor("SkinColor", Color) = (1,1,1,1)
        _HairColor("HairColor", Color) = (1,1,1,1)
        _BodyColor("BodyColor", Color) = (1,1,1,1)
        _EyeColor("EyeColor", Color) = (1,1,1,1)

        _HairLength("HairLength", Float) = 0

        _HairIdx("HairIdx", Float) = 0
        _HeadIdx("HeadIdx", Float) = 0
        _FaceIdx("FaceIdx", Float) = 0
        _JobIdx("JobIdx", Float) = 0

        _Height("Height", Float) = 0
        _Weight("Weight", Float) = 0

        _CanvasDims("CanvasDims", Vector) = (0, 0, 0, 0)
        _CanvasMoveDir("CanvasMoveDir", Vector) = (0, 0, 0, 0)
        _CanvasMoveSpeed("CanvasMoveSpeed", Float) = 0

        _OPT_Stache("OPT_Stache", Vector) = (0, 0, 0, 0)
        _OPT_Beard("OPT_Beard", Vector) = (0, 0, 0, 0)

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
        int _HeadIdx;
        int _HairIdx;
        int _FaceIdx;
        int _JobIdx;

        int _Height;
        int _Weight;

        float2 _CanvasDims;
        float2 _CanvasMoveDir;
        float _CanvasMoveSpeed;

        float2 _OPT_Stache;
        float2 _OPT_Beard;

        static float4 ARR_SkinColorChoices[10] = {
            float4(0.95,0.8,0.66,1), 
            float4(0.98,0.79,0.61,1), 
            float4(0.91,0.72,0.49,1), 
            float4(0.92,0.7,0.52,1), 
            float4(0.87,0.65,0.44,1), 
            float4(0.88,0.67,0.4,1), 
            float4(0.88,0.64,0.43,1), 
            float4(0.77,0.55,0.36,1), 
            float4(0.73,0.52,0.3,1), 
            float4(0.68,0.45,0.27,1), 
        };

        static float4 ARR_HairColorChoices[11] = {
            float4(0.47,0.47,0.47,1), 
            float4(0.62,0.62,0.62,1), 
            float4(0.78,0.78,0.78,1), 
            float4(0.23,0.07,0.0,1), 
            float4(0.39,0.19,0.0,1), 
            float4(0.56,0.31,0.0,1), 
            float4(0.86,0.27,0.0,1), 
            float4(0.86,0.66,0.0,1), 
            float4(0.86,0.86,0.0,1), 
            float4(1.0,0.8,0.35,1), 
            float4(0.0,0.47,0.0,1),
        };

        static float4 ARR_EyeColorChoices[4] = {
            float4(0.58,0.23,0.07,1), 
            float4(0.06,0.52,0.54,1), 
            float4(0.19,0.74,0.23,1), 
            float4(0.27,0.27,0.27,1), 
        };

        static float4 ARR_FaveColorChoices[13] = {
            float4(0.7,0.02,0,1), 
            float4(0.98,0.52,0.06,1), 
            float4(0.6,0.6,0.0,1), 
            float4(1.0,1.0,0.2,1), 
            float4(0.2,0.4,0.0,1), 
            float4(0.6,1.0,0.2,1), 
            float4(0.0,0.0,0.58,1), 
            float4(0.0,0.4,0.4,1), 
            float4(0.49,0.0,1.0,1), 
            float4(0.8,0.0,0.8,1), 
            float4(0.4,0.2,0.0,1), 
            float4(1.0,1.0,1.0,1), 
            float4(0.2,0.2,0.2,1), 
        };

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
        TEXTURE2D_ARRAY(T2DR_Jobs);
        SAMPLER(samplerT2DR_Jobs);
        TEXTURE2D_ARRAY(T2DR_Staches);
        SAMPLER(samplerT2DR_Staches);
        TEXTURE2D_ARRAY(T2DR_Beards);
        SAMPLER(samplerT2DR_Beards);
        TEXTURE2D(MainTexProp);
        SAMPLER(samplerMainTexProp);
        SAMPLER(SamplerState_Linear_Repeat);

            // Graph Functions

        void Unity_Divide_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A / B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
            
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix * 2 - 1;
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
            Out = UV;
        }

        void Unity_Modulo_float2(float2 A, float2 B, out float2 Out)
        {
            Out = fmod(A, B);
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

            // Movement
            float2 TilingAmnts;
            Unity_Divide_float2(_CanvasDims, float2(64, 64), TilingAmnts);

            float MoveSpeed;
            float2 MoveDir;
            Unity_Multiply_float(_TimeParameters.x, _CanvasMoveSpeed, MoveSpeed);
            Unity_Multiply_float((MoveSpeed.xx), _CanvasMoveDir, MoveDir);
            
            float2 TilingResultUV;
            Unity_TilingAndOffset_float(IN.uv0.xy, TilingAmnts, MoveDir, TilingResultUV);
            float2 RotateResultUV;
            Unity_Rotate_Degrees_float(TilingResultUV, float2 (0.5, 0.5), 15, RotateResultUV);
            float2 ModMovement;
            Unity_Modulo_float2(RotateResultUV, float2(1, 1), ModMovement);

            // Get grid position and use to infer Properties
            float2 GridPosition = float2(RotateResultUV.x % 500, RotateResultUV.y % 500);
            int magicNumber = int(GridPosition.y) * 100 + int(GridPosition.x);
            magicNumber ^= magicNumber << 13;
            magicNumber ^= magicNumber >> 7;
            magicNumber ^= magicNumber << 17;

            // Prevent negatives and integer overflow
            if(magicNumber < 0) magicNumber = magicNumber * -1;
            //if(magicNumber > 999999) magicNumber -= 9999;

            _SkinColor = ARR_SkinColorChoices[magicNumber % 10];                      magicNumber++;
            _HairColor = ARR_HairColorChoices[magicNumber % 11];                      magicNumber++;
            _EyeColor = ARR_EyeColorChoices[magicNumber % 4];                         magicNumber++;
            _BodyColor = ARR_FaveColorChoices[magicNumber % 13];                      magicNumber++;
            _Weight = magicNumber % 3;                                                magicNumber++;
            _Height = magicNumber % 3;                                                magicNumber++;
            _FaceIdx = magicNumber % 14;                                              magicNumber++;
            _HeadIdx = magicNumber % 8;                                               magicNumber++;
            _HairLength = magicNumber % 3;                                            magicNumber++;
            _HairIdx = magicNumber % 21;                                              magicNumber++;
            _JobIdx  = magicNumber % 97 < 25 ? _Weight * 64 + (magicNumber % 64) : -1; magicNumber++;
            _OPT_Stache = float2(magicNumber % 7 == 0 ? 1 : 0, magicNumber % 20);     magicNumber++;
            _OPT_Beard = float2(magicNumber % 7 == 0 ? 1 : 0, magicNumber % 8);       magicNumber++;

            // Height of character
            float2 FullBodyOffset;
            float HeightOffset = 0.3 - (0.15 * _Height);
            float HorzOffset = 0;
            Unity_TilingAndOffset_float(ModMovement, float2 (1, 1), float2 (HorzOffset, HeightOffset), FullBodyOffset);

            // Hair-specific offset
            float2 HairOffset;
            Unity_TilingAndOffset_float(FullBodyOffset, float2 (1, 1), float2 (0, -0.2), HairOffset);

            // Offsets for tall items (may extend above given 64x64 range)
            float2 TallOffset;
            Unity_TilingAndOffset_float(ModMovement, float2 (0.6667, 0.6667), float2 (0.166667, 0.2 - (0.1 * _Height)), TallOffset);

            // Create main portrait
            UnityTexture2DArray T2DR_Bodies_Arr = UnityBuildTexture2DArrayStruct(T2DR_Bodies);
            float4 BodyChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Bodies_Arr.tex, T2DR_Bodies_Arr.samplerstate, FullBodyOffset, _Weight);

            UnityTexture2DArray T2DR_Heads_Arr = UnityBuildTexture2DArrayStruct(T2DR_Heads);
            float4 HeadChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Heads_Arr.tex, T2DR_Heads_Arr.samplerstate, FullBodyOffset, _HeadIdx);

            UnityTexture2DArray T2DR_Faces_Arr = UnityBuildTexture2DArrayStruct(T2DR_Faces);
            float4 FaceChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Faces_Arr.tex, T2DR_Faces_Arr.samplerstate, FullBodyOffset, _FaceIdx);

            float4 JobChoice = -1;
            if(_JobIdx > -1) {
                UnityTexture2DArray T2DR_Jobs_Arr = UnityBuildTexture2DArrayStruct(T2DR_Jobs);
                JobChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Jobs_Arr.tex, T2DR_Jobs_Arr.samplerstate, TallOffset, _JobIdx);
            }

            // We'll try to draw some sort of hair if either Hair Length or Hair Color (the two hair CPDs) are given
            bool HasAnyHair = _HairLength > -1 || _HairColor.a > 0.01;
            float4 HairChoice = -1;
            if(HasAnyHair) {
                // If hair length is defined (regardless of color) pick a style
                if(_HairLength > -1) {
                    UnityTexture2DArray T2DR_Hair_Arr;
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

                    // If Hair color not defined, go with a generic gray
                    if(_HairColor.a < 0.01) {
                        _HairColor = float4(0.6,0.6,0.6,1);
                    }
                }
                // If only color is defined, go with a generic default style
                else {
                    UnityTexture2DArray T2DR_Hair_Arr = UnityBuildTexture2DArrayStruct(T2DR_Hair_S);
                    HairChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Hair_Arr.tex, T2DR_Hair_Arr.samplerstate, HairOffset, 6);
                }
            }

            // Main components - head, body, hair etc.
            float4 OverlayStep1 = Overlay_float(BodyChoice, HeadChoice);
            float4 OverlayStep2 = Overlay_float(OverlayStep1, FaceChoice);
            float4 OverlayStep3 = HairChoice > -1 ? OverlayHair_float(OverlayStep2, HairChoice) : OverlayStep2;
            float4 MainBuild = JobChoice > -1 ? OverlayJob_float(OverlayStep3, JobChoice) : OverlayStep3;

            // COLOR SHIFT APPROACH
            /*float4 OverlayStep1 = OverlayCS_float(BodyChoice, HeadChoice, _SkinColor);
            float4 OverlayStep2 = OverlayCS_float(OverlayStep1, FaceChoice, _EyeColor);
            float4 OverlayStep3 = HairChoice > -1 ? OverlayHair_float(OverlayStep2, HairChoice) : OverlayStep2;*/

            // Optional components, like facial hair
            if(_OPT_Stache.x) {
                UnityTexture2DArray T2DR_Stache_Arr = UnityBuildTexture2DArrayStruct(T2DR_Staches);
                float4 StacheChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Stache_Arr.tex, T2DR_Stache_Arr.samplerstate, FullBodyOffset, _OPT_Stache.y);
                MainBuild = Overlay_float(MainBuild, StacheChoice);
            }
            if(_OPT_Beard.x) {
                UnityTexture2DArray T2DR_Beard_Arr = UnityBuildTexture2DArrayStruct(T2DR_Beards);
                float4 BeardChoice = SAMPLE_TEXTURE2D_ARRAY(T2DR_Beard_Arr.tex, T2DR_Beard_Arr.samplerstate, FullBodyOffset, _OPT_Beard.y);
                MainBuild = Overlay_float(MainBuild, BeardChoice);
            }
            
            float4 Colorized;
            Colorized = Colorize_float(MainBuild, _SkinColor, _HairColor, _BodyColor, _EyeColor);
            // COLOR SHIFT APPROACH
            /*float4 Colorized = MainBuild;*/
            float4 Finalized = Colorized;

            surface.BaseColor = (Finalized.xyz);
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
