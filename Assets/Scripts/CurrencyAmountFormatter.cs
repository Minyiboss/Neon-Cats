using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// Formats whole currency amounts with thousand-tier suffixes: a, b, … z, aa, ab, …
/// Tier 1 = /1e3, tier 2 = /1e6, etc. Safe for values up to <see cref="int.MaxValue"/>.
/// </summary>
public static class CurrencyAmountFormatter
{
    public static string Format(int value)
    {
        if (value < 0)
            value = 0;
        if (value < 1000)
            return value.ToString(CultureInfo.InvariantCulture);

        double scaled = value;
        int tier = 0;
        while (scaled >= 1000.0)
        {
            scaled /= 1000.0;
            tier++;
        }

        return FormatScaled(scaled) + TierToSuffix(tier);
    }

    /// <summary>Floors a non-negative currency float and formats it (clamps to int.MaxValue).</summary>
    public static string FormatFromFloat(float currency)
    {
        if (currency <= 0f)
            return "0";
        double d = System.Math.Floor(currency);
        if (d >= int.MaxValue)
            return Format(int.MaxValue);
        return Format((int)d);
    }

    /// <summary>
    /// For stats like income multiplier where decimals matter. Does not floor and does not use currency suffixes.
    /// </summary>
    public static string FormatStatFloat(float value)
    {
        if (value <= 0f)
            return "0";
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    static string FormatScaled(double scaled)
    {
        if (scaled >= 100.0)
            return Mathf.FloorToInt((float)(scaled + 1e-4)).ToString(CultureInfo.InvariantCulture);
        if (scaled >= 10.0)
            return (Mathf.FloorToInt((float)(scaled * 10f + 1e-4)) / 10f).ToString("0.#", CultureInfo.InvariantCulture);
        return (Mathf.FloorToInt((float)(scaled * 100f + 1e-4)) / 100f).ToString("0.##", CultureInfo.InvariantCulture);
    }

    /// <summary>1 → a, 26 → z, 27 → aa (same indexing as spreadsheet columns).</summary>
    static string TierToSuffix(int tier)
    {
        if (tier <= 0)
            return string.Empty;

        var sb = new StringBuilder();
        int n = tier;
        while (n > 0)
        {
            n--;
            sb.Insert(0, (char)('a' + (n % 26)));
            n /= 26;
        }

        return sb.ToString();
    }
}
