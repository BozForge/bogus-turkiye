using System.Collections.Generic;

namespace BozForge.Bogus.Turkiye;

/// <summary>Kart kuruluşu.</summary>
public enum CardScheme
{
    /// <summary>Visa</summary>
    Visa,
    /// <summary>Mastercard</summary>
    Mastercard,
    /// <summary>American Express</summary>
    Amex,
    /// <summary>Troy</summary>
    Troy,
}

/// <summary>Kart tipi.</summary>
public enum CardType
{
    /// <summary>Kredi kartı.</summary>
    Credit,
    /// <summary>Banka kartı.</summary>
    Debit,
}

/// <summary>
/// Herkese açık bir test kartı. Numaralar ödeme kuruluşlarının sandbox ortamları için
/// yayımladığı test kartlarıdır; gerçek bir kart numarası üretilmez.
/// </summary>
public sealed class TrTestCard
{
    internal TrTestCard(string number, CardScheme scheme, CardType type, string issuer, int expiryMonth, int expiryYear, string cvv)
    {
        Number = number;
        Scheme = scheme;
        Type = type;
        Issuer = issuer;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Cvv = cvv;
    }

    /// <summary>Boşluksuz kart numarası.</summary>
    public string Number { get; }

    /// <summary>Kart kuruluşu.</summary>
    public CardScheme Scheme { get; }

    /// <summary>Kredi ya da banka kartı.</summary>
    public CardType Type { get; }

    /// <summary>Test listesinde kartın bağlı olduğu banka.</summary>
    public string Issuer { get; }

    /// <summary>Son kullanma ayı (1-12).</summary>
    public int ExpiryMonth { get; }

    /// <summary>Son kullanma yılı (dört hane, ileri bir tarih).</summary>
    public int ExpiryYear { get; }

    /// <summary>Güvenlik kodu; Amex için 4, diğerleri için 3 hane.</summary>
    public string Cvv { get; }

    /// <inheritdoc />
    public override string ToString() => Number;
}

internal static class TestCardData
{
    internal readonly struct Entry
    {
        public Entry(string number, CardScheme scheme, CardType type, string issuer)
        {
            Number = number;
            Scheme = scheme;
            Type = type;
            Issuer = issuer;
        }

        public string Number { get; }
        public CardScheme Scheme { get; }
        public CardType Type { get; }
        public string Issuer { get; }
    }

    // Kaynak: iyzico sandbox test kartları, başarılı yanıt dönenler
    // (https://docs.iyzico.com/ek-bilgiler/test-kartlari, 2026-10).
    public static readonly IReadOnlyList<Entry> All = new[]
    {
        new Entry("5890040000000016", CardScheme.Mastercard, CardType.Debit, "Akbank"),
        new Entry("5526080000000006", CardScheme.Mastercard, CardType.Credit, "Akbank"),
        new Entry("9792072000017956", CardScheme.Troy, CardType.Credit, "Akbank"),
        new Entry("4766620000000001", CardScheme.Visa, CardType.Debit, "Denizbank"),
        new Entry("4603450000000000", CardScheme.Visa, CardType.Credit, "Denizbank"),
        new Entry("9792023757123604", CardScheme.Troy, CardType.Debit, "QNB"),
        new Entry("4987490000000002", CardScheme.Visa, CardType.Debit, "QNB"),
        new Entry("5311570000000005", CardScheme.Mastercard, CardType.Credit, "QNB"),
        new Entry("9792020000000001", CardScheme.Troy, CardType.Debit, "QNB"),
        new Entry("9792030000000000", CardScheme.Troy, CardType.Credit, "QNB"),
        new Entry("5170410000000004", CardScheme.Mastercard, CardType.Debit, "Garanti BBVA"),
        new Entry("5400360000000003", CardScheme.Mastercard, CardType.Credit, "Garanti BBVA"),
        new Entry("374427000000003", CardScheme.Amex, CardType.Credit, "Garanti BBVA"),
        new Entry("4475050000000003", CardScheme.Visa, CardType.Debit, "Halkbank"),
        new Entry("5528790000000008", CardScheme.Mastercard, CardType.Credit, "Halkbank"),
        new Entry("4059030000000009", CardScheme.Visa, CardType.Debit, "HSBC"),
        new Entry("5504720000000003", CardScheme.Mastercard, CardType.Credit, "HSBC"),
        new Entry("5892830000000000", CardScheme.Mastercard, CardType.Debit, "İş Bankası"),
        new Entry("4543590000000006", CardScheme.Visa, CardType.Credit, "İş Bankası"),
        new Entry("4910050000000006", CardScheme.Visa, CardType.Debit, "Vakıfbank"),
        new Entry("4157920000000002", CardScheme.Visa, CardType.Credit, "Vakıfbank"),
        new Entry("6500528865390837", CardScheme.Troy, CardType.Debit, "Vakıfbank"),
        new Entry("6501700194147183", CardScheme.Troy, CardType.Credit, "Vakıfbank"),
        new Entry("5168880000000002", CardScheme.Mastercard, CardType.Debit, "Yapı Kredi"),
        new Entry("5451030000000000", CardScheme.Mastercard, CardType.Credit, "Yapı Kredi"),
    };
}
