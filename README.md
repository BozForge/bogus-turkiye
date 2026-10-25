# BozForge.Bogus.Turkiye

[Bogus](https://github.com/bchavez/Bogus) için Türkiye eklentisi. Testlerinde ve demo verinde doğrulamadan geçen TCKN, VKN, TR IBAN, test kartı ve GSM numarası üretir.

*Turkish test data extensions for Bogus: valid TCKN (national ID), VKN (tax ID), TR IBAN with real bank codes, public test cards and mobile numbers.*

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
    .Generate(100);

var sirketVkn = new Faker().Company.Vkn();
var kart = new Faker().Finance.TrTestCard(CardScheme.Troy);
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

Bogus'un seed'iyle çalışır: aynı seed aynı veriyi üretir (`UseSeed`, `Randomizer.Seed`). Tek istisna, test kartının son kullanma yılı. Kartın geçerli kalması için bu yıl, bugünün yılına göre hesaplanır.

## Önemli: yalnızca test için

- **Üretilen TCKN, VKN ve IBAN'lar gerçek kişilere ya da hesaplara denk gelebilir.** Kontrol hanesi tutan numaralar sınırlı bir küme oluşturur ve TCKN için ayrılmış resmî bir test aralığı yok. Bu verileri production ortamına koyma; kimseyi tanımlamak ya da bir sisteme gerçek kişi gibi kaydetmek için kullanma.
- **Kart numaraları üretilmez.** Paket yalnızca ödeme kuruluşlarının sandbox ortamları için herkese açık yayımladığı test kartlarından seçer.
- Kontrol hanesi tutan bir TCKN, gerçek kimlik doğrulamasından (NVİ/KPS) geçmez. O doğrulama numarayla birlikte ad, soyad ve doğum yılını da kontrol eder.

## Kaynaklar

- Banka kodları: [TCMB FAST katılımcı listesi](https://www.tcmb.gov.tr/wps/wcm/connect/fast/katilimcilar) (mevduat ve katılım bankaları; 2026-10)
- Test kartları: [iyzico sandbox test kartları](https://docs.iyzico.com/ek-bilgiler/test-kartlari) (başarılı yanıt dönenler)
- TCKN ve VKN algoritmaları: [python-stdnum](https://github.com/arthurdejong/python-stdnum) (`stdnum.tr`) ile aynı sonucu verir
- Testler, üretilen değerleri bağımsız kütüphanelerle doğrular: [IbanNet](https://github.com/skwasjer/IbanNet), [TurkishId](https://github.com/ssg/TurkishId), [TurkishValidators](https://github.com/enesatacdev/TurkishValidators)

## Lisans

MIT
