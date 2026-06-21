using UnityEngine;
using UnityEditor;
using System.IO;

public static class TreeBuilder
{
    private const string TreesFolder = "Assets/Models/Trees";
    private const string MaterialsFolder = "Assets/Models/Trees/Materials";

    [MenuItem("Tools/DarkLightFantasy/Generate Tree Prefabs")]
    public static void Generate()
    {
        if (!Directory.Exists(TreesFolder)) Directory.CreateDirectory(TreesFolder);
        if (!Directory.Exists(MaterialsFolder)) Directory.CreateDirectory(MaterialsFolder);

        var barkMat   = MakeMat("Bark",    new Color(0.30f, 0.18f, 0.08f), 0.85f);
        var pineMat   = MakeMat("PineLeaves", new Color(0.12f, 0.32f, 0.14f), 1.0f);
        var oakMat    = MakeMat("OakLeaves",  new Color(0.20f, 0.45f, 0.18f), 1.0f);
        var deadMat   = MakeMat("DeadBark",   new Color(0.22f, 0.16f, 0.10f), 1.0f);

        BuildPine(barkMat, pineMat);
        BuildOak(barkMat, oakMat);
        BuildDead(deadMat);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Trees Generated",
            $"3 tree prefabs saved to:\n{TreesFolder}\n\nUse them in Terrain → Paint Trees → Edit Trees → Add Tree.", "OK");
    }

    private static Material MakeMat(string name, Color color, float roughness)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f - roughness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 1f - roughness);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0f);

        var path = $"{MaterialsFolder}/{name}.mat";
        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    private static GameObject NewGO(string name, Transform parent, PrimitiveType type, Vector3 pos, Vector3 scl, Vector3 eul, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scl;
        go.transform.localEulerAngles = eul;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        return go;
    }

    private static void BuildPine(Material bark, Material leaves)
    {
        var root = new GameObject("Tree_Pine");
        var trunk = NewGO("Trunk", root.transform, PrimitiveType.Cylinder,
            new Vector3(0, 1.6f, 0), new Vector3(0.35f, 1.6f, 0.35f), Vector3.zero, bark);

        for (int i = 0; i < 4; i++)
        {
            float y = 1.8f + i * 0.9f;
            float s = Mathf.Lerp(2.2f, 0.6f, i / 3f);
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyComp(leaf);
            leaf.name = $"Cone_{i}";
            leaf.transform.SetParent(root.transform, false);
            leaf.transform.localPosition = new Vector3(0, y, 0);
            leaf.transform.localScale = new Vector3(s, 1.2f, s);
            leaf.GetComponent<MeshRenderer>().sharedMaterial = leaves;
            ReplaceWithCone(leaf);
        }

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0, 2.5f, 0);
        box.size = new Vector3(2.2f, 5f, 2.2f);

        SavePrefab(root, "Tree_Pine");
    }

    private static void BuildOak(Material bark, Material leaves)
    {
        var root = new GameObject("Tree_Oak");
        NewGO("Trunk", root.transform, PrimitiveType.Cylinder,
            new Vector3(0, 1.4f, 0), new Vector3(0.45f, 1.4f, 0.45f), Vector3.zero, bark);

        Vector3[] crownPos = {
            new Vector3(0, 3.2f, 0),
            new Vector3(0.8f, 3.1f, 0.3f),
            new Vector3(-0.6f, 3.0f, -0.5f),
            new Vector3(0.4f, 3.6f, -0.7f),
            new Vector3(-0.8f, 3.5f, 0.5f),
        };
        float[] crownScale = { 2.0f, 1.4f, 1.5f, 1.2f, 1.3f };
        for (int i = 0; i < crownPos.Length; i++)
        {
            NewGO($"Crown_{i}", root.transform, PrimitiveType.Sphere,
                crownPos[i], Vector3.one * crownScale[i], Vector3.zero, leaves);
        }

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0, 2.5f, 0);
        box.size = new Vector3(2.5f, 5f, 2.5f);

        SavePrefab(root, "Tree_Oak");
    }

    private static void BuildDead(Material bark)
    {
        var root = new GameObject("Tree_Dead");
        NewGO("Trunk", root.transform, PrimitiveType.Cylinder,
            new Vector3(0, 1.5f, 0), new Vector3(0.30f, 1.5f, 0.30f), Vector3.zero, bark);

        Vector3[] branchPos = {
            new Vector3(0.4f, 2.4f, 0.0f),
            new Vector3(-0.4f, 2.7f, 0.2f),
            new Vector3(0.3f, 2.9f, -0.4f),
            new Vector3(0.0f, 3.0f, 0.5f),
            new Vector3(0.6f, 1.9f, 0.2f),
        };
        Vector3[] branchRot = {
            new Vector3(0, 0, 45),
            new Vector3(0, 30, -55),
            new Vector3(20, 0, 65),
            new Vector3(-30, 60, 0),
            new Vector3(0, 90, 80),
        };
        for (int i = 0; i < branchPos.Length; i++)
        {
            NewGO($"Branch_{i}", root.transform, PrimitiveType.Cylinder,
                branchPos[i], new Vector3(0.08f, 0.6f, 0.08f), branchRot[i], bark);
        }

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0, 1.8f, 0);
        box.size = new Vector3(1.2f, 3.6f, 1.2f);

        SavePrefab(root, "Tree_Dead");
    }

    private static void DestroyComp(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
    }

    private static void ReplaceWithCone(GameObject placeholder)
    {
        var mesh = BuildConeMesh(8, 0.5f, 1f);
        var mf = placeholder.GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;
    }

    private static Mesh BuildConeMesh(int sides, float radius, float height)
    {
        var verts = new Vector3[sides + 2];
        var tris = new int[sides * 3 * 2];
        verts[0] = new Vector3(0, 0.5f, 0);
        verts[1] = new Vector3(0, -0.5f, 0);
        for (int i = 0; i < sides; i++)
        {
            float a = (float)i / sides * Mathf.PI * 2f;
            verts[2 + i] = new Vector3(Mathf.Cos(a) * radius, -0.5f, Mathf.Sin(a) * radius);
        }
        int t = 0;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            tris[t++] = 0; tris[t++] = 2 + i; tris[t++] = 2 + next;
            tris[t++] = 1; tris[t++] = 2 + next; tris[t++] = 2 + i;
        }
        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void SavePrefab(GameObject go, string name)
    {
        string path = $"{TreesFolder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[TreeBuilder] Saved {path}");
    }
}
