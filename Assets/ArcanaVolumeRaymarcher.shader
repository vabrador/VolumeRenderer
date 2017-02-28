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
        float4 objSpaceVertex : TEXCOORD1;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.objSpaceVertex = v.vertex;
				return o;
			}

      #define LIGHT_DIRECTION normalize(float3(2, 3, 1))
      inline float4 light(float4 srcColor, float4 normal) {
        float3 srcColorXYZ = srcColor.xyz;
        float3 litColor = lerp(srcColorXYZ, srcColorXYZ * dot(LIGHT_DIRECTION, normal.xyz), normal.a);
        return fixed4(litColor.x, litColor.y, litColor.z, srcColor.a);
      }

      inline float4 blend(float4 srcColor, float4 dstColor) {
        float3 color = (srcColor.xyz * (1 - dstColor.a))
                     + dstColor.xyz * (dstColor.a);
        return fixed4(color.x, color.y, color.z, saturate(dstColor.a + srcColor.a));
      }

      sampler3D _ColorVolume;
      sampler3D _NormalVolume;
      float4 sampleAndBlend(float3 pos, float4 baseColor) {
        float4 sampleColor = tex3D(_ColorVolume, pos);
        float4 sampleNormal = tex3D(_NormalVolume, pos);
        sampleColor = light(sampleColor, sampleNormal);
        return blend(sampleColor, baseColor);
      }

      #define OBJECT_WIDTH 1.0
      #define VOXELS_PER_DIMENSION 64
      #define MAX_ITERATIONS 128
      float4 raymarch(float3 pos, float3 dir) {
        float voxelWidth = OBJECT_WIDTH / VOXELS_PER_DIMENSION;
        float3 voxelSpacePos = pos * VOXELS_PER_DIMENSION;
        float4 color = 0;
        float3 step;
        float3 halfStep;

        float invDirX = 1.0 / dir.x;
        //if (dir.x == 0) invDirX = 10000;
        float invDirY = 1.0 / dir.y;
        //if (dir.y == 0) invDirY = 10000;
        float invDirZ = 1.0 / dir.z;
        //if (dir.z == 0) invDirZ = 10000;

        for (int i = 0; i < MAX_ITERATIONS; i++) {

          // Calculate step

          float dX = 1 - frac(voxelSpacePos.x) - saturate(sign(-dir.x));
          if (abs(dX) < 0.00001) dX = 1;
          float stepX = abs(dX * invDirX);
          
          float dY = 1 - frac(voxelSpacePos.y) - saturate(sign(-dir.y));
          if (abs(dY) < 0.00001) dY = 1;
          float stepY = abs(dY * invDirY);
          
          float dZ = 1 - frac(voxelSpacePos.z) - saturate(sign(-dir.z));
          if (abs(dZ) < 0.00001) dZ = 1;
          float stepZ = abs(dZ * invDirZ);

          step = dir * min(stepX, min(stepY, stepZ));
          halfStep = step * 0.5;

          color = sampleAndBlend(voxelWidth * halfStep + pos, color);
          pos = voxelWidth * step + pos;
          voxelSpacePos = pos * VOXELS_PER_DIMENSION;
        }

        return color;
      }
			
			fixed4 frag (v2f i) : SV_Target {
        float4 objSpacePos = i.objSpaceVertex;
        float3 objSpaceViewDir = -ObjSpaceViewDir(objSpacePos);

        float voxelWidth = OBJECT_WIDTH / VOXELS_PER_DIMENSION;
        float3 pos = objSpacePos.xyz + float3(0.5, 0.5, 0.5);
        float3 dir = normalize(objSpaceViewDir);

        fixed4 color = raymarch(pos, dir);
        return color;
			}
			ENDCG
		}
	}
}

/*

Old raymarch code

      #define OBJ_WIDTH 1.0 // objects are 1 object unit wide
      #define NUM_VOXELS 64 // this needs to match the texture width
      #define ITERATIONS 64
      #define STEP_SIZE 0.03
      inline fixed4 raymarch(sampler3D colorVolume, sampler3D normalVolume, float3 origin, float3 dir) {
        fixed4 accumedColor = 0;
        fixed4 colorSample;
        float4 normalSample;
        float3 lightSource = float3(1, 1, 1);

        float voxelWidth = (OBJ_WIDTH / NUM_VOXELS);

        float3 step;
        // old step
        step = dir * STEP_SIZE;

        float3 pos = origin;
        for (int i = 0; i < ITERATIONS && accumedColor.a < 1; i++) {
          // Sample
          colorSample = tex3D(colorVolume, pos);
          normalSample = tex3D(normalVolume, pos);
          float sampleBrightness = dot(normalSample.xyz, lightSource);
          float3 color = (accumedColor.xyz * (1 - colorSample.a))
            + ((colorSample.xyz * sampleBrightness) * colorSample.a);
          accumedColor = fixed4(color.x, color.y, color.z, (accumedColor.a + colorSample.a / ITERATIONS * 20));

          pos += step;
        }
        return accumedColor;
      }

Old step code

          // Calculate step
          // float cosThetaX = dir.x;
          // float dX = pos.x % voxelWidth;
          // if (dir.x > 0)
          // dX = voxelWidth - dX;
          // float speedX = abs(cosThetaX);
          // if (dX == 0) dX = voxelWidth;
          // float stepX = (dX / speedX);
          // if (isnan(stepX)) stepX = 100000;
          
          // float cosThetaY = dir.y;
          // float dY = pos.y % voxelWidth;
          // if (dir.y > 0)
          // dY = voxelWidth - dY;
          // float speedY = abs(cosThetaY);
          // if (dY == 0) dY = voxelWidth;
          // float stepY = (dY / speedY);
          // if (isnan(stepY)) stepY = 100000;
          
          // float cosThetaZ = dir.z;
          // float dZ = pos.z % voxelWidth;
          // if (dir.z > 0)
          // dZ = voxelWidth - dZ;
          // float speedZ = abs(cosThetaZ);
          // if (dZ == 0) dZ = voxelWidth;
          // float stepZ = (dZ / speedZ);
          // if (isnan(stepZ)) stepZ = 100000;
          //step = dir * min(stepX, min(stepY, stepZ));
          ///// if ray leaves boundaries of voxels, break


  // old bresenham raymarch

      // #define MID_VOXEL_RESOLUTION 1024
      // #define MAX_ITERATIONS 16
      // fixed4 bresenhamRaymarch(float3 start, float3 end, float voxelWidth) {
      //   float x = start.x, y = start.y, z = start.z;
      //   float changeX = (end.x - start.x);
      //   float changeY = (end.y - start.y);
      //   float changeZ = (end.z - start.z);

      //   // component proportions (inflated integers)
      //   int dx = (int)(MID_VOXEL_RESOLUTION * changeX);
      //   int dy = (int)(MID_VOXEL_RESOLUTION * changeY);
      //   int dz = (int)(MID_VOXEL_RESOLUTION * changeZ);
      //   int absDx = abs(dx), absDy = abs(dy), absDz = abs(dz);
      //   int absDx2 = absDx * 2, absDy2 = absDy * 2, absDz2 = absDz * 2;

      //   // voxel iteration and incrementing
      //   int iDx = (int)abs(changeX * VOXELS_PER_DIMENSION);
      //   int iDy = (int)abs(changeY * VOXELS_PER_DIMENSION);
      //   int iDz = (int)abs(changeZ * VOXELS_PER_DIMENSION);

      //   float xInc = voxelWidth, yInc = voxelWidth, zInc = voxelWidth;
      //   if (dx < 0) xInc = -voxelWidth;
      //   if (dy < 0) yInc = -voxelWidth;
      //   if (dz < 0) zInc = -voxelWidth;

      //   fixed4 sampleColor;
      //   fixed4 sampleNormal;
      //   fixed4 color = 0;
      //   color = sampleAndBlend(fixed3(x, y, z), color);

      //   int err1, err2;
      //   if (absDx >= absDy && absDx >= absDz) {
      //     err1 = absDy2 - absDx;
      //     err2 = absDz2 - absDx;

      //     [unroll(MAX_ITERATIONS)]
      //     for (int i = 0; i < iDx; i++) {
      //       color = sampleAndBlend(fixed3(x, y, z), color);

      //       if (err1 > 0) {
      //         y += yInc;
      //         err1 -= absDx2;
      //       }

      //       if (err2 > 0) {
      //         z += zInc;
      //         err2 -= absDx2;
      //       }

      //       err1 += absDy2;
      //       err2 += absDz2;
      //       x += xInc;
      //     }
      //   }
      //   // else if (absDy > absDx && absDy >= absDz) {
      //   //   err1 = absDx2 - absDy;
      //   //   err2 = absDz2 - absDy;

      //   //   [unroll(MAX_ITERATIONS)]
      //   //   for (int i = 0; i < iDy; i++) {
      //   //     color = sampleAndBlend(fixed3(x, y, z), color);

      //   //     if (err1 > 0) {
      //   //       x += xInc;
      //   //       err1 -= absDy2;
      //   //     }

      //   //     if (err2 > 0) {
      //   //       z += zInc;
      //   //       err2 -= absDy2;
      //   //     }

      //   //     err1 += absDx2;
      //   //     err2 += absDz2;
      //   //     y += yInc;
      //   //   }
      //   // }
      //   // else { // AbsDz > AbsDx && AbsDz > AbsDy
      //   //   err1 = absDx2 - absDz;
      //   //   err2 = absDy2 - absDz;

      //   //   [unroll(MAX_ITERATIONS)]
      //   //   for (int i = 0; i < iDz; i++) {
      //   //     color = sampleAndBlend(fixed3(x, y, z), color);

      //   //     if (err1 > 0) {
      //   //       x += xInc;
      //   //       err1 -= absDz2;
      //   //     }

      //   //     if (err2 > 0) {
      //   //       y += yInc;
      //   //       err2 -= absDz2;
      //   //     }

      //   //     err1 += absDx2;
      //   //     err2 += absDy2;
      //   //     z += zInc;
      //   //   }
      //   // }

      //   return color;
      // }

*/