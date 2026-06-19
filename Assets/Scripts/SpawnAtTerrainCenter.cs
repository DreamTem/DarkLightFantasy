using UnityEngine;

[ExecuteAlways]
public class SpawnAtTerrainCenter : MonoBehaviour
{
    public Terrain targetTerrain;
    public float heightOffset = 5f;
    public bool moveRoot = true;
    public bool snapInEditor = true;

    void OnEnable()
    {
        if (!Application.isPlaying && snapInEditor)
            DoCenter();
    }

    void Start()
    {
        if (Application.isPlaying)
            DoCenter();
    }

    [ContextMenu("Center On Terrain Now")]
    public void DoCenter()
    {
        if (targetTerrain == null)
            targetTerrain = Terrain.activeTerrain;
        if (targetTerrain == null)
        {
            Debug.LogWarning("[SpawnAtTerrainCenter] No Terrain found in scene.");
            return;
        }

        var data = targetTerrain.terrainData;
        Vector3 origin = targetTerrain.transform.position;
        Vector3 center = origin + new Vector3(data.size.x * 0.5f, 0f, data.size.z * 0.5f);
        center.y = targetTerrain.SampleHeight(center) + origin.y + heightOffset;

        Transform target = moveRoot ? transform.root : transform;

        if (Application.isPlaying)
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = center;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                target.position = center;
            }
        }
        else
        {
            target.position = center;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }
    }
}
