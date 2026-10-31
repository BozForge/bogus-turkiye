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

    private static readonly TrProvince[] Provinces = (TrProvince[])Enum.GetValues(typeof(TrProvince));

    // Plakada Türkçeye özgü harfler (Ç, Ğ, İ, Ö, Ş, Ü) ile Q, W ve X kullanılmaz.
    private const string PlateLetters = "ABCDEFGHIJKLMNOPRSTUVYZ";

    // Harf sayısı -> rakam grubunun alabileceği uzunluklar (harf + rakam en çok 6).
    private static readonly int[][] PlateDigitLengths = { new[] { 4, 5 }, new[] { 3, 4 }, new[] { 2, 3 } };

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

    /// <summary>
    /// Tutarlı bir Türkiye adresi: il, ilçe, mahalle ve posta kodu birbirine aittir.
    /// Önce il, sonra o ilin ilçelerinden biri, sonra o ilçenin mahallelerinden biri seçilir.
    /// </summary>
    /// <param name="address">Bogus adres veri seti.</param>
    /// <param name="province">İl; verilmezse rastgele seçilir.</param>
    public static TrAddress TrAddress(this Address address, TrProvince? province = null)
        => TrAddress(address.Random, province);

    /// <summary>
    /// Kişinin adresi. Aynı <see cref="Person"/> için her çağrıda aynı adresi döndürür;
    /// böylece il, ilçe ve posta kodu ayrı kurallarda kullanılsa da birbirini tutar.
    /// </summary>
    public static TrAddress TrAddress(this Person person)
    {
        const string key = nameof(ExtensionsForTurkiye) + "TrAddress";
        var context = ((IHasContext)person).Context;
        if (context.TryGetValue(key, out var cached)) return (TrAddress)cached;

        var address = TrAddress(person.Random, province: null);
        context[key] = address;
        return address;
    }

    /// <summary>
    /// Özel araç plakası: il kodu (01-81), 1-3 harf ve 2-5 rakam; ör. <c>34 ABC 123</c>.
    /// Resmî ve özel araçlara ayrılmış harf grupları (polis, kamu, taksi, diplomatik,
    /// geçici plaka vb.) üretilmez. Rakam grubu 0 ile başlamaz.
    /// </summary>
    /// <param name="vehicle">Bogus araç veri seti.</param>
    /// <param name="province">İl; verilmezse rastgele seçilir.</param>
    /// <param name="formatted"><c>true</c> ise gruplar boşlukla ayrılır (<c>34 ABC 123</c>), değilse bitişik yazılır (<c>34ABC123</c>).</param>
    public static string TrPlate(this Vehicle vehicle, TrProvince? province = null, bool formatted = true)
    {
        var random = vehicle.Random;
        var code = ((int)(province ?? random.ArrayElement(Provinces))).ToString("00");

        var letterCount = random.Int(1, 3);
        string letters;
        do
        {
            var chars = new char[letterCount];
            for (var i = 0; i < letterCount; i++) chars[i] = PlateLetters[random.Int(0, PlateLetters.Length - 1)];
            letters = new string(chars);
        } while (IsReservedPlateLetters(letters));

        var digitCount = random.ArrayElement(PlateDigitLengths[letterCount - 1]);
        var digits = random.Int(1, 9) + string.Concat(random.Digits(digitCount - 1));

        return formatted ? $"{code} {letters} {digits}" : code + letters + digits;
    }

    // Kaynak: Vehicle registration plates of Turkey (Wikipedia), özel seriler tablosu.
    internal static bool IsReservedPlateLetters(string letters) => letters switch
    {
        "A" or "AA" or "AAA" => true,           // polis, kamu
        "B" or "G" or "T" => true,              // uluslararası kuruluş, geçici, taksi
        "CC" or "CD" => true,                   // konsolosluk, diplomatik
        "JAA" or "SGH" or "GMR" => true,        // jandarma, sahil güvenlik, geçici gümrük
        { Length: 2 } when letters[0] == 'M' => true,   // MA-MZ: geçici ikamet eden yabancılar
        { Length: 3 } when letters[0] == 'S' => true,   // SAA-SZZ: geçici koruma
        { Length: 3 } when letters[0] == 'T' && letters[1] <= 'K' => true,  // TAA-TKZ: İstanbul taksi
        _ => false,
    };

    private static TrAddress TrAddress(Randomizer random, TrProvince? province)
    {
        var entry = AddressData.Get(province ?? random.ArrayElement(Provinces));
        var district = random.ListItem(entry.Districts);
        var neighborhood = random.ListItem(district.Neighborhoods);

        // Adres Türkiye'de olduğu için sokak adı, Faker'ın dili ne olursa olsun Türkçe.
        var street = new Address("tr") { Random = random }.StreetName();

        return new TrAddress(
            entry.Province,
            entry.Name,
            district.Name,
            neighborhood.Name,
            neighborhood.PostalCode,
            street,
            random.Int(1, 199).ToString());
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
