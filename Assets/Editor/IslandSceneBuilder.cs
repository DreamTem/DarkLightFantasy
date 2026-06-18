using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class IslandSceneBuilder
{
    private const string SceneAssetPath = "Assets/Scenes/Scene2.unity";
    private const string TerrainDataPath = "Assets/Scenes/Scene2_IslandTerrain.asset";

    [MenuItem("Tools/DarkLightFantasy/Build Island Scene2")]
    public static void Build()
    {
        var dir = Path.GetDirectoryName(SceneAssetPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Scene2";

        CreateTerrain();
        CreateWater();
        CreateLighting();
        CreatePlayer();
        CreateCamera();
        PlaceWitchHouse();
        CreateSkybox();

        EditorSceneManager.SaveScene(scene, SceneAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[IslandSceneBuilder] Scene2 created at " + SceneAssetPath);
        EditorUtility.DisplayDialog("Island Scene Built",
            "Scene2 created at " + SceneAssetPath + "\n\nPress Play to test.", "OK");
    }

    private static void CreateTerrain()
    {
        int res = 257;
        var data = new TerrainData
        {
            heightmapResolution = res,
            size = new Vector3(500f, 60f, 500f),
            alphamapResolution = 256,
            baseMapResolution = 256
        };

        float[,] heights = new float[res, res];
        Vector2 center = new Vector2(res * 0.5f, res * 0.5f);
        float maxR = res * 0.42f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxR;
                float falloff = Mathf.Clamp01(1f - dist);
                float island = Mathf.SmoothStep(0f, 1f, falloff);

                float n1 = Mathf.PerlinNoise(x * 0.02f + 13f, y * 0.02f + 7f);
                float n2 = Mathf.PerlinNoise(x * 0.06f + 51f, y * 0.06f + 91f) * 0.5f;
                float n3 = Mathf.PerlinNoise(x * 0.12f + 200f, y * 0.12f + 33f) * 0.25f;
                float noise = (n1 + n2 + n3) / 1.75f;

                float baseH = 0.10f;
                float h = baseH + island * (0.20f + noise * 0.45f);
                heights[y, x] = Mathf.Clamp01(h);
            }
        }
        data.SetHeights(0, 0, heights);

        ApplyTerrainLayers(data);

        AssetDatabase.CreateAsset(data, TerrainDataPath);
        AssetDatabase.SaveAssets();

        var terrainGO = Terrain.CreateTerrainGameObject(data);
        terrainGO.name = "IslandTerrain";
        terrainGO.transform.position = new Vector3(-250f, 0f, -250f);
        terrainGO.isStatic = true;
    }

    private static void ApplyTerrainLayers(TerrainData data)
    {
        var sand = MakeLayer("Sand", new Color(0.85f, 0.78f, 0.55f), new Vector2(15f, 15f));
        var grass = MakeLayer("Grass", new Color(0.30f, 0.45f, 0.18f), new Vector2(15f, 15f));
        var rock = MakeLayer("Rock", new Color(0.40f, 0.38f, 0.35f), new Vector2(15f, 15f));
        var dirt = MakeLayer("Dirt", new Color(0.32f, 0.22f, 0.14f), new Vector2(15f, 15f));

        data.terrainLayers = new TerrainLayer[] { sand, grass, rock, dirt };

        int aRes = data.alphamapResolution;
        var splat = new float[aRes, aRes, 4];

        for (int y = 0; y < aRes; y++)
        {
            for (int x = 0; x < aRes; x++)
            {
                float normX = (float)x / (aRes - 1);
                float normY = (float)y / (aRes - 1);
                float h = data.GetInterpolatedHeight(normX, normY) / data.size.y;
                float steepness = data.GetSteepness(normX, normY);

                float wSand = h < 0.16f ? 1f : Mathf.SmoothStep(1f, 0f, (h - 0.16f) * 10f);
                float wRock = Mathf.Clamp01((steepness - 25f) / 30f);
                float wGrass = (1f - wSand - wRock) * (1f - Mathf.Clamp01((h - 0.55f) * 4f));
                float wDirt = 1f - wSand - wRock - wGrass;
                wDirt = Mathf.Clamp01(wDirt);

                float sum = wSand + wGrass + wRock + wDirt;
                if (sum < 0.0001f) { wGrass = 1f; sum = 1f; }
                splat[y, x, 0] = wSand / sum;
                splat[y, x, 1] = wGrass / sum;
                splat[y, x, 2] = wRock / sum;
                splat[y, x, 3] = wDirt / sum;
            }
        }
        data.SetAlphamaps(0, 0, splat);
    }

    private static TerrainLayer MakeLayer(string name, Color color, Vector2 size)
    {
        var layer = new TerrainLayer();
        var tex = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            float n = Mathf.PerlinNoise((i % 64) * 0.15f, (i / 64) * 0.15f);
            pixels[i] = color * (0.8f + n * 0.4f);
            pixels[i].a = 1f;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        layer.diffuseTexture = tex;
        layer.tileSize = size;

        string layerPath = $"Assets/Scenes/Scene2_Layer_{name}.terrainlayer";
        AssetDatabase.CreateAsset(layer, layerPath);
        return layer;
    }

    private static void CreateWater()
    {
        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0f, 9.5f, 0f);
        water.transform.localScale = new Vector3(150f, 1f, 150f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = new Color(0.20f, 0.45f, 0.65f, 0.75f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);
        var matPath = "Assets/Scenes/Scene2_Water.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        water.GetComponent<MeshRenderer>().sharedMaterial = mat;

        var collider = water.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static void CreateLighting()
    {
        var sunGO = new GameObject("Directional Light (Sun)");
        var sun = sunGO.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.2f;
        sun.color = new Color(1f, 0.92f, 0.78f);
        sun.shadows = LightShadows.Soft;
        sunGO.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.45f, 0.55f, 0.75f);
        RenderSettings.ambientEquatorColor = new Color(0.50f, 0.50f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.25f, 0.20f, 0.15f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.55f, 0.65f, 0.75f);
        RenderSettings.fogDensity = 0.005f;
    }

    private static void CreatePlayer()
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 25f, 0f);
        var rb = player.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        var moveType = System.Type.GetType("CubeMovement, Assembly-CSharp");
        if (moveType != null) player.AddComponent(moveType);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = new Color(0.85f, 0.20f, 0.15f);
        player.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1500f;
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0f, 35f, -15f);
        camGO.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

        var followType = System.Type.GetType("CameraFollow, Assembly-CSharp");
        if (followType != null) camGO.AddComponent(followType);
    }

    private static void PlaceWitchHouse()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/WitchHouse.fbx");
        if (prefab == null)
        {
            Debug.LogWarning("[IslandSceneBuilder] WitchHouse.fbx not found");
            return;
        }
        var house = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        house.name = "WitchHouse";

        var terrain = Terrain.activeTerrain;
        float gx = 30f, gz = 20f;
        float gy = terrain != null ? terrain.SampleHeight(new Vector3(gx, 0, gz)) + terrain.transform.position.y : 15f;
        house.transform.position = new Vector3(gx, gy, gz);
        house.transform.rotation = Quaternion.Euler(0f, 135f, 0f);
    }

    private static void CreateSkybox()
    {
        var mat = new Material(Shader.Find("Skybox/Procedural"));
        mat.SetColor("_SkyTint", new Color(0.55f, 0.60f, 0.75f));
        mat.SetColor("_GroundColor", new Color(0.30f, 0.30f, 0.35f));
        mat.SetFloat("_AtmosphereThickness", 1.4f);
        mat.SetFloat("_Exposure", 1.1f);
        var skyPath = "Assets/Scenes/Scene2_Sky.mat";
        AssetDatabase.CreateAsset(mat, skyPath);
        RenderSettings.skybox = mat;
        DynamicGI.UpdateEnvironment();
    }
}
