using Prototype;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class LD58_RandomInstantiater : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] string map = "_MainTex";
#endif

    [SerializeField, ReadOnly] MeshRenderer meshRenderer;
    [SerializeField, ReadOnly] MeshCollider meshCollider;

#if UNITY_EDITOR
    [SerializeField] GameObject[] prefabs;
    [SerializeField] int seed = 0;
    [SerializeField] float spacing = 3;
    [SerializeField, Range(0, 1)] float maxOffset = .5f;
    [SerializeField] Vector2 scale = new(.9f, 1.1f);
    [SerializeField] Vector2 yScale = new(.5f, 1.5f);
    [SerializeField] int maxLoops = 100000;
    [SerializeField] int maxObjects = 10000;
#endif

#if UNITY_EDITOR
    [ContextMenu("Setup")]
    void Setup()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer)
        {
            meshRenderer.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetByGUID<Material>(
                UnityEditor.AssetDatabase.FindAssetGUIDs("\"editor mask\" t:material").FirstOrDefault()
            );
            UnityEditor.EditorUtility.SetDirty(meshRenderer);
        }

        meshCollider = GetComponent<MeshCollider>();

        transform.localPosition = new(0, 0, -7);
        transform.localScale = Vector3.one * 60;

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [SerializeField, ReadOnly] List<Vector2> randomPoints;

    [ContextMenu("Generate Points")]
    void GeneratePoints()
    {
        if (!meshCollider)
        {
            return;
        }

        meshCollider.enabled = true;

        var state = Random.state;
        Random.InitState(seed);

        var bounds = meshCollider.bounds;

        randomPoints.Clear();

        var i = 0;

        for (var x = bounds.min.x; x < bounds.max.x; x += spacing)
        {
            for (var y = bounds.min.y; y < bounds.max.y; y += spacing)
            {
                if (i >= maxLoops || randomPoints.Count >= maxObjects)
                {
                    goto Break;
                }

                var position = new Vector3(
                    x + (Utils.RandomFloat() * 2 - 1) * spacing * maxOffset,
                    y + (Utils.RandomFloat() * 2 - 1) * spacing * maxOffset,
                    transform.position.z - 1
                );

                i++;

                if (Utils.RandomTrue(GetPixel(position)))
                {
                    randomPoints.Add((Vector2) position);
                }
            }
        }

    Break:
        Random.state = state;

        meshCollider.enabled = false;

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("Generate Game Objects")]
    void GenerateGameObjects()
    {
        if (!meshCollider)
        {
            return;
        }

        meshCollider.enabled = true;

        var state = Random.state;
        Random.InitState(seed);

        transform.DestroyChildrenImmediate();

        foreach (var point in randomPoints)
        {
            var instance = Utils.RandomPick(prefabs).Instantiate(transform);
            instance.transform.position = point;

            instance.transform.localScale =
                instance.transform.localScale
                / transform.localScale.x
                * Utils.RandomRange(scale)
                * Mathf.LerpUnclamped(yScale.y, yScale.x, (point.y - transform.position.y) / meshCollider.bounds.size.y);
        }

        Random.state = state;

        meshCollider.enabled = false;

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("Generate Points && Game Objects")]
    void GeneratePointsAndGameObjects()
    {
        GeneratePoints();
        GenerateGameObjects();
    }
#endif

    void Update()
    {
        if (Application.isPlaying)
        {
            meshRenderer.enabled = false;
            meshCollider.enabled = false;
            return;
        }

#if UNITY_EDITOR
        var isSelected = UnityEditor.Selection.activeGameObject == gameObject;

        if (isSelected)
        {
            if (meshRenderer)
            {
                meshRenderer.enabled = true;
            }
            if (meshCollider)
            {
                meshCollider.enabled = true;
            }
        }
        else
        {
            if (meshRenderer)
            {
                meshRenderer.enabled = false;
            }
            if (meshCollider)
            {
                meshCollider.enabled = false;
            }
        }
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        foreach (var point in randomPoints)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(point, .25f);
        }
    }

    float GetPixel(Vector3 position)
    {
        var results = new RaycastHit[1];
        var count = Physics.RaycastNonAlloc(position, Vector3.forward, results, 2);

        if (count == 0)
        {
            return 0;
        }

        var hit = results[0];
        var uv = hit.textureCoord;
        if (!hit.transform.TryGetComponent(out MaterialPropertyOverride.MaterialPropertyOverride @override))
        {
            return 0;
        }

        var mainTex = @override.textureProperties.FirstOrDefault(x => x.name == map);
        if (mainTex is null)
        {
            return 0;
        }

        var texture = mainTex.value as Texture2D;
        if (!texture)
        {
            return 0;
        }

        var pixel = texture.GetPixelBilinear(uv.x, uv.y);
        return pixel.r;
    }
#endif
}
