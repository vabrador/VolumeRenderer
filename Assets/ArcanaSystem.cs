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
    int width = 64;
    bool point = false;

    if (_colorTex == null) {
      _colorTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
      if (point) {
        _colorTex.filterMode = FilterMode.Point;
        _colorTex.anisoLevel = 0;
      }
      else {
        _colorTex.filterMode = FilterMode.Trilinear;
        _colorTex.anisoLevel = 9;
      }
      _colorTex.wrapMode = TextureWrapMode.Clamp;
    }
    if (_normalTex == null) {
      _normalTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
      if (point) {
        _normalTex.filterMode = FilterMode.Point;
        _normalTex.anisoLevel = 0;
      }
      else {
        _normalTex.filterMode = FilterMode.Trilinear;
        _normalTex.anisoLevel = 9;
      }
      _normalTex.wrapMode = TextureWrapMode.Clamp;
    }

    Vector3[] spherePositions = new Vector3[64];
    Color[]   sphereColors    = new Color[64];
    for (int i = 0; i < spherePositions.Length; i++) {
      spherePositions[i] = new Vector3(Random.value, Random.value, Random.value) * width;
      sphereColors[i] = Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1);
    }

    Color[] colors = new Color[width * width * width];
    Color[] normals = new Color[width * width * width];
    for (int s = 0; s < colors.Length; s++) {
      colors[s] = new Color(0, 0, 0, 0);
      normals[s] = new Color(1, 1, 1, 0);

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
      MaybeRenderSphere(circlePos: (Vector3.one * width) / 2F,
                        radius: 6,
                        color: Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1F, 1F, 1F),
                        curPos: new Vector3(i, j, k),
                        curIdx: s,
                        colorData: colors,
                        normalData: normals);
      
      // Maybe render other spheres
      for (int m = 0; m < spherePositions.Length; m++) {
        MaybeRenderSphere(circlePos: spherePositions[m],
                          radius: 2,
                          color: sphereColors[m],
                          curPos: new Vector3(i, j, k),
                          curIdx: s,
                          colorData: colors,
                          normalData: normals);
      }

    }

    _colorTex.SetPixels(colors);
    _colorTex.Apply(true, false);
    _normalTex.SetPixels(normals);
    _normalTex.Apply(true, false);
  }

  private void MaybeRenderSphere(Vector3 circlePos, Vector3 curPos, float radius, Color color, int curIdx, Color[] colorData, Color[] normalData) {
    Vector3 toPos = (curPos - circlePos);
    if (toPos.sqrMagnitude < (radius) * (radius)) {
      colorData[curIdx] = color;
      Vector3 normal = toPos.normalized;
      normalData[curIdx] = new Color(normal.x, normal.y, normal.z, 1F);
    }
  }

}
