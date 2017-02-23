Shader "Unlit/ArcanaVolumeRenderer" {
	Properties {
    _ColorVolume("Color Volume", 3D) = "black" {}
    _NormalVolume("Normal Volume", 3D) = "black" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
    Blend SrcAlpha OneMinusSrcAlpha
    //Blend One One

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

      sampler3D _ColorVolume;
      sampler3D _NormalVolume;

      inline fixed4 raymarch(sampler3D colorVolume, sampler3D normalVolume, float3 pos, float3 dir);

      fixed4 frag(v2f i) : SV_Target{
        float4 objSpacePos = i.objSpaceVertex;
        float3 objSpaceViewDir = -ObjSpaceViewDir(objSpacePos);

        fixed4 color = raymarch(_ColorVolume, _NormalVolume,
          objSpacePos.xyz + float4(0.5, 0.5, 0.5, 0),
          normalize(objSpaceViewDir));

        return color;
      }

      #define NUM_ITERATIONS 64
      #define STEP_SIZE 0.02

      #define USE_LIGHTING
      
      // Raymarching Blend Mode
      //#define TRADITIONAL_TRANSPARENCY
      //#define PREMULTIPLIED_TRANSPARENCY
      #define ADDITIVE_BLEND

      // Additive blend option
      #define COLOR_PRESERVING_ADDITIVE // divides color components by largest color component

      inline fixed4 raymarch(sampler3D colorVolume, sampler3D normalVolume, float3 pos, float3 dir) {
        fixed4 accumedColor = 0;
        fixed4 colorSample;
        float4 normalSample;
        float3 lightSource = normalize(float3(1, 1, 1));
        float3 step = dir * STEP_SIZE;
        float3 p = pos;
        for (int i = 0; i < NUM_ITERATIONS && accumedColor.a < 1; i++) {
          /*for (int j = 0; j < 8; j++) {

          }*/
          colorSample = tex3D(colorVolume, p);
          normalSample = tex3D(normalVolume, p);
          float sampleBrightness = lerp(1, dot(normalSample.xyz, lightSource), normalSample.a);
          float alpha = accumedColor.a;
          float alphaRemaining = (1 - alpha);
          float3 litColorSample = colorSample.xyz;
          #ifdef USE_LIGHTING
            litColorSample = colorSample.xyz * sampleBrightness;
          #endif
          float3 color;
          #ifdef TRADITIONAL_TRANSPARENCY
            color = (accumedColor.xyz * (1 - colorSample.a))
                  + (litColorSample * colorSample.a);
          #endif
          #ifdef PREMULTIPLIED_TRANSPARENCY
            color = (accumedColor.xyz)
                  + (litColorSample * (1 - (accumedColor.a)));
          #endif
          #ifdef ADDITIVE_BLEND
            color = (litColorSample)
                  + (accumedColor.xyz);
          #endif
          accumedColor = fixed4(color.x, color.y, color.z, alpha + colorSample.a + (lerp(0, (1 - sampleBrightness)*(1 - sampleBrightness), normalSample.a)));
          p += step;
        }
        float divide = 1;
        #ifdef COLOR_PRESERVING_ADDITIVE
          divide = accumedColor.x;
          if (accumedColor.y > accumedColor.x) divide = accumedColor.y;
          if (accumedColor.z > accumedColor.y && accumedColor.z > accumedColor.x) divide = accumedColor.z;
          if (divide < 1) divide = 1;
        #endif
        return fixed4(accumedColor.x / divide, accumedColor.y / divide, accumedColor.z / divide, accumedColor.a);
      }
			ENDCG
		}
	}
}
