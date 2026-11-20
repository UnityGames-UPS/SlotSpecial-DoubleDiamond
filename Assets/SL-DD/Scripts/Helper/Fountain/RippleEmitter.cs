using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class RippleEmitter : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private RectTransform itemPrefab;
    [SerializeField] private RectTransform spawnOrigin;
    [SerializeField, Min(0)] private int initialPoolSize = 32;

    [Header("Ring")]
    [SerializeField, Min(1)] private int itemsPerRing = 20;
    [SerializeField, Min(0f)] private float startRadius = 0f;
    [SerializeField, Min(0f)] private float endRadius = 400f;
    [SerializeField, Min(0.01f)] private float expansionSeconds = 0.8f;
    [SerializeField, Min(0f)] private float fadeOutSeconds = 0.25f;
    [SerializeField, Min(0.01f)] private float ringInterval = 1f;

    private GenericObjectPool<FountainItem> pool;
    private Coroutine spawnLoop;

    internal void StartRipple()
    {
        if (!isActiveAndEnabled || spawnLoop != null || itemPrefab == null || spawnOrigin == null)
            return;

        pool ??= new GenericObjectPool<FountainItem>(
            () => FountainItem.CreateInstance(itemPrefab, transform), initialPoolSize);
        spawnLoop = StartCoroutine(SpawnRings());
    }

    internal void StopRipple()
    {
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        pool?.ReturnAll();
    }

    private IEnumerator SpawnRings()
    {
        while (spawnOrigin != null)
        {
            SpawnRing();
            yield return new WaitForSeconds(Mathf.Max(0.01f, ringInterval));
        }

        spawnLoop = null;
    }

    private void SpawnRing()
    {
        Vector3 center = transform.InverseTransformPoint(spawnOrigin.position);
        int count = Mathf.Max(1, itemsPerRing);
        float angleStep = 2f * Mathf.PI / count;
        float inner = Mathf.Max(0f, startRadius);
        float outer = Mathf.Max(inner, endRadius);
        float duration = Mathf.Max(0.01f, expansionSeconds);

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            pool.Rent().LaunchRipple(center + direction * inner, center + direction * outer,
                duration, fadeOutSeconds, pool.Return);
        }
    }

    private void OnDisable()
    {
        StopRipple();
    }
}
