namespace BozForge.Bogus.Turkiye;

/// <summary>
/// Türkiye'deki mevduat ve katılım bankaları. Değer, bankanın TCMB'deki katılımcı
/// kodudur; IBAN'ın 5-9. hanelerinde (sıfırla doldurulmuş 5 hane) yer alır.
/// Kaynak: TCMB FAST katılımcı listesi (2026-10).
/// </summary>
/// <remarks>
/// Yatırım ve kalkınma bankaları ile elektronik para kuruluşları dahil değildir.
/// </remarks>
public enum TrBank
{
    /// <summary>T.C. Ziraat Bankası A.Ş.</summary>
    Ziraat = 10,
    /// <summary>Türkiye Halk Bankası A.Ş.</summary>
    Halkbank = 12,
    /// <summary>Türkiye Vakıflar Bankası T.A.O.</summary>
    Vakifbank = 15,
    /// <summary>Türk Ekonomi Bankası A.Ş.</summary>
    Teb = 32,
    /// <summary>Akbank T.A.Ş.</summary>
    Akbank = 46,
    /// <summary>Şekerbank T.A.Ş.</summary>
    Sekerbank = 59,
    /// <summary>Türk Ticaret Bankası A.Ş.</summary>
    TurkTicaretBankasi = 60,
    /// <summary>Türkiye Garanti Bankası A.Ş.</summary>
    Garanti = 62,
    /// <summary>Türkiye İş Bankası A.Ş.</summary>
    IsBankasi = 64,
    /// <summary>Yapı ve Kredi Bankası A.Ş.</summary>
    YapiKredi = 67,
    /// <summary>ING Bank A.Ş.</summary>
    Ing = 99,
    /// <summary>Fibabanka A.Ş.</summary>
    Fibabanka = 103,
    /// <summary>ICBC Turkey Bank A.Ş.</summary>
    Icbc = 109,
    /// <summary>QNB Bank A.Ş.</summary>
    Qnb = 111,
    /// <summary>HSBC Bank A.Ş.</summary>
    Hsbc = 123,
    /// <summary>Alternatifbank A.Ş.</summary>
    Alternatifbank = 124,
    /// <summary>Burgan Bank A.Ş.</summary>
    BurganBank = 125,
    /// <summary>Denizbank A.Ş.</summary>
    Denizbank = 134,
    /// <summary>Anadolubank A.Ş.</summary>
    Anadolubank = 135,
    /// <summary>Odea Bank A.Ş.</summary>
    Odeabank = 146,
    /// <summary>Enpara Bank A.Ş.</summary>
    Enpara = 157,
    /// <summary>Colendi Bank A.Ş.</summary>
    Colendi = 158,
    /// <summary>FUPS Bank A.Ş.</summary>
    Fups = 159,
    /// <summary>Ziraat Dinamik Banka A.Ş.</summary>
    ZiraatDinamik = 160,
    /// <summary>Albaraka Türk Katılım Bankası A.Ş.</summary>
    AlbarakaTurk = 203,
    /// <summary>Kuveyt Türk Katılım Bankası A.Ş.</summary>
    KuveytTurk = 205,
    /// <summary>Türkiye Finans Katılım Bankası A.Ş.</summary>
    TurkiyeFinans = 206,
    /// <summary>Ziraat Katılım Bankası A.Ş.</summary>
    ZiraatKatilim = 209,
    /// <summary>Vakıf Katılım Bankası A.Ş.</summary>
    VakifKatilim = 210,
    /// <summary>Türkiye Emlak Katılım Bankası A.Ş.</summary>
    EmlakKatilim = 211,
    /// <summary>Hayat Finans Katılım Bankası A.Ş.</summary>
    HayatFinans = 212,
    /// <summary>T.O.M. Katılım Bankası A.Ş.</summary>
    TomKatilim = 213,
    /// <summary>Dünya Katılım Bankası A.Ş.</summary>
    DunyaKatilim = 214,
}
