Shader "Unlit/ArcanaVolumeRenderer" {
	Properties {
    _ColorVolume("Color Volume", 3D) = "black" {}
    _NormalVolume("Normal Volume", 3D) = "black" {}
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

      #define LIGHT_SOURCE = ;

      #define ITERATIONS 32
      #define STEP_SIZE 0.03
      inline fixed4 raymarch(sampler3D colorVolume, sampler3D normalVolume, float3 pos, float3 dir) {
        fixed4 accumedColor = 0;
        fixed4 colorSample;
        float4 normalSample;
        float3 lightSource = float3(1, 1, 1);
        float3 step = dir * STEP_SIZE;
        float3 p = pos;
        for (int i = 0; i < ITERATIONS && accumedColor.a < 1; i++) {
          colorSample = tex3D(colorVolume, p);
          normalSample = tex3D(normalVolume, p);
          float sampleBrightness = dot(normalSample.xyz, lightSource);
          float3 color = (accumedColor.xyz * (1 - colorSample.a))
            + ((colorSample.xyz * sampleBrightness) * colorSample.a);
          accumedColor = fixed4(color.x, color.y, color.z, (accumedColor.a + colorSample.a / ITERATIONS * 20));
          p += step;
        }
        return accumedColor;
      }

      sampler3D _ColorVolume;
      sampler3D _NormalVolume;
			
			fixed4 frag (v2f i) : SV_Target {
        float4 objSpacePos = i.objSpaceVertex;
        float3 objSpaceViewDir = -ObjSpaceViewDir(objSpacePos);

        fixed4 color = raymarch(_ColorVolume, _NormalVolume,
          objSpacePos.xyz + float4(0.5, 0.5, 0.5, 0),
          normalize(objSpaceViewDir));

        return color;
			}
			ENDCG
		}
	}
}
