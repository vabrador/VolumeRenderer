Shader "Unlit/ArcanaVolumeRenderer" {
	Properties {
		_Volume ("Volume", 3D) = "black" {}
    _Scale("Scale", Range(0, 2)) = 0.5
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
    Blend One One

		Pass {
			CGPROGRAM

			#include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

			struct v2f {
				float4 vertex : SV_POSITION;
        float4 objSpaceVertex : TEXCOORD0;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.objSpaceVertex = v.vertex;
				return o;
			}

#define ITERATIONS 64
#define STEP_SIZE 0.05
      float3 raymarch(sampler3D volume, float3 pos, float3 dir) {
        float density = 0;
        float4 samp;
        float3 step = dir * STEP_SIZE;
        float3 p = pos;
        float3 unit = float3(1, 1, 1);
        for (int i = 0; i < ITERATIONS; i++) {
          samp = tex3D(volume, p).x;
          density += samp / ITERATIONS * 20;
          p += step;
        }
        return density;
      }

      sampler3D _Volume;
      float _Scale;
			
			fixed4 frag (v2f i) : SV_Target {
        float3 objSpacePos     = i.objSpaceVertex;
        float3 objSpaceViewDir = -UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, float4(i.objSpaceVertex.x, i.objSpaceVertex.y, i.objSpaceVertex.z, 1)).xyz);

        float scale = _Scale;

        float density = raymarch(_Volume, objSpacePos, normalize(objSpaceViewDir));

        // float4 color = float4(1, 0, 0, 1);
        return density;
			}
			ENDCG
		}
	}
}
