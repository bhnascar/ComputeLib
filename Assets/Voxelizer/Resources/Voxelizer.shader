Shader "Unlit/Voxelizer"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // Interior voxelization pass
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            BlendOp LogicalXor

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 viewPos : TEXCOORD;
            };

            int grid_dimension;

            // Treats color values as a 128-bit bit array. Z is some value from 0-127.
            // This function sets every bit up to the z_value.
            int4 to_rgba(int z_value) {
                int r = (z_value >= 31) ? 0xFFFFFFFF : (1 << (z_value + 1)) - 1;
                int g = (z_value >= 63) ? 0xFFFFFFFF : (1 << max(z_value - 31, 0)) - 1;
                int b = (z_value >= 95) ? 0xFFFFFFFF : (1 << max(z_value - 63, 0)) - 1;
                int a = (1 << max(z_value - 95, 0)) - 1;
                return int4(r, g, b, a);
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex);
                return o;
            }

            // Note on grid_coords calculation: Clip space is 
            // (x:[-1,1], y:[-1,1], z:[1,0]).
            int4 frag(v2f i): SV_Target {
                float3 clip_coords = mul(UNITY_MATRIX_P, float4(i.viewPos, 1.0)).xyz;
                float3 voxel_size = 2.0 / grid_dimension;
                clip_coords.z = -2.0f * clip_coords.z + 1.0f; // Map z to [-1,1] range.
                clip_coords.z += 0.5f * voxel_size; // Add half voxel.
                int3 grid_coords = (clip_coords + 1.0f) / voxel_size;
                int4 encoded = to_rgba(grid_coords.z - 1);
                return encoded;
                // return (clip_coords.z < 0) ? asint(float4(1, 0, 0, 1)) : asint(float4(0, 1, 0, 1));
            }

            ENDCG
        }

        // Surface voxelization pass
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            BlendOp LogicalOr

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 viewPos : TEXCOORD;
            };

            int grid_dimension;

            int4 to_rgba(int z_value) {
                int r = 1 << z_value;
                int g = (z_value < 32) ? 0 : 1 << max(z_value - 32, 0);
                int b = (z_value < 64) ? 0 : 1 << max(z_value - 64, 0);
                int a = (z_value < 96) ? 0 : 1 << max(z_value - 96, 0);
                return int4(r, g, b, a);
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex);
                return o;
            }

            int4 frag(v2f i): SV_Target {
                float3 clip_coords = mul(UNITY_MATRIX_P, float4(i.viewPos, 1.0)).xyz;
                float3 voxel_size = 2.0 / grid_dimension;
                clip_coords.z = -2.0f * clip_coords.z + 1.0f;
                int3 grid_coords = (clip_coords + 1.0f) / voxel_size;
                int4 encoded = to_rgba(grid_coords.z);
                return encoded;
            }

            ENDCG
        }
    }
}
