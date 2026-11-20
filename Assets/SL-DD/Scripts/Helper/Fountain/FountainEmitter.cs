using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class FountainEmitter : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private RectTransform itemPrefab;
    [SerializeField] private RectTransform spawnOrigin;
    [SerializeField, Min(0)] private int initialPoolSize = 20;
    [SerializeField, Min(1)] private int itemsPerBurst = 4;
    [SerializeField, Min(0.01f)] private float burstInterval = 0.2f;

    [Header("Flight")]
    [SerializeField] private Vector2 riseHeight = new Vector2(450f, 620f);
    [SerializeField] private Vector2 sidewaysDrift = new Vector2(280f, 620f);
    [SerializeField] private Vector2 riseSeconds = new Vector2(0.45f, 0.7f);
    [SerializeField] private Vector2 fallSeconds = new Vector2(0.5f, 0.8f);
    [SerializeField, Min(0f)] private float dropBelowOrigin = 240f;
    [SerializeField, Range(0f, 180f)] private float maxStartRotation = 180f;
    [SerializeField, Min(0f)] private float fadeInSeconds = 0.12f;

    private GenericObjectPool<FountainItem> pool;
    private Coroutine spawnLoop;

    internal bool StartFountain()
    {
        if (!isActiveAndEnabled || itemPrefab == null || spawnOrigin == null)
            return false;

        if (spawnLoop == null)
        {
            pool ??= new GenericObjectPool<FountainItem>(
                () => FountainItem.CreateInstance(itemPrefab, transform), initialPoolSize);
            spawnLoop = StartCoroutine(SpawnBursts());
        }

        return true;
    }

    internal void StopFountain()
    {
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        pool?.ReturnAll();
    }

    private IEnumerator SpawnBursts()
    {
        while (spawnOrigin != null)
        {
            // Every item in this burst starts at the same point in the emitter's space.
            Vector3 origin = transform.InverseTransformPoint(spawnOrigin.position);
            for (int i = 0; i < Mathf.Max(1, itemsPerBurst); i++)
            {
                float drift = Random.Range(sidewaysDrift.x, sidewaysDrift.y);
                if (Random.value < 0.5f)
                    drift = -drift;

                pool.Rent().Launch(origin, Random.Range(riseHeight.x, riseHeight.y), drift,
                    Mathf.Max(0.01f, Random.Range(riseSeconds.x, riseSeconds.y)),
                    Mathf.Max(0.01f, Random.Range(fallSeconds.x, fallSeconds.y)),
                    dropBelowOrigin, Random.Range(-maxStartRotation, maxStartRotation),
                    fadeInSeconds, pool.Return);
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, burstInterval));
        }

        spawnLoop = null;
    }

    private void OnDisable()
    {
        StopFountain();
    }
}
