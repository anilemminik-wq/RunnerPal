using System.Collections.Generic;
using UnityEngine;

// Rounded-corner primitives for the cartoon art pass: replaces sharp CreatePrimitive(Cube) boxes with smooth,
// beveled ones everywhere the game builds its own geometry (props, obstacles, road furniture). Classic "rounded
// cube" technique: build a subdivided box, then push every vertex out to a sphere of the given radius near the
// corners/edges while keeping flat faces in the middle.
public static class ProceduralMesh
{
    // size = full box dimensions, radius = corner rounding (capped to half the smallest dimension),
    // segments = subdivisions along each edge (more = smoother curve, 6-10 is enough at gameplay scale).
    public static Mesh RoundedBox(Vector3 size, float radius, int segments = 6)
    {
        radius = Mathf.Min(radius, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.5f - 0.001f);
        radius = Mathf.Max(radius, 0f);
        Vector3 half = size * 0.5f;
        Vector3 inner = half - Vector3.one * radius; // the flat "core" box the rounding is built around

        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        void Face(Vector3 normal, Vector3 uAxis, Vector3 vAxis)
        {
            int baseIndex = verts.Count;
            for (int iy = 0; iy <= segments; iy++)
            {
                float v = (float)iy / segments;
                for (int ix = 0; ix <= segments; ix++)
                {
                    float u = (float)ix / segments;
                    // Position on the flat face, in the core-box's local frame.
                    Vector3 flat = Vector3.Scale(normal, half) + Vector3.Scale(uAxis, half) * (u * 2f - 1f) + Vector3.Scale(vAxis, half) * (v * 2f - 1f);
                    // Clamp each axis to the inner box, then push the leftover out along a normalized direction:
                    // corners round in 3D, edges round in 2D, faces stay flat.
                    Vector3 clamped = new Vector3(
                        Mathf.Clamp(flat.x, -inner.x, inner.x),
                        Mathf.Clamp(flat.y, -inner.y, inner.y),
                        Mathf.Clamp(flat.z, -inner.z, inner.z));
                    Vector3 diff = flat - clamped;
                    Vector3 dir = diff.sqrMagnitude > 1e-8f ? diff.normalized : normal;
                    Vector3 pos = clamped + dir * radius;
                    verts.Add(pos);
                    norms.Add(dir.sqrMagnitude > 1e-8f ? dir : normal);
                    uvs.Add(new Vector2(u, v));
                }
            }
            int row = segments + 1;
            for (int iy = 0; iy < segments; iy++)
                for (int ix = 0; ix < segments; ix++)
                {
                    int i0 = baseIndex + iy * row + ix;
                    int i1 = i0 + 1, i2 = i0 + row, i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
        }

        Face(Vector3.up, Vector3.right, Vector3.forward);
        Face(Vector3.down, Vector3.right, Vector3.back);
        Face(Vector3.right, Vector3.forward, Vector3.up);
        Face(Vector3.left, Vector3.back, Vector3.up);
        Face(Vector3.forward, Vector3.left, Vector3.up);
        Face(Vector3.back, Vector3.right, Vector3.up);

        var mesh = new Mesh { name = "RoundedBox" };
        if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }


    // A smooth capsule-ish rounded cylinder (rounded top/bottom rim), for posts and poles.
    public static Mesh RoundedCylinder(float radius, float height, float rim, int radialSegments = 12, int rimSegments = 3)
    {
        rim = Mathf.Min(rim, Mathf.Min(radius, height * 0.5f) - 0.001f);
        rim = Mathf.Max(rim, 0f);
        float coreR = radius - rim, coreH = height * 0.5f - rim;
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();
        int rows = rimSegments * 2 + 1;
        float bottomFrac = rimSegments / (float)rows, topFrac = (rimSegments + 1) / (float)rows;
        for (int row = 0; row <= rows; row++)
        {
            float t = (float)row / rows;
            // Three zones: rounded bottom cap, straight side, rounded top cap (like a capsule's ends).
            float ringY, ringR, normalY;
            if (t < bottomFrac)
            {
                float a = Mathf.Lerp(-90f, 0f, t / bottomFrac) * Mathf.Deg2Rad;
                ringY = -coreH + Mathf.Sin(a) * rim; ringR = coreR + Mathf.Cos(a) * rim; normalY = Mathf.Sin(a);
            }
            else if (t > topFrac)
            {
                float a = Mathf.Lerp(0f, 90f, (t - topFrac) / bottomFrac) * Mathf.Deg2Rad;
                ringY = coreH + Mathf.Sin(a) * rim; ringR = coreR + Mathf.Cos(a) * rim; normalY = Mathf.Sin(a);
            }
            else
            {
                ringY = Mathf.Lerp(-coreH, coreH, (t - bottomFrac) / (topFrac - bottomFrac)); ringR = coreR + rim; normalY = 0f;
            }
            float normalXZ = Mathf.Sqrt(Mathf.Max(0f, 1f - normalY * normalY));
            for (int i = 0; i <= radialSegments; i++)
            {
                float ang = (float)i / radialSegments * Mathf.PI * 2f;
                float cx = Mathf.Cos(ang), cz = Mathf.Sin(ang);
                verts.Add(new Vector3(cx * ringR, ringY, cz * ringR));
                norms.Add(new Vector3(cx * normalXZ, normalY, cz * normalXZ));
                uvs.Add(new Vector2((float)i / radialSegments, t));
            }
        }
        int rowLen = radialSegments + 1;
        for (int row = 0; row < rows; row++)
            for (int i = 0; i < radialSegments; i++)
            {
                int i0 = row * rowLen + i, i1 = i0 + 1, i2 = i0 + rowLen, i3 = i2 + 1;
                tris.Add(i0); tris.Add(i2); tris.Add(i1);
                tris.Add(i1); tris.Add(i2); tris.Add(i3);
            }
        // Flat caps: the rim only bevels the edge, so the core radius at each pole still needs a disc.
        void Cap(float y, int ringStart, bool up)
        {
            int center = verts.Count;
            verts.Add(new Vector3(0f, y, 0f));
            norms.Add(up ? Vector3.up : Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i < radialSegments; i++)
            {
                int a = ringStart + i, b = ringStart + i + 1;
                if (up) { tris.Add(center); tris.Add(a); tris.Add(b); }
                else { tris.Add(center); tris.Add(b); tris.Add(a); }
            }
        }
        if (coreR > 0.001f) { Cap(-coreH - rim, 0, false); Cap(coreH + rim, rows * rowLen, true); }
        var mesh = new Mesh { name = "RoundedCylinder" };
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
