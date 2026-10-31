using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BozForge.Bogus.Turkiye;

/// <summary>
/// Türkiye adresi. İl, ilçe, mahalle ve posta kodu PTT'nin posta kodu listesinde birlikte
/// yer alan gerçek bir kayıttan gelir; sokak adı ve bina numarası rastgeledir.
/// </summary>
public sealed class TrAddress
{
    internal TrAddress(TrProvince province, string provinceName, string district, string neighborhood, string postalCode, string street, string buildingNumber)
    {
        Province = province;
        ProvinceName = provinceName;
        District = district;
        Neighborhood = neighborhood;
        PostalCode = postalCode;
        Street = street;
        BuildingNumber = buildingNumber;
    }

    /// <summary>İl.</summary>
    public TrProvince Province { get; }

    /// <summary>İlin adı, ör. <c>İstanbul</c>.</summary>
    public string ProvinceName { get; }

    /// <summary>İlçe, ör. <c>Kadıköy</c>. İl merkezindeki ilçeler PTT listesindeki gibi <c>Merkez</c> olabilir.</summary>
    public string District { get; }

    /// <summary>Mahalle ya da köy, PTT listesindeki yazımıyla, ör. <c>Caferağa Mah.</c></summary>
    public string Neighborhood { get; }

    /// <summary>Mahallenin 5 haneli posta kodu. İlk iki hane il kodudur.</summary>
    public string PostalCode { get; }

    /// <summary>Sokak ya da cadde adı, ör. <c>Okul Sokak</c>. Rastgele üretilir.</summary>
    public string Street { get; }

    /// <summary>Bina numarası. Rastgele üretilir.</summary>
    public string BuildingNumber { get; }

    /// <summary>Tek satır adres: <c>Caferağa Mah. Okul Sokak No: 12, 34710 Kadıköy/İstanbul</c></summary>
    public override string ToString()
        => $"{Neighborhood} {Street} No: {BuildingNumber}, {PostalCode} {District}/{ProvinceName}";
}

/// <summary>Gömülü <c>Data/adresler.txt</c> dosyasındaki il/ilçe/mahalle/posta kodu kayıtları.</summary>
internal static class AddressData
{
    private static readonly Lazy<ProvinceEntry[]> Provinces = new(Load);

    /// <summary>İl koduna göre sıralı 81 il.</summary>
    public static ProvinceEntry[] All => Provinces.Value;

    public static ProvinceEntry Get(TrProvince province) => All[(int)province - 1];

    private static ProvinceEntry[] Load()
    {
        var provinces = new List<ProvinceEntry>(81);
        List<DistrictEntry>? districts = null;
        List<NeighborhoodEntry>? neighborhoods = null;

        using var stream = typeof(AddressData).Assembly.GetManifestResourceStream("BozForge.Bogus.Turkiye.adresler.txt")
            ?? throw new InvalidOperationException("Adres verisi pakette bulunamadı.");
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Satırlar sekmeyle girintili: il, onun altında ilçe, onun altında posta kodu + mahalleler.
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length == 0 || line[0] == '#') continue;

            if (line.StartsWith("\t\t", StringComparison.Ordinal))
            {
                var parts = line.Substring(2).Split('\t');
                foreach (var name in parts[1].Split('|'))
                    neighborhoods!.Add(new NeighborhoodEntry(name, parts[0]));
            }
            else if (line[0] == '\t')
            {
                neighborhoods = new List<NeighborhoodEntry>();
                districts!.Add(new DistrictEntry(line.Substring(1), neighborhoods));
            }
            else
            {
                var parts = line.Split('\t');
                districts = new List<DistrictEntry>();
                provinces.Add(new ProvinceEntry((TrProvince)int.Parse(parts[0]), parts[1], districts));
            }
        }

        return provinces.ToArray();
    }
}

internal sealed class ProvinceEntry
{
    public ProvinceEntry(TrProvince province, string name, IList<DistrictEntry> districts)
    {
        Province = province;
        Name = name;
        Districts = districts;
    }

    public TrProvince Province { get; }
    public string Name { get; }
    public IList<DistrictEntry> Districts { get; }
}

internal sealed class DistrictEntry
{
    public DistrictEntry(string name, IList<NeighborhoodEntry> neighborhoods)
    {
        Name = name;
        Neighborhoods = neighborhoods;
    }

    public string Name { get; }
    public IList<NeighborhoodEntry> Neighborhoods { get; }
}

internal sealed class NeighborhoodEntry
{
    public NeighborhoodEntry(string name, string postalCode)
    {
        Name = name;
        PostalCode = postalCode;
    }

    public string Name { get; }
    public string PostalCode { get; }
}
