using UnityEngine;

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
    _colorTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
    _colorTex.wrapMode = TextureWrapMode.Clamp;
    if (point) {
      _colorTex.filterMode = FilterMode.Point;
      _colorTex.anisoLevel = 0;
    }
    else {
      _colorTex.filterMode = FilterMode.Trilinear;
      _colorTex.anisoLevel = 9;
    }
    _normalTex = new Texture3D(width, width, width, TextureFormat.RGBA32, true);
    _normalTex.wrapMode = TextureWrapMode.Clamp;
    if (point) {
      _normalTex.filterMode = FilterMode.Point;
      _normalTex.anisoLevel = 0;
    }
    else {
      _normalTex.filterMode = FilterMode.Trilinear;
      _normalTex.anisoLevel = 9;
    }

    int numSpheres = 64;
    Vector3[] spherePositions = new Vector3[numSpheres];
    float[] sphereRadii = new float[numSpheres];
    Color[] sphereColors = new Color[numSpheres];
    for (int i = 0; i < numSpheres; i++) {
      spherePositions[i] = new Vector3((int)(Random.value * width), (int)(Random.value * width), (int)(Random.value * width));
      sphereRadii[i] = Random.Range(1F, 5F);
      sphereColors[i] = GetRandomColor();
    }

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
      // if (Random.value < 0.001) {
      //   colors[s] = Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1);
      // }

      Vector3 curPos = new Vector3(i, j, k);

      // Render a sphere
      // MaybeRenderSphere((Vector3.one * width)/2F,
      //                   (width / 3F), Random.ColorHSV(0, 1, 0.5F, 1, 0.5F, 1),
      //                   curPos, colors, normals, s);

      for (int m = 0; m < numSpheres; m++) {
        MaybeRenderSphere(spherePositions[m], sphereRadii[m], sphereColors[m], curPos, colors, normals, s);
      }
      
    }

    _colorTex.SetPixels(colors);
    _colorTex.Apply(true, false);
    _normalTex.SetPixels(normals);
    _normalTex.Apply(true, false);
  }

  private void MaybeRenderSphere(Vector3 pos, float radius, Color color,
                                 Vector3 curPos, Color[] colors, Color[] normals, int curIdx) {
    Vector3 toPos = (curPos - pos);
    if (toPos.sqrMagnitude < radius * radius) {
      colors[curIdx] = color;
    }

    float normalRadius = radius * 1.4F;
    if (toPos.sqrMagnitude < normalRadius * normalRadius) {
      Vector3 normal = toPos.normalized;
      normals[curIdx] = new Color(normal.x, normal.y, normal.z, 1F);
      if (colors[curIdx].a <= 0) {
        colors[curIdx] = new Color(color.r, color.g, color.b, 0F);
      }
    }
  }

  private Color GetRandomColor() {
    return Random.ColorHSV(0, 1, 0.5F, 1, 0.3F, 1);
  }

}
