using Bogus;
using TurkishValidators.Validators;

namespace BozForge.Bogus.Turkiye.Tests;

// Plaka TurkishValidators ile, posta kodu TurkishValidators ile doğrulanır. İl kodu ↔ il adı
// eşlemesi iki ayrı kaynağın karşılaştırılmasıyla sınanır: TrProvince (trafik kodları) ve
// gömülü PTT listesi.
public class PlateAndAddressTests
{
    private const int Samples = 2_000;

    private static Faker NewFaker(int seed = 2026) => new("tr") { Random = new Randomizer(seed) };

    // Trafik kodu sırasıyla iller; PTT listesindeki yazımla aynı olmalı.
    private static readonly string[] ProvinceNames =
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın",
        "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı",
        "Çorum", "Denizli", "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir",
        "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir",
        "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir", "Kocaeli", "Konya", "Kütahya", "Malatya",
        "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya",
        "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak",
        "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale", "Batman", "Şırnak",
        "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce",
    };

    // İl kodu 01-81; harf grubunda Ç, Ğ, İ, Ö, Ş, Ü, Q, W, X yok; 1 harf → 4-5 rakam,
    // 2 harf → 3-4 rakam, 3 harf → 2-3 rakam; rakam grubu 0 ile başlamaz.
    private const string PlatePattern =
        "^(0[1-9]|[1-7][0-9]|8[01]) ([A-PR-VYZ] [1-9][0-9]{3,4}|[A-PR-VYZ]{2} [1-9][0-9]{2,3}|[A-PR-VYZ]{3} [1-9][0-9]{1,2})$";

    [Fact]
    public void TrPlate_matches_the_private_plate_format_and_passes_independent_validator()
    {
        var f = NewFaker();
        for (var i = 0; i < Samples; i++)
        {
            var plate = f.Vehicle.TrPlate();

            Assert.Matches(PlatePattern, plate);
            Assert.True(PlakaValidator.IsValid(plate), $"TurkishValidators reddetti: {plate}");
            Assert.False(ExtensionsForTurkiye.IsReservedPlateLetters(plate.Split(' ')[1]), plate);
        }
    }

    [Fact]
    public void TrPlate_produces_every_letter_and_digit_layout()
    {
        var f = NewFaker();
        var layouts = Enumerable.Range(0, Samples)
            .Select(_ => f.Vehicle.TrPlate().Split(' '))
            .Select(p => $"{p[1].Length}+{p[2].Length}")
            .ToHashSet();

        Assert.Equal(new[] { "1+4", "1+5", "2+3", "2+4", "3+2", "3+3" }, layouts.Order());
    }

    [Theory]
    [InlineData(TrProvince.Adana, "01")]
    [InlineData(TrProvince.Ankara, "06")]
    [InlineData(TrProvince.Istanbul, "34")]
    [InlineData(TrProvince.Duzce, "81")]
    public void TrPlate_uses_the_requested_province_code(TrProvince province, string code)
    {
        var f = NewFaker();
        for (var i = 0; i < 50; i++)
            Assert.StartsWith(code + " ", f.Vehicle.TrPlate(province));
    }

    [Fact]
    public void TrPlate_unformatted_has_no_spaces()
    {
        var f = NewFaker();
        for (var i = 0; i < 200; i++)
        {
            var plate = f.Vehicle.TrPlate(formatted: false);

            Assert.Matches("^[0-9]{2}[A-Z]{1,3}[0-9]{2,5}$", plate);
            Assert.True(PlakaValidator.IsValid(plate), $"TurkishValidators reddetti: {plate}");
        }
    }

    [Theory]
    [InlineData("A", true)]
    [InlineData("AAA", true)]
    [InlineData("T", true)]
    [InlineData("CD", true)]
    [InlineData("MK", true)]
    [InlineData("SAB", true)]
    [InlineData("TAK", true)]
    [InlineData("TLA", false)]
    [InlineData("AB", false)]
    [InlineData("KLM", false)]
    public void Reserved_plate_letters_are_recognised(string letters, bool reserved)
    {
        Assert.Equal(reserved, ExtensionsForTurkiye.IsReservedPlateLetters(letters));
    }

    [Fact]
    public void TrPlate_is_repeatable_with_the_same_seed()
    {
        var f1 = NewFaker(7);
        var f2 = NewFaker(7);
        var first = Enumerable.Range(0, 20).Select(_ => f1.Vehicle.TrPlate()).ToList();
        var second = Enumerable.Range(0, 20).Select(_ => f2.Vehicle.TrPlate()).ToList();

        Assert.Equal(first, second);
        Assert.Equal(20, first.Distinct().Count());
    }

    [Fact]
    public void Address_data_province_names_match_traffic_codes()
    {
        Assert.Equal(81, AddressData.All.Length);
        for (var i = 0; i < 81; i++)
        {
            var entry = AddressData.All[i];
            Assert.Equal((TrProvince)(i + 1), entry.Province);
            Assert.Equal(ProvinceNames[i], entry.Name);
        }
    }

    [Fact]
    public void Address_data_covers_every_district()
    {
        // Türkiye'de 973 ilçe var (30 büyükşehirde 519, diğer 51 ilde merkez dahil 454).
        Assert.Equal(973, AddressData.All.Sum(p => p.Districts.Count));
        Assert.All(AddressData.All.SelectMany(p => p.Districts), d => Assert.NotEmpty(d.Neighborhoods));
        Assert.True(AddressData.All.Sum(p => p.Districts.Sum(d => d.Neighborhoods.Count)) > 70_000);
    }

    [Fact]
    public void Every_postal_code_belongs_to_its_province()
    {
        foreach (var province in AddressData.All)
        foreach (var district in province.Districts)
        foreach (var postalCode in district.Neighborhoods.Select(n => n.PostalCode).Distinct())
        {
            Assert.Matches("^[0-9]{5}$", postalCode);
            Assert.Equal(((int)province.Province).ToString("00"), postalCode.Substring(0, 2));
            Assert.True(PostaKoduValidator.IsValid(postalCode), $"TurkishValidators reddetti: {postalCode}");
        }
    }

    [Fact]
    public void TrAddress_parts_belong_together()
    {
        var f = NewFaker();
        var known = AddressData.All
            .SelectMany(p => p.Districts.SelectMany(d => d.Neighborhoods.Select(n => (p.Name, District: d.Name, n.Name, n.PostalCode))))
            .ToHashSet();

        for (var i = 0; i < Samples; i++)
        {
            var a = f.Address.TrAddress();

            Assert.Contains((a.ProvinceName, a.District, a.Neighborhood, a.PostalCode), known);
            Assert.Equal(ProvinceNames[(int)a.Province - 1], a.ProvinceName);
            Assert.Equal(((int)a.Province).ToString("00"), a.PostalCode.Substring(0, 2));
            Assert.False(string.IsNullOrWhiteSpace(a.Street));
            Assert.InRange(int.Parse(a.BuildingNumber), 1, 199);
        }
    }

    [Fact]
    public void TrAddress_reaches_every_province()
    {
        var f = NewFaker();
        var provinces = Enumerable.Range(0, Samples).Select(_ => f.Address.TrAddress().Province).ToHashSet();
        Assert.Equal(81, provinces.Count);
    }

    [Theory]
    [InlineData(TrProvince.Istanbul)]
    [InlineData(TrProvince.Igdir)]
    [InlineData(TrProvince.Sanliurfa)]
    public void TrAddress_uses_the_requested_province(TrProvince province)
    {
        var f = NewFaker();
        for (var i = 0; i < 100; i++)
        {
            var a = f.Address.TrAddress(province);
            Assert.Equal(province, a.Province);
            Assert.StartsWith(((int)province).ToString("00"), a.PostalCode);
        }
    }

    [Fact]
    public void TrAddress_streets_are_turkish_even_with_an_english_faker()
    {
        var f = new Faker("en") { Random = new Randomizer(1) };
        var streets = Enumerable.Range(0, 50).Select(_ => f.Address.TrAddress().Street).ToList();
        Assert.Contains(streets, s => s.EndsWith(" Sokak") || s.EndsWith(" Caddesi"));
    }

    [Fact]
    public void TrAddress_to_string_is_a_single_line()
    {
        var a = NewFaker().Address.TrAddress();
        Assert.Equal($"{a.Neighborhood} {a.Street} No: {a.BuildingNumber}, {a.PostalCode} {a.District}/{a.ProvinceName}", a.ToString());
    }

    [Fact]
    public void Person_TrAddress_is_stable_for_the_same_person()
    {
        var person = NewFaker().Person;
        Assert.Same(person.TrAddress(), person.TrAddress());
    }

    [Fact]
    public void Person_TrAddress_keeps_separate_rules_consistent_and_repeatable()
    {
        Faker<Musteri> NewMusteri() => new Faker<Musteri>("tr")
            .UseSeed(11)
            .RuleFor(m => m.Il, f => f.Person.TrAddress().ProvinceName)
            .RuleFor(m => m.Ilce, f => f.Person.TrAddress().District)
            .RuleFor(m => m.PostaKodu, f => f.Person.TrAddress().PostalCode)
            .RuleFor(m => m.Adres, f => f.Person.TrAddress().ToString());

        var first = NewMusteri().Generate(50);
        var second = NewMusteri().Generate(50);

        Assert.All(first, m => Assert.EndsWith($", {m.PostaKodu} {m.Ilce}/{m.Il}", m.Adres));
        Assert.Equal(first.Select(m => m.Adres), second.Select(m => m.Adres));
        Assert.True(first.Select(m => m.Adres).Distinct().Count() > 45);
    }

    private sealed class Musteri
    {
        public string Il { get; set; } = "";
        public string Ilce { get; set; } = "";
        public string PostaKodu { get; set; } = "";
        public string Adres { get; set; } = "";
    }
}
