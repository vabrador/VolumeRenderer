Shader "Unlit/ArcanaSimulation" {
  Properties {
    _Color0("Color 0", Color) = (0, 0, 0, 0)
    _Color1("Color 1", Color) = (1, 1, 1, 1)
    _Scale("Effect Scale", float) = 50
  }
  SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 100
    Blend One One

    Pass {
      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        #include "classicnoise4d.cginc"
        inline float scaledNoise(float4 i, float spatialFreq, float timeFreq) {
          return cnoise(float4(i.x * spatialFreq, i.y * spatialFreq, i.z * spatialFreq, i.w * timeFreq));
        }

        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };

        struct v2f
        {
          float4 vertex : SV_POSITION;
          float4 objPos : TEXCOORD0;
        };

        v2f vert(appdata v) {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.objPos = v.vertex;
          return o;
        }

        float4 _Color0;
        float4 _Color1;
        float _Scale;

        fixed4 frag(v2f i) : SV_Target {
          float x = i.objPos.x;
          float y = i.objPos.y;
          float z = i.objPos.z;
          float4 noiseIn = float4(x, y, z, _Time.x);
          float spatialFreq = 20.0 * _Scale;
          float timeFreq = 3.0;
          float noise = scaledNoise(noiseIn, spatialFreq, timeFreq);
          noise = 1 / sqrt(noise * noise) / 10 - 0.5; // neat

          return lerp(_Color0, _Color1, noise);
        }
      ENDCG
    }
  }
}
