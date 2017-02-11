using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

    public ComputeShader computeShader;
    public MeshRenderer particlePreviewRenderer;

    private int _createParticleKernelIdx;
    private RenderTexture _particleResultTex;
    private Material _particleTexPreviewMaterialInstance;

    private int _particleTexWidth = 1024;

    void Start() {
        _particleResultTex = new RenderTexture(_particleTexWidth, _particleTexWidth, 24);
        _particleResultTex.enableRandomWrite = true;
        _particleResultTex.Create();
        _createParticleKernelIdx = computeShader.FindKernel("CreateParticle");
        computeShader.SetInt(Shader.PropertyToID("_particleTexWidth"), _particleTexWidth);
        computeShader.SetTexture(_createParticleKernelIdx, "_particleResultTex", _particleResultTex);

        _particleTexPreviewMaterialInstance = particlePreviewRenderer.material;
        _particleTexPreviewMaterialInstance.mainTexture = _particleResultTex;
    }

    void LateUpdate() {
        uint xGroupSize, yGroupSize, zGroupSize;
        computeShader.GetKernelThreadGroupSizes(_createParticleKernelIdx, out xGroupSize, out yGroupSize, out zGroupSize);
        computeShader.Dispatch(_createParticleKernelIdx, (int)(_particleTexWidth / xGroupSize), (int)(_particleTexWidth / yGroupSize), 1);
    }

}
