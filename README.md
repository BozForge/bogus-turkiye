# BozForge.Bogus.Turkiye

[Bogus](https://github.com/bchavez/Bogus) için Türkiye eklentisi. Testlerinde ve demo verinde doğrulamadan geçen TCKN, VKN, TR IBAN, test kartı, GSM numarası ve araç plakası, ili, ilçesi, mahallesi ve posta kodu birbirini tutan adres üretir.

*Turkish test data extensions for Bogus: valid TCKN (national ID), VKN (tax ID), TR IBAN with real bank codes, public test cards, mobile numbers, licence plates and consistent addresses (province, district, neighbourhood and postal code that belong together).*

## Neden?

Bogus'un kendisi Türkiye için kimlik numarası üretmiyor. TR IBAN üretiyor ama gerçek bir Türk IBAN'ına benzemiyor:

| | Bogus `f.Finance.Iban(countryCode: "TR")` | `f.Finance.TrIban()` |
|---|---|---|
| Örnek | `TR94305142W67F96K4K3W73W13` | `TR420005902999068639145135` |
| Rezerv hanesi (10. hane) | Harf de olabilir | Her zaman `0` |
| Hesap numarası | Harf ve rakam karışık | Yalnızca rakam |
| Banka kodu | Rastgele 5 hane | TCMB listesindeki gerçek bankalar |
| IbanNet doğrulaması | 1.000 örnekten 184'ü geçersiz | Hepsi geçerli |

"24 rakam" kontrolü yapan bir doğrulayıcın varsa Bogus'un ürettiği IBAN'lar testini patlatır.

Adreste de durum benzer. Bogus'un `tr` dili il ve posta kodunu birbirinden bağımsız seçer, ilçe ve mahalle hiç üretmez:

| | Bogus `f.Address.City()` + `ZipCode()` | `f.Address.TrAddress()` |
|---|---|---|
| Örnek | `Karabük` / `35339` | `Koçyaka Mah. Atatürk Bulvarı No: 180, 42450 Altınekin/Konya` |
| Posta kodu | Rastgele 5 hane, başka ile ait olabilir (35 İzmir'in kodu) | Mahallenin PTT'deki gerçek kodu |
| İlçe ve mahalle | Yok | O ile ait gerçek ilçe ve mahalle |
| Plaka | Yok | `f.Vehicle.TrPlate()` → `33 FOT 27` |

## Kurulum

```bash
dotnet add package BozForge.Bogus.Turkiye
```

## Kullanım

```csharp
using Bogus;
using BozForge.Bogus.Turkiye;

var musteriler = new Faker<Musteri>("tr")
    .RuleFor(m => m.Ad, f => f.Person.FirstName)
    .RuleFor(m => m.Tckn, f => f.Person.Tckn())
    .RuleFor(m => m.Iban, f => f.Finance.TrIban())
    .RuleFor(m => m.Telefon, f => f.Phone.TrGsm(TrPhoneFormat.National))
    .RuleFor(m => m.Il, f => f.Person.TrAddress().ProvinceName)
    .RuleFor(m => m.PostaKodu, f => f.Person.TrAddress().PostalCode)
    .RuleFor(m => m.Adres, f => f.Person.TrAddress().ToString())
    .Generate(100);

var sirketVkn = new Faker().Company.Vkn();
var kart = new Faker().Finance.TrTestCard(CardScheme.Troy);
var plaka = new Faker().Vehicle.TrPlate(TrProvince.Istanbul);
var adres = new Faker().Address.TrAddress(TrProvince.Izmir);
```

| Metot | Çıktı | Not |
|---|---|---|
| `f.Person.Tckn()` | `17291716060` | Kontrol haneleri tutar. Aynı kişi için hep aynı numara. |
| `f.Company.Vkn()` | `4540536920` | 10 haneli vergi kimlik numarası. |
| `f.Finance.TrIban()` | `TR420005902999068639145135` | Rastgele banka. |
| `f.Finance.TrIban(TrBank.Ziraat, formatted: true)` | `TR31 0001 0015 7093 8268 0171 10` | Banka seçimi ve boşluklu biçim. |
| `f.Finance.TrTestCard()` | `TrTestCard` | Numara, kuruluş, tip, banka, son kullanma tarihi, CVV. |
| `f.Finance.TrTestCardNumber(CardScheme.Visa, CardType.Debit)` | `4766620000000001` | Yalnızca numara. |
| `f.Phone.TrGsm()` | `+905457599747` | `E164`, `International`, `National`, `Digits` biçimleri. |
| `f.Vehicle.TrPlate()` | `13 Y 90686` | Özel araç plakası. İl seçilebilir; `formatted: false` ile `13Y90686`. |
| `f.Address.TrAddress()` | `TrAddress` | İl, ilçe, mahalle, posta kodu, sokak, bina no. `ToString()` tek satır adres verir. |
| `f.Address.TrAddress(TrProvince.Izmir)` | `Korutürk Mah. Dar Sokak No: 127, 35330 Balçova/İzmir` | İl seçimi. |
| `f.Person.TrAddress()` | `TrAddress` | Aynı kişi için hep aynı adres; il, ilçe ve posta kodu ayrı kurallarda da birbirini tutar. |

Adreste il, ilçe, mahalle ve posta kodu PTT listesindeki gerçek bir kayıttan gelir. Sokak adı (Bogus'un Türkçe sokak adları) ve bina numarası rastgeledir. Önce il, sonra ilçe, sonra mahalle eşit olasılıkla seçilir; nüfusa göre ağırlık yoktur.

Plakada il kodu 01-81, harf grubu 1-3 harf (Ç, Ğ, İ, Ö, Ş, Ü, Q, W, X yok), rakam grubu harf sayısına göre 2-5 hanedir. Polis, kamu, taksi, diplomatik ve geçici plaka gibi resmî ve özel serilere ayrılmış harf grupları üretilmez.

Bogus'un seed'iyle çalışır: aynı seed aynı veriyi üretir (`UseSeed`, `Randomizer.Seed`). Tek istisna, test kartının son kullanma yılı. Kartın geçerli kalması için bu yıl, bugünün yılına göre hesaplanır.

## Önemli: yalnızca test için

- **Üretilen TCKN, VKN, IBAN ve plakalar gerçek kişilere, hesaplara ya da araçlara denk gelebilir.** Kontrol hanesi tutan numaralar sınırlı bir küme oluşturur ve TCKN için ayrılmış resmî bir test aralığı yok. Bu verileri production ortamına koyma; kimseyi tanımlamak ya da bir sisteme gerçek kişi gibi kaydetmek için kullanma.
- **Kart numaraları üretilmez.** Paket yalnızca ödeme kuruluşlarının sandbox ortamları için herkese açık yayımladığı test kartlarından seçer.
- Adresteki mahalle ve posta kodu gerçektir, sokak ve bina numarası uydurmadır; adres gerçek bir binayı göstermez ama bir kargo ya da adres doğrulama servisine gönderme.
- Kontrol hanesi tutan bir TCKN, gerçek kimlik doğrulamasından (NVİ/KPS) geçmez. O doğrulama numarayla birlikte ad, soyad ve doğum yılını da kontrol eder.

## Doğruluk nasıl sağlanıyor?

- **Bağımsız doğrulama:** Testler üretilen değerleri bu paketin kendi koduyla değil, başka kütüphanelerle doğrular: IBAN için [IbanNet](https://github.com/skwasjer/IbanNet), TCKN için [TurkishId](https://github.com/ssg/TurkishId) ve [TurkishValidators](https://github.com/enesatacdev/TurkishValidators), VKN, GSM, plaka ve posta kodu için TurkishValidators. TCKN, VKN, IBAN, plaka ve adres 2.000'er örnekle denenir.
- **Plaka biçimi:** TurkishValidators'ın plaka kontrolü gevşek olduğu için (Q, X ya da "ABC 1234" kabul eder) plakalar ayrıca harf/rakam kuralını birebir uygulayan bir düzenli ifadeyle de sınanır.
- **İki kaynağın karşılaştırması:** `TrProvince` il kodları (trafik kodları) ile PTT listesindeki il adları birebir eşleşir; listede 81 ilin 973 ilçesinin hepsi var ve her posta kodunun ilk iki hanesi kendi ilinin koduyla aynı.
- **Bilinen numaralar:** TCKN ve VKN kontrol haneleri [python-stdnum](https://github.com/arthurdejong/python-stdnum) (`stdnum.tr`) ile aynı sonucu verir; testlerdeki bilinen örnek numaralar oradan alınmıştır.

## Kaynaklar

- Banka kodları: [TCMB FAST katılımcı listesi](https://www.tcmb.gov.tr/wps/wcm/connect/fast/katilimcilar) (mevduat ve katılım bankaları; 2026-10)
- Test kartları: [iyzico sandbox test kartları](https://docs.iyzico.com/ek-bilgiler/test-kartlari) (başarılı yanıt dönenler)
- Adresler: PTT posta kodu listesi (postakodu.ptt.gov.tr), [muratgozel/turkey-neighbourhoods](https://github.com/muratgozel/turkey-neighbourhoods) üzerinden (MIT, 2024-01 verisi; 81 il, 973 ilçe, 73.300 mahalle ve köy). Lisans metni [THIRD-PARTY-NOTICES.md](https://github.com/BozForge/bogus-turkiye/blob/main/THIRD-PARTY-NOTICES.md) dosyasında. Veri `dotnet run tools/AdresVerisi.cs` ile yeniden üretilir.
- Plaka kuralları ve ayrılmış seriler: [Vehicle registration plates of Turkey](https://en.wikipedia.org/wiki/Vehicle_registration_plates_of_Turkey) (Karayolları Trafik Yönetmeliği'nde bu kurallar ek şekillerde yer alıyor)

## Lisans

MIT. Adres verisinin lisansı için [THIRD-PARTY-NOTICES.md](https://github.com/BozForge/bogus-turkiye/blob/main/THIRD-PARTY-NOTICES.md).
