using System;
using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public class ColourConfig
{
    [Serializable]
    public struct ColourSet
    {
        public Color BackgroundColour;
        public Color ForegroundColour;
        public Color HighlightColour;
    }

    [SerializeField]
    private ColourSet[] sets;

    public ColourSet CurrentSet { get; private set; }

    public void GenerateColourSet()
    {
        this.CurrentSet = this.sets[Random.Range(0, this.sets.Length)];
    }
}
