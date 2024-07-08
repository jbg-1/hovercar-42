using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    [System.Serializable]
    public struct PlayerColor
    {
        public string name;
        public Material material;
        public string hexValue;
        public string[] gradientHexValues;
        public Color color;
        public Color[] gradientColors;

        public PlayerColor(string name, string hexValue, Material material, string[] gradientHexValues)
        {
            this.name = name;
            this.hexValue = hexValue;
            this.material = material;
            this.gradientHexValues = gradientHexValues;
            this.color = HexToColor(hexValue);
            this.gradientColors = HexValuesToColors(gradientHexValues);
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            else
            {
                Debug.LogError("Invalid hex color string: " + hex);
                return Color.black;
            }
        }

        private static Color[] HexValuesToColors(string[] hexValues)
        {
            Color[] colors = new Color[hexValues.Length];
            for (int i = 0; i < hexValues.Length; i++)
            {
                if (!ColorUtility.TryParseHtmlString(hexValues[i], out colors[i]))
                {
                    Debug.LogError("Invalid hex color string: " + hexValues[i]);
                    colors[i] = Color.black;
                }
            }
            return colors;
        }
    }


    [SerializeField] private Material orangeMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material purpleMaterial;
    [SerializeField] private Material cyanMaterial;
    [SerializeField] private Material pinkMaterial;

    private List<PlayerColor> colors;

    private void Awake()
    {
        colors = new List<PlayerColor>
        {
            new PlayerColor("Orange", "#FFA500", orangeMaterial, new[] { "#FFA500", "#FF8C00", "#FF7043", "#FFAB40" }),
            new PlayerColor("Blau", "#0000FF", blueMaterial, new[] { "#0000FF", "#1E90FF", "#4169E1", "#6495ED" }),
            new PlayerColor("Rot", "#FF0000", redMaterial, new[] { "#FF0000", "#FF4500", "#FF6347", "#FF7F50" }),
            new PlayerColor("Grün", "#00FF00", greenMaterial, new[] { "#00FF00", "#32CD32", "#00FA9A", "#7CFC00" }),
            new PlayerColor("Gelb", "#FFFF00", yellowMaterial, new[] { "#FFFF00", "#FFD700", "#FFEC8B", "#F0E68C" }),
            new PlayerColor("Lila", "#800080", purpleMaterial, new[] { "#800080", "#8A2BE2", "#9370DB", "#BA55D3" }),
            new PlayerColor("Hellblau", "#00FFFF", cyanMaterial, new[] { "#00FFFF", "#00CED1", "#20B2AA", "#48D1CC" }),
            new PlayerColor("Pink", "#FF00FF", pinkMaterial, new[] { "#FF00FF", "#FF1493", "#FF69B4", "#FFB6C1" })
        };
    }

    public List<PlayerColor> GetAllColors()
    {
        return colors;
    }
}
