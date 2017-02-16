using UnityEngine;
using UnityEngine.Rendering;

public class ArcanaSystem : MonoBehaviour {

  public Renderer arcanaVolumeRenderer;

  private Material _arcanaVolumeMaterialInstance;

  [SerializeField]
  Texture3D _noiseTex;

  void Start() {
    _noiseTex = GenerateRandomTexture();
    _arcanaVolumeMaterialInstance = arcanaVolumeRenderer.material;
    _arcanaVolumeMaterialInstance.SetTexture(Shader.PropertyToID("_Volume"), _noiseTex);
  }

  private Texture3D GenerateRandomTexture() {
    int width = 64;
    Texture3D tex = new Texture3D(width, width, width, TextureFormat.RGBA32, false);

    Color[] colors = new Color[width * width * width];
    for (int k = 0; k < colors.Length; k++) {
      colors[k] = new Color(0, 0, 0, 0);
      if (Random.value < 0.001) {
        colors[k] = Color.white;
      }
    }

    tex.SetPixels(colors);
    tex.Apply();
    return tex;
  }

}
