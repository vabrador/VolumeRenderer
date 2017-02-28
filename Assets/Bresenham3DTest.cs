using System;
using System.Collections.Generic;
using UnityEngine;

public class Bresenham3DTest : MonoBehaviour {

  public Transform start;
  public Transform end;

  private List<GameObject> _cubes = new List<GameObject>();

  void Update() {
    if (start.hasChanged || end.hasChanged) {
      for (int i = 0; i < _cubes.Count; i++) {
        Destroy(_cubes[i]);
      }
      _cubes.Clear();

      Bresenham3D.Line(start.position, end.position, MakeCube);

      start.hasChanged = false;
      end.hasChanged = false;
    }
  }
	
  private void MakeCube(Vector3 position) {
    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    obj.transform.position = position;
    _cubes.Add(obj);
  }

}

public static class Bresenham3D {

  public static void Line(Vector3 start, Vector3 end, Action<Vector3> illuminateFunction, int midVoxelResolution=1024) {
    int m = midVoxelResolution;

    float x = start.x, y = start.y, z = start.z;

    float changeX = (end.x - start.x);
    float changeY = (end.y - start.y);
    float changeZ = (end.z - start.z);

    int dx = (int)(m * changeX);
    int dy = (int)(m * changeY);
    int dz = (int)(m * changeZ);

    int xInc = 1, yInc = 1, zInc = 1;
    if (dx < 0) xInc = -1;
    if (dy < 0) yInc = -1;
    if (dz < 0) zInc = -1;

    int absDx = Mathf.Abs(dx);
    int absDy = Mathf.Abs(dy);
    int absDz = Mathf.Abs(dz);

    int iDx = Mathf.Abs((int)changeX);
    int iDy = Mathf.Abs((int)changeY);
    int iDz = Mathf.Abs((int)changeZ);

    int absDx2 = absDx * 2;
    int absDy2 = absDy * 2;
    int absDz2 = absDz * 2;

    int err1, err2;
    illuminateFunction(new Vector3(x, y, z));
    if (absDx >= absDy && absDx >= absDz) {
      err1 = absDy2 - absDx;
      err2 = absDz2 - absDx;

      for (int i = 0; i < iDx; i++) {
        if (err1 > 0) {
          y += yInc;
          err1 -= absDx2;
        }

        if (err2 > 0) {
          z += zInc;
          err2 -= absDx2;
        }

        err1 += absDy2;
        err2 += absDz2;
        x += xInc;

        illuminateFunction(new Vector3(x, y, z));
      }
    }
    else if (absDy > absDx && absDy >= absDz) {
      err1 = absDx2 - absDy;
      err2 = absDz2 - absDy;

      for (int i = 0; i < iDy; i++) {
        if (err1 > 0) {
          x += xInc;
          err1 -= absDy2;
        }

        if (err2 > 0) {
          z += zInc;
          err2 -= absDy2;
        }

        err1 += absDx2;
        err2 += absDz2;
        y += yInc;

        illuminateFunction(new Vector3(x, y, z));
      }
    }
    else { // AbsDz > AbsDx && AbsDz > AbsDy
      err1 = absDx2 - absDz;
      err2 = absDy2 - absDz;

      for (int i = 0; i < iDz; i++) {
        if (err1 > 0) {
          x += xInc;
          err1 -= absDz2;
        }

        if (err2 > 0) {
          y += yInc;
          err2 -= absDz2;
        }

        err1 += absDx2;
        err2 += absDy2;
        z += zInc;

        illuminateFunction(new Vector3(x, y, z));
      }
    }
  }

}
