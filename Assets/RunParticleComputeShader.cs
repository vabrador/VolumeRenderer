using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunParticleComputeShader : MonoBehaviour {

  public ComputeShader computeShader;
  public MeshRenderer particlePreviewRenderer;
  public MeshRenderer particleSourceRenderer;
  public MeshRenderer paintedParticleRenderer;

  private int _createParticleKernelIdx;
  private RenderTexture _particleTex;
  private Material _particleTexPreviewMaterialInstance;

  private int _paintParticlesKernelIdx;
  private int _clearParticlesKernelIdx;
  private Texture2D _particleSourceTex;
  private Material _particleSourcePreviewMaterialInstance;
  private RenderTexture _paintedParticleTex;
  private Material _paintedParticlePreviewMaterialInstance;

  private int _particleTexWidth = 1024;

  void Start() {
    // Create Particle
    _particleTex = new RenderTexture(_particleTexWidth, _particleTexWidth, 24);
    _particleTex.name = "Particle Render Texture";
    _particleTex.enableRandomWrite = true;
    _particleTex.Create();

    _createParticleKernelIdx = computeShader.FindKernel("CreateParticle");
    computeShader.SetInt(Shader.PropertyToID("_particleTexWidth"), _particleTexWidth);
    computeShader.SetTexture(_createParticleKernelIdx, "_createParticleTex", _particleTex);

    _particleTexPreviewMaterialInstance = particlePreviewRenderer.material;
    _particleTexPreviewMaterialInstance.mainTexture = _particleTex;

    // Dispatch creation of particle
    uint xGroupSize, yGroupSize, zGroupSize;
    computeShader.GetKernelThreadGroupSizes(_createParticleKernelIdx, out xGroupSize, out yGroupSize, out zGroupSize);
    computeShader.Dispatch(_createParticleKernelIdx, (int)(_particleTexWidth / xGroupSize), (int)(_particleTexWidth / yGroupSize), 1);

    // Paint Particles
    FillParticleSourceTex();
    _particleSourcePreviewMaterialInstance = particleSourceRenderer.material;
    _particleSourcePreviewMaterialInstance.mainTexture = _particleSourceTex;

    _paintedParticleTex = new RenderTexture(_particleTexWidth, _particleTexWidth, 24);
    _paintedParticleTex.name = "Painted Particle Render Texture";
    _paintedParticleTex.enableRandomWrite = true;
    _paintedParticleTex.Create();

    _paintedParticlePreviewMaterialInstance = paintedParticleRenderer.material;
    _paintedParticlePreviewMaterialInstance.mainTexture = _paintedParticleTex;

    _paintParticlesKernelIdx = computeShader.FindKernel("PaintParticles");
    computeShader.SetTexture(_paintParticlesKernelIdx, "_particleTex", _particleTex);
    computeShader.SetTexture(_paintParticlesKernelIdx, "_particleSourceTex", _particleSourceTex);
    computeShader.SetTexture(_paintParticlesKernelIdx, "_paintedParticleTex", _paintedParticleTex);
    _clearParticlesKernelIdx = computeShader.FindKernel("ClearParticles");
    computeShader.SetTexture(_clearParticlesKernelIdx, "_paintedParticleTex", _paintedParticleTex);
  }

  private void FillParticleSourceTex() {
    _particleSourceTex = new Texture2D(1024, 1024);
    _particleSourceTex.name = "Particle Source Texture";
    Color[] pixels = new Color[1024*1024];
    Color color = Color.black;
    for (int i = 0; i < 1024; i++) {
      for (int j = 0; j < 1024; j++) {
        if (Random.value > 0.9999F) {
          color = Color.white;
        }
        else {
          color = Color.black;
        }
        pixels[i + 1024 * j] = color;
      }
    }
    _particleSourceTex.SetPixels(pixels);
    _particleSourceTex.Apply();
  }

  void LateUpdate() {
    // Thread group sizes (painted particles -- probably doesn't need to happen every update)
    uint xGroupSize, yGroupSize, zGroupSize;
    computeShader.GetKernelThreadGroupSizes(_paintParticlesKernelIdx, out xGroupSize, out yGroupSize, out zGroupSize);

    // Clear the canvas
    computeShader.Dispatch(_clearParticlesKernelIdx, (int)(_particleTexWidth / xGroupSize), (int)(_particleTexWidth / yGroupSize), 1);
    // Paint particles
    computeShader.Dispatch(_paintParticlesKernelIdx, (int)(_particleTexWidth / xGroupSize), (int)(_particleTexWidth / yGroupSize), 1);
  }

}
