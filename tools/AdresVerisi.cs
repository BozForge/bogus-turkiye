// Adres veri dosyasını (src/BozForge.Bogus.Turkiye/Data/adresler.txt) yeniden üretir.
// Çalıştırma: dotnet run tools/AdresVerisi.cs
//
// Kaynak: muratgozel/turkey-neighbourhoods (MIT), PTT'nin posta kodu listesinden
// (postakodu.ptt.gov.tr, pk_list) derlenmiş il/ilçe/mahalle/posta kodu kayıtları.
// Veriyi güncellemek için Commit'i yeni bir sürüme çevirip aracı yeniden çalıştır.
#:property PublishAot=false
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string Commit = "6deccd073ed2f4e8cfe895fe0e5c4a5b1429ee92";
const string Url = $"https://raw.githubusercontent.com/muratgozel/turkey-neighbourhoods/{Commit}/src/data/neighbourhoods.json";

var tr = CultureInfo.GetCultureInfo("tr-TR");
var output = Path.Combine(AppContext.GetData("EntryPointFileDirectoryPath") as string ?? ".",
    "..", "src", "BozForge.Bogus.Turkiye", "Data", "adresler.txt");

using var http = new HttpClient();
var records = JsonSerializer.Deserialize<string[][]>(await http.GetStringAsync(Url))!;

// [il kodu, il, ilçe, mahalle, posta kodu]
var provinces = records
    .GroupBy(r => (Code: r[0], Name: r[1]))
    .OrderBy(g => g.Key.Code, StringComparer.Ordinal);

var sb = new StringBuilder();
sb.Append("# Biçim: \"il kodu<TAB>il\", altında \"<TAB>ilçe\", altında \"<TAB><TAB>posta kodu<TAB>mahalle|mahalle|...\"\n");
sb.Append($"# Kaynak: PTT posta kodu listesi, muratgozel/turkey-neighbourhoods@{Commit[..7]} (MIT) üzerinden.\n");
sb.Append("# Elle düzenleme yapma; tools/AdresVerisi.cs ile yeniden üret.\n");

var count = 0;
foreach (var province in provinces)
{
    sb.Append(province.Key.Code).Append('\t').Append(province.Key.Name).Append('\n');
    foreach (var district in province.GroupBy(r => r[2]).OrderBy(g => g.Key, StringComparer.Ordinal))
    {
        sb.Append('\t').Append(district.Key).Append('\n');
        foreach (var pk in district.GroupBy(r => r[4]).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var names = pk.Select(r => Clean(r[3])).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToArray();
            count += names.Length;
            sb.Append("\t\t").Append(pk.Key).Append('\t').Append(string.Join("|", names)).Append('\n');
        }
    }
}

File.WriteAllText(output, sb.ToString(), new UTF8Encoding(false));
Console.WriteLine($"{count} mahalle yazıldı: {Path.GetFullPath(output)}");

// PTT adları büyük harfli; kaynak paket bunları kelime kelime büyük harfe çeviriyor ama
// parantezden sonraki harfi küçük bırakıyor ("Bekir Mah (aşağıdere Köyü)"). "Mah"
// kısaltmasına nokta eklenir, fazla boşluklar atılır.
string Clean(string name)
{
    name = Regex.Replace(name.Trim(), @"\s+", " ");
    name = Regex.Replace(name, @"\((\p{Ll})", m => "(" + m.Groups[1].Value.ToUpper(tr));
    return Regex.Replace(name, @"\bMah\b(?!\.)", "Mah.");
}
