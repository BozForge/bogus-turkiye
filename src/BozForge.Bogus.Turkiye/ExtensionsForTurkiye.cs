using System;
using System.Linq;
using System.Text;
using Bogus;
using Bogus.DataSets;

namespace BozForge.Bogus.Turkiye;

/// <summary>GSM numarasının yazım biçimi.</summary>
public enum TrPhoneFormat
{
    /// <summary><c>+905321234567</c></summary>
    E164,
    /// <summary><c>+90 532 123 45 67</c></summary>
    International,
    /// <summary><c>0532 123 45 67</c></summary>
    National,
    /// <summary><c>5321234567</c></summary>
    Digits,
}

/// <summary>
/// Türkiye'ye özgü Bogus eklentileri. Üretilen her değer kontrol hanelerinden geçer;
/// değerler yalnızca test ve geliştirme içindir, gerçek kişilere ya da hesaplara denk
/// gelebilir.
/// </summary>
public static class ExtensionsForTurkiye
{
    private static readonly TrBank[] Banks = (TrBank[])Enum.GetValues(typeof(TrBank));

    // Turkcell 53x, Vodafone 54x, Türk Telekom 50x ve 55x. Numara taşıma nedeniyle
    // önek operatörü kesin göstermez; burada yalnızca geçerli önek olması önemli.
    private static readonly string[] GsmPrefixes =
    {
        "501", "505", "506", "507",
        "530", "531", "532", "533", "534", "535", "536", "537", "538", "539",
        "541", "542", "543", "544", "545", "546", "547", "548", "549",
        "551", "552", "553", "554", "555", "559",
    };

    /// <summary>
    /// T.C. Kimlik Numarası: 11 hane, ilk hane 0 değil, 10. ve 11. haneler kontrol hanesi.
    /// Aynı <see cref="Person"/> için her çağrıda aynı numarayı döndürür.
    /// </summary>
    public static string Tckn(this Person person)
    {
        const string key = nameof(ExtensionsForTurkiye) + "TCKN";
        var context = ((IHasContext)person).Context;
        if (context.TryGetValue(key, out var cached)) return (string)cached;

        var tckn = Tckn(person.Random);
        context[key] = tckn;
        return tckn;
    }

    /// <summary>
    /// Vergi Kimlik Numarası (tüzel kişiler): 10 hane, son hane kontrol hanesi.
    /// </summary>
    public static string Vkn(this Company company)
    {
        var first9 = new int[9];
        first9[0] = company.Random.Int(1, 9);
        for (var i = 1; i < 9; i++) first9[i] = company.Random.Int(0, 9);

        var check = Checksums.VknCheckDigit(first9);
        return string.Concat(first9) + check;
    }

    /// <summary>
    /// TR IBAN: 26 karakter, gerçek bir banka kodu, rezerv hanesi 0 ve 16 haneli
    /// rakamlardan oluşan hesap numarası. Mod 97 kontrolünden geçer.
    /// </summary>
    /// <param name="finance">Bogus finans veri seti.</param>
    /// <param name="bank">Banka; verilmezse rastgele seçilir.</param>
    /// <param name="formatted"><c>true</c> ise dörder karakterlik gruplar halinde boşluklu yazılır.</param>
    public static string TrIban(this Finance finance, TrBank? bank = null, bool formatted = false)
    {
        var chosen = bank ?? finance.Random.ArrayElement(Banks);

        var bban = new StringBuilder(22);
        bban.Append(((int)chosen).ToString("00000"));
        bban.Append('0');
        for (var i = 0; i < 16; i++) bban.Append((char)('0' + finance.Random.Int(0, 9)));

        var iban = "TR" + Checksums.IbanCheckDigits("TR", bban.ToString()) + bban;
        return formatted ? FormatIban(iban) : iban;
    }

    /// <summary>
    /// Herkese açık bir test kartı seçer ve ileri tarihli son kullanma tarihi ile CVV ekler.
    /// Son kullanma yılı bugünün yılına göre hesaplanır.
    /// </summary>
    /// <param name="finance">Bogus finans veri seti.</param>
    /// <param name="scheme">Kart kuruluşu; verilmezse tüm kartlar arasından seçilir.</param>
    /// <param name="type">Kart tipi; verilmezse kredi ve banka kartları arasından seçilir.</param>
    public static TrTestCard TrTestCard(this Finance finance, CardScheme? scheme = null, CardType? type = null)
    {
        var candidates = TestCardData.All
            .Where(c => scheme is null || c.Scheme == scheme)
            .Where(c => type is null || c.Type == type)
            .ToArray();

        if (candidates.Length == 0)
            throw new ArgumentException($"{scheme} {type} için test kartı yok.");

        var entry = finance.Random.ArrayElement(candidates);
        var cvvLength = entry.Scheme == CardScheme.Amex ? 4 : 3;
        var cvv = string.Concat(finance.Random.Digits(cvvLength));

        return new TrTestCard(
            entry.Number,
            entry.Scheme,
            entry.Type,
            entry.Issuer,
            expiryMonth: finance.Random.Int(1, 12),
            expiryYear: DateTime.UtcNow.Year + finance.Random.Int(1, 5),
            cvv);
    }

    /// <summary>Herkese açık bir test kartının yalnızca numarasını döndürür.</summary>
    public static string TrTestCardNumber(this Finance finance, CardScheme? scheme = null, CardType? type = null)
        => finance.TrTestCard(scheme, type).Number;

    /// <summary>Türkiye GSM numarası (5xx önekli, 10 hane).</summary>
    public static string TrGsm(this PhoneNumbers phone, TrPhoneFormat format = TrPhoneFormat.E164)
    {
        var prefix = phone.Random.ArrayElement(GsmPrefixes);
        var rest = string.Concat(phone.Random.Digits(7));

        return format switch
        {
            TrPhoneFormat.E164 => $"+90{prefix}{rest}",
            TrPhoneFormat.International => $"+90 {prefix} {rest.Substring(0, 3)} {rest.Substring(3, 2)} {rest.Substring(5, 2)}",
            TrPhoneFormat.National => $"0{prefix} {rest.Substring(0, 3)} {rest.Substring(3, 2)} {rest.Substring(5, 2)}",
            TrPhoneFormat.Digits => prefix + rest,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    internal static string Tckn(Randomizer random)
    {
        var first9 = new int[9];
        first9[0] = random.Int(1, 9);
        for (var i = 1; i < 9; i++) first9[i] = random.Int(0, 9);

        var (tenth, eleventh) = Checksums.TcknCheckDigits(first9);
        return string.Concat(first9) + tenth + eleventh;
    }

    private static string FormatIban(string iban)
    {
        var sb = new StringBuilder(iban.Length + iban.Length / 4);
        for (var i = 0; i < iban.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(' ');
            sb.Append(iban[i]);
        }

        return sb.ToString();
    }
}
