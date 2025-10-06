using UnityEngine;

/// <summary>
/// Provides various color mixing strategies for the ColorPicker system
/// </summary>
public static class ColorMixingStrategies
{
    /// <summary>
    /// Simple average mixing (current default method)
    /// </summary>
    public static Color AverageMix(Color c1, Color c2)
    {
        return new Color(
            (c1.r + c2.r) / 2f,
            (c1.g + c2.g) / 2f,
            (c1.b + c2.b) / 2f,
            (c1.a + c2.a) / 2f
        );
    }

    /// <summary>
    /// Additive color mixing (for light-based mixing)
    /// </summary>
    public static Color AdditiveMix(Color c1, Color c2)
    {
        return new Color(
            Mathf.Clamp01(c1.r + c2.r),
            Mathf.Clamp01(c1.g + c2.g),
            Mathf.Clamp01(c1.b + c2.b),
            Mathf.Clamp01(c1.a + c2.a)
        );
    }

    /// <summary>
    /// Multiplicative color mixing (for pigment-based mixing)
    /// </summary>
    public static Color MultiplicativeMix(Color c1, Color c2)
    {
        return new Color(
            c1.r * c2.r,
            c1.g * c2.g,
            c1.b * c2.b,
            c1.a * c2.a
        );
    }


    /// <summary>
    /// Weighted mix - allows custom weight between two colors
    /// </summary>
    /// <param name="c1">First color</param>
    /// <param name="c2">Second color</param>
    /// <param name="weight">Weight of second color (0 to 1)</param>
    public static Color WeightedMix(Color c1, Color c2, float weight)
    {
        weight = Mathf.Clamp01(weight);
        return Color.Lerp(c1, c2, weight);
    }

    /// <summary>
    /// Screen blending mode (commonly used in image editing)
    /// </summary>
    public static Color ScreenMix(Color c1, Color c2)
    {
        return new Color(
            1f - (1f - c1.r) * (1f - c2.r),
            1f - (1f - c1.g) * (1f - c2.g),
            1f - (1f - c1.b) * (1f - c2.b),
            1f - (1f - c1.a) * (1f - c2.a)
        );
    }

    /// <summary>
    /// Overlay blending mode
    /// </summary>
    public static Color OverlayMix(Color c1, Color c2)
    {
        return new Color(
            OverlayChannel(c1.r, c2.r),
            OverlayChannel(c1.g, c2.g),
            OverlayChannel(c1.b, c2.b),
            (c1.a + c2.a) / 2f
        );
    }


    #region Helper Methods
    public static bool IsColorSimilar(Color color1, Color color2, float threshold = 0.2f)
    {
        return Vector4.Distance(color1, color2) <= threshold;
    }
    private static float OverlayChannel(float a, float b)
    {
        return a < 0.5f 
            ? 2f * a * b 
            : 1f - 2f * (1f - a) * (1f - b);
    }

    private static float InverseGammaCorrection(float c)
    {
        return c > 0.04045f ? Mathf.Pow((c + 0.055f) / 1.055f, 2.4f) : c / 12.92f;
    }

    private static float GammaCorrection(float c)
    {
        return c > 0.0031308f ? 1.055f * Mathf.Pow(c, 1f / 2.4f) - 0.055f : 12.92f * c;
    }

    #endregion
}
