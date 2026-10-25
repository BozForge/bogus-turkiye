using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BozForge.Bogus.Turkiye;

/// <summary>
/// Kontrol hanesi hesapları. Üreticiler ve testler aynı kodu kullanır.
/// </summary>
internal static class Checksums
{
    /// <summary>
    /// TCKN'nin 10. ve 11. hanesi. İlk 9 hane verilir, ilk hane 0 olamaz.
    /// </summary>
    public static (int Tenth, int Eleventh) TcknCheckDigits(int[] first9)
    {
        if (first9.Length != 9) throw new ArgumentException("TCKN için 9 hane gerekir.", nameof(first9));

        var odd = first9[0] + first9[2] + first9[4] + first9[6] + first9[8];
        var even = first9[1] + first9[3] + first9[5] + first9[7];

        var tenth = ((odd * 7 - even) % 10 + 10) % 10;
        var eleventh = (first9.Sum() + tenth) % 10;

        return (tenth, eleventh);
    }

    /// <summary>
    /// VKN'nin 10. hanesi. Gelir İdaresi'nin algoritması; python-stdnum'daki
    /// <c>stdnum.tr.vkn</c> ile aynı sonucu verir.
    /// </summary>
    public static int VknCheckDigit(int[] first9)
    {
        if (first9.Length != 9) throw new ArgumentException("VKN için 9 hane gerekir.", nameof(first9));

        var sum = 0;
        for (var i = 1; i <= 9; i++)
        {
            // Sağdan sola: i = 1 son hane (9.), i = 9 ilk hane.
            var digit = first9[9 - i];
            var c1 = (digit + i) % 10;
            if (c1 == 0) continue;

            var c2 = c1 * (1 << i) % 9;
            sum += c2 == 0 ? 9 : c2;
        }

        return (10 - sum % 10) % 10;
    }

    /// <summary>
    /// ISO 13616 (mod 97) kontrol haneleri. <paramref name="bban"/> ülke kodu ve
    /// kontrol haneleri olmadan, yalnızca hesap kısmıdır.
    /// </summary>
    public static string IbanCheckDigits(string countryCode, string bban)
    {
        var rearranged = bban + countryCode + "00";
        var remainder = Mod97(rearranged);
        return (98 - remainder).ToString("00");
    }

    /// <summary>Boşluksuz bir IBAN'ın mod 97 kontrolünü doğrular.</summary>
    public static bool IsValidIbanChecksum(string iban)
    {
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);
        return Mod97(rearranged) == 1;
    }

    /// <summary>Luhn kontrolü (kart numaraları).</summary>
    public static bool IsValidLuhn(string digits)
    {
        var sum = 0;
        var doubleIt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleIt)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }

            sum += d;
            doubleIt = !doubleIt;
        }

        return sum % 10 == 0;
    }

    private static int Mod97(string alphanumeric)
    {
        var numeric = new StringBuilder(alphanumeric.Length * 2);
        foreach (var ch in alphanumeric)
        {
            if (char.IsDigit(ch)) numeric.Append(ch);
            else numeric.Append(char.ToUpperInvariant(ch) - 'A' + 10);
        }

        return (int)(BigInteger.Parse(numeric.ToString()) % 97);
    }
}
