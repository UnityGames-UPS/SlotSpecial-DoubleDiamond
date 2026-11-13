
using System.Collections.Generic;
using UnityEngine;

public class WinBoxSelector : MonoBehaviour
{
    [Header("Win Box Sprites")]
    [SerializeField] private List<Sprite> sprites = new();

    // Get sprite by index
    public Sprite GetSprite(int index)
    {
        if (index < 0 || index >= sprites.Count)
        {
            Debug.LogWarning($"Invalid WinBox sprite index: {index}");
            return null;
        }

        return sprites[index];
    }

    // Get a random sprite
    public Sprite GetRandomSprite()
    {
        if (sprites.Count == 0)
        {
            Debug.LogWarning("No WinBox sprites assigned!");
            return null;
        }

        return sprites[Random.Range(0, sprites.Count)];
    }
}
