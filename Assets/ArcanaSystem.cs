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
    Texture3D tex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
    tex.filterMode = FilterMode.Trilinear;
    tex.anisoLevel = 9;
    tex.wrapMode = TextureWrapMode.Clamp;

    Color[] colors = new Color[width * width * width];
    for (int s = 0; s < colors.Length; s++) {
      colors[s] = new Color(0, 0, 0, 0);

      int widthSqrd = width * width;
      int i = s % width % (widthSqrd);
      int j = (s / width) % width;
      int k = s / (widthSqrd);
      if (i <= 1 || i >= width - 2) continue;
      if (j <= 1 || j >= width - 2) continue;
      if (k <= 1 || k >= width - 2) continue;

      if (Random.value < 0.001) {
        colors[s] = Color.white;
      }
    }

    tex.SetPixels(colors);
    tex.Apply();
    return tex;
  }

}
