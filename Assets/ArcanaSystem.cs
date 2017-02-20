using UnityEngine;
using UnityEngine.Rendering;

public class ArcanaSystem : MonoBehaviour {

  public Renderer arcanaVolumeRenderer;

  private Material _arcanaVolumeMaterialInstance;

  [SerializeField]
  private Texture3D _colorTex;
  private Texture3D _normalTex;

  void Start() {
    PopulateTextures();
    _arcanaVolumeMaterialInstance = arcanaVolumeRenderer.material;
    _arcanaVolumeMaterialInstance.SetTexture(Shader.PropertyToID("_ColorVolume"), _colorTex);
    _arcanaVolumeMaterialInstance.SetTexture(Shader.PropertyToID("_NormalVolume"), _normalTex);
  }

  private void PopulateTextures() {
    int width = 128;
    _colorTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
    _colorTex.filterMode = FilterMode.Trilinear;
    _colorTex.anisoLevel = 9;
    _colorTex.wrapMode = TextureWrapMode.Clamp;
    _normalTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
    _normalTex.filterMode = FilterMode.Trilinear;
    _normalTex.anisoLevel = 9;
    _normalTex.wrapMode = TextureWrapMode.Clamp;

    Color[] colors = new Color[width * width * width];
    Color[] normals = new Color[width * width * width];
    for (int s = 0; s < colors.Length; s++) {
      colors[s] = new Color(0, 0, 0, 0);
      normals[s] = new Color(1, 1, 1, 1);

      int widthSqrd = width * width;
      int i = s % width % (widthSqrd);
      int j = (s / width) % width;
      int k = s / (widthSqrd);
      if (i <= 1 || i >= width - 2) continue;
      if (j <= 1 || j >= width - 2) continue;
      if (k <= 1 || k >= width - 2) continue;

      // Render noise
      if (Random.value < 0.001) {
        colors[s] = Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1);
      }

      // Render a sphere
      Vector3 pos = new Vector3(i, j, k);
      float centeredSphereRadius = (width / 3F);
      Vector3 toPos = (pos - (Vector3.one * width)/2F);
      if (toPos.sqrMagnitude < (centeredSphereRadius)*(centeredSphereRadius)) {
        colors[s] = Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1);
        Vector3 normal = toPos.normalized;
        normals[s] = new Color(normal.x, normal.y, normal.z, 1F);
      }
    }

    _colorTex.SetPixels(colors);
    _colorTex.Apply(true, false);
    _normalTex.SetPixels(normals);
    _normalTex.Apply(true, false);
  }

}
