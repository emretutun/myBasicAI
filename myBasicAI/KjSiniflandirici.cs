using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MyBasicAI;

public static class KjSiniflandirici
{
    // --- 81 il + sık kullanılan yurt dışı şehirler ---
    private static readonly HashSet<string> Sehirler = new(StringComparer.OrdinalIgnoreCase)
    {
        "Adana", "Adiyaman", "Afyonkarahisar", "Agri", "Amasya", "Ankara", "Antalya", "Artvin",
        "Aydin", "Balikesir", "Bilecik", "Bingol", "Bitlis", "Bolu", "Burdur", "Bursa", "Canakkale",
        "Cankiri", "Corum", "Denizli", "Diyarbakir", "Edirne", "Elazig", "Erzincan", "Erzurum",
        "Eskisehir", "Gaziantep", "Giresun", "Gumushane", "Hakkari", "Hatay", "Isparta", "Mersin",
        "Istanbul", "Izmir", "Kars", "Kastamonu", "Kayseri", "Kirklareli", "Kirsehir", "Kocaeli",
        "Konya", "Kutahya", "Malatya", "Manisa", "Kahramanmaras", "Mardin", "Mugla", "Mus",
        "Nevsehir", "Nigde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas",
        "Tekirdag", "Tokat", "Trabzon", "Tunceli", "Sanliurfa", "Usak", "Van", "Yozgat", "Zonguldak",
        "Aksaray", "Bayburt", "Karaman", "Kirikkale", "Batman", "Sirnak", "Bartin", "Ardahan",
        "Igdir", "Yalova", "Karabuk", "Kilis", "Osmaniye", "Duzce",
        // İlçe/semt adları (yer kuralında ikinci kelime olarak çok sık geçiyor, şehir gibi davranmalı)
        "Besiktas", "Kadikoy", "Uskudar", "Bakirkoy", "Maltepe", "Kartal", "Pendik", "Beyoglu",
        "Sisli", "Konak", "Mamak", "Cankaya", "Kecioren", "Yenimahalle", "Bornova", "Karsiyaka",
        "Nilufer", "Osmangazi", "Selcuklu", "Meram", "Seyhan", "Muratpasa", "Kepez",
        // Yurt dışı
        "Washington", "Moskova", "Paris", "Londra", "Berlin", "New York", "Tokyo", "Pekin",
        "Riyad", "Bagdat", "Tahran", "Dubai", "Atina", "Roma", "Madrid", "Viyana", "Bruxelles",
        "Sofya", "Belgrad", "Bukres", "Saraybosna", "Tbilisi", "Baku", "Erivan", "Tel Aviv",
        "Kahire", "Cenevre"
    };

    // Tek kelimelik "yer" sayılan kurumsal/jenerik lokasyonlar
    private static readonly HashSet<string> KurumYerleri = new(StringComparer.OrdinalIgnoreCase)
    {
        "Olay Yeri", "Canli", "TBMM", "Cumhurbaskanligi Kulliyesi", "Adliye", "Valilik",
        "Emniyet Mudurlugu", "Hastane", "Stadyum", "Havalimani", "Otogar", "Liman", "Gar",
        "Kopru", "Tunel", "Otoyol", "Fabrika", "Sanayi Bolgesi", "Universite Kampusu",
        "Okul", "Mahalle", "Ilce", "Merkez", "Sahil", "Baraj Golu", "Ormanlik Alan",
        "Belediye Binasi", "Meydan", "AVM", "Koy"
    };

    // Unvanlar — kural artık BUNLARDAN birini içerip içermemesine göre ISIMLIK kararı veriyor.
    // Liste ne kadar genişse o kadar isabetli olur; eksik unvan -> yanlış MUHABIR/KAMERAMAN sınıflandırmasına yol açar.
    private static readonly string[] UnvanlarListesi =
    {
        "Doktor", "Dr.", "Uzman Dr.", "Doc. Dr.", "Prof. Dr.", "Prof.", "Doc.",
        "Avukat", "Av.", "Hukuk Danismani",
        "Bakan", "Bakan Yardimcisi", "Cumhurbaskani", "Vali", "Kaymakam", "Muhtar",
        "Mahalle Muhtari", "Belediye Baskani", "Milletvekili", "Genel Mudur", "Genel Sekreter",
        "Mudur", "Komiser", "Savci", "Hakim",
        "Saglik Bakani", "Milli Egitim Bakani", "Icisleri Bakani", "Emniyet Genel Muduru",
        "Ekonomi Uzmani", "Meteoroloji Uzmani", "Uzman Psikolog", "Diyetisyen",
        "Tuketici Haklari Dernegi Baskani",
        "Muhendis", "Bilgisayar Muhendisi", "Mimar",
        "Oyuncu", "Sarkici", "Sanatci", "Yonetmen", "Yapimci", "Sunucu", "Spiker",
        "Spor Spikeri", "Kanal Yoneticisi", "Genel Yayin Yonetmeni", "Yapim Koordinatoru",
        "Teknik Direktor", "Kulup Baskani", "Futbolcu", "Antrenor", "Hakem",
        "Gorgu Tanigi", "Basin Sozcusu", "Asistan", "Danisman",
        "Muhabir", "Kameraman"
    };

    private static readonly HashSet<string> Unvanlar = new(UnvanlarListesi, StringComparer.OrdinalIgnoreCase);

    // Tek başına isim sayılmaması gereken çok genel/işlevsel kelimeler (bağlaç, zamir, sık sözlük kelimeleri).
    // Bunlar büyük harfle başlasa bile "isim" gibi değerlendirilmemeli.
    private static readonly HashSet<string> GenelKelimeler = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ve", "Ile", "Ya", "De", "Da", "Bu", "Su", "O", "Ben", "Sen", "Biz", "Siz", "Onlar",
        "Var", "Yok", "Icin", "Gibi", "Kadar", "Once", "Sonra", "Simdi", "Bugun", "Dun",
        "Yarin", "Flas", "Acil", "Kritik", "Gelisme", "Detaylar", "Canli", "Son", "Dakika",
        "Ozel", "Haber", "Final", "Devam", "Tamamlandi", "Onaylandi", "Bekleniyor"
    };

    private static readonly string[] FiilEkleri =
    [
        "ildi", "ıldı", "uldu", "üldü", "edildi", "etti", "yor", "ecek", "acak",
        "yapildi", "yapıldı", "yaptilar", "oldu", "oluyor", "basladi", "bitti", "geldi", "gitti",
        "aciklandi", "duyurdu", "yasandi", "yasadi", "cikti", "dustu", "yukseldi", "geriledi",
        "bekleniyor", "suruyor", "kapatildi", "kapatıldı", "kirildi", "kirdi", "surukledi",
        "kovuldu", "atandi", "secildi", "yapti", "saglandi", "sunuldu"
    ];

    // Haber metni olduğuna işaret eden anahtar kelime/kalıplar (konu başlıkları).
    private static readonly string[] HaberKaliplari =
    [
        "son dakika", "flas", "operasyon", "kaza", "yangin", "aciklama", "rekor", "karar",
        "meclis", "bakanlik", "polis", "savci", "mahkeme", "borsa", "enflasyon", "faiz",
        "secim", "parti", "kongre", "istifa", "tutuklama", "gozalti", "yarali", "olu",
        "deprem", "sel", "firtina", "uyari", "zam", "indirim", "paket", "tasarisi",
        "ozel haber", "gelisme", "detaylar", "kritik", "acil", "canli", "anayasa",
        "yargitay", "ihracat", "ithalat", "asgari ucret", "kripto", "siber", "veri ihlali"
    ];

    public static string Normalize(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return string.Empty;

        var trimmed = metin.Trim();
        var sb = new StringBuilder(trimmed.Length);

        foreach (var ch in trimmed.Normalize(NormalizationForm.FormD))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// İlk harfi büyütüp gerisini olduğu gibi bırakır (kullanıcı küçük harfle yazsa da
    /// isim/unvan tespiti çalışabilsin diye). Tüm metne TitleCase uygulamak yerine
    /// sadece kelime bazlı kontrol noktalarında kullanılır.
    /// </summary>
    private static string Kelime_BuyukHarfle(string kelime)
    {
        if (string.IsNullOrEmpty(kelime)) return kelime;
        if (char.IsUpper(kelime[0])) return kelime;
        return char.ToUpper(kelime[0], new CultureInfo("tr-TR")) + kelime.Substring(1);
    }

    public static string? TryRuleBased(string metinHam)
    {
        if (string.IsNullOrWhiteSpace(metinHam))
            return null;

        var text = Normalize(metinHam);
        var lower = text.ToLower(new CultureInfo("tr-TR"));

        // ============================================================
        // 1. KESİN KURALLAR: Haber/Kamera etiketli kalıplar
        // ============================================================
        if (Regex.IsMatch(text, @"\b(Haber|Muhabir|Kamera|Kameraman)\s*[:\-]", RegexOptions.IgnoreCase))
            return "MUHABIR KAMERAMAN";

        if (Regex.IsMatch(text, @"\bMuhabir\b", RegexOptions.IgnoreCase) &&
            Regex.IsMatch(text, @"\bKamera(man)?\b", RegexOptions.IgnoreCase))
            return "MUHABIR KAMERAMAN";

        // ============================================================
        // 2. YER (KJ YER) KONTROLLERİ — unvan kontrolünden ÖNCE çalışmalı,
        //    yoksa "X Mahallesi Y" gibi yapılar yanlışlıkla isim sanılabilir.
        // ============================================================
        if (lower.Contains(" mahallesi "))
            return "KJ YER";

        if (KurumYerleri.Contains(text))
            return "KJ YER";

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 1 && (Sehirler.Contains(words[0]) || KurumYerleri.Contains(words[0])))
            return "KJ YER";

        if (words.Length == 2 && Sehirler.Contains(words[0]) && Sehirler.Contains(words[1]))
            return "KJ YER";

        // ============================================================
        // 3. İSİMLİK (KJ ISIMLIK) KONTROLLERİ
        //    Unvan, metnin İÇİNDE herhangi bir yerde geçebilir (örn. "Emre Tutun Sunucu").
        //    "Muhabir" ve "Kamera(man)" kelimeleri BİRLİKTE geçtiğinde adım 1'de
        //    zaten MUHABIR KAMERAMAN olarak yakalanmış olur; ama "Kameraman" tek
        //    başına (örn. "Furkan Demir Kameraman") bir kişinin mesleğini belirten
        //    bir unvan olarak ISIMLIK sayılmalı.
        // ============================================================
        bool unvanIceriyor = Unvanlar
            .Where(u => !string.Equals(u, "Muhabir", StringComparison.OrdinalIgnoreCase))
            .Any(u => UnvanEslesiyorMu(text, u));

        if (unvanIceriyor && !HasFiilEki(lower) && !HasHaberKaliplari(lower))
            return "KJ ISIMLIK";

        // ============================================================
        // 4. MUHABİR KAMERAMAN — unvansız, 2-6 kelimelik çift/üçlü isim kalıbı
        //    (örn. "Cansu Canan Ozgen Emre Tutun" veya 3 kişi: "A B C D E F")
        // ============================================================
        if (words.Length >= 2 && words.Length <= 6 && AllNameLike(words) && !HasHaberKaliplari(lower))
            return "MUHABIR KAMERAMAN";

        // ============================================================
        // 5. YER — iki kelimeli "İlçe Şehir" kalıbı (örn. "Beyoglu Kayseri")
        //    Unvan/isim kontrolünden SONRA, çünkü "Ali Veli" gibi isim çiftleri
        //    şehir listesine yanlışlıkla girmemeli.
        // ============================================================
        if (words.Length == 2 && Sehirler.Contains(words[1]) &&
            !Unvanlar.Contains(words[0]) && !HasFiilEki(lower))
            return "KJ YER";

        // ============================================================
        // 6. HABER METNİ (KJ METIN)
        // ============================================================
        if (HasFiilEki(lower) || HasHaberKaliplari(lower))
            return "KJ METIN";

        if (words.Length >= 2 && !AllNameLike(words))
            return "KJ METIN";

        // Hiçbir kural net karar veremediyse -> ML modeline bırak.
        return null;
    }

    /// <summary>
    /// Bir unvanın metin içinde geçip geçmediğini kontrol eder.
    /// Regex \b (word boundary), nokta ile biten unvanlarda (örn. "Dr.", "Av.")
    /// güvenilir çalışmaz çünkü nokta zaten "kelime dışı" bir karakterdir ve
    /// \b orada oluşmaz. Bu yüzden nokta ile bitenler için sondaki boundary
    /// kontrolü atlanır, sadece baştaki harf sınırı aranır.
    /// </summary>
    private static bool UnvanEslesiyorMu(string text, string unvan)
    {
        var bas = @"(?<![A-Za-zÇĞİÖŞÜçğıöşü])";
        var son = unvan.EndsWith(".", StringComparison.Ordinal)
            ? ""
            : @"(?![A-Za-zÇĞİÖŞÜçğıöşü])";

        var pattern = bas + Regex.Escape(unvan) + son;
        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
    }

    private static bool HasFiilEki(string lower)
    {
        // Sondan eşleşme + kelime sınırı kontrolü: rastgele substring eşleşmesini önlemek için
        // her ek artık kelime SONUNDA aranıyor, metnin herhangi bir yerinde değil.
        var sonKelime = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";

        foreach (var ek in FiilEkleri)
        {
            if (lower.EndsWith(ek, StringComparison.Ordinal))
                return true;
            // Cümle içinde fiil olabilecek kelimeleri de yakala (örn. "... yapildi acil" gibi son kelime ek almasa da)
            if (sonKelime.EndsWith(ek, StringComparison.Ordinal))
                return true;
        }

        return Regex.IsMatch(lower, @"\w+(dı|di|du|dü|tı|ti|tu|tü|miş|mış|muş|müş)\b");
    }

    private static bool HasHaberKaliplari(string lower)
    {
        foreach (var k in HaberKaliplari)
        {
            if (k.Contains(' '))
            {
                // Çok kelimeli kalıp (örn. "son dakika") - normal substring araması güvenli.
                if (lower.Contains(k, StringComparison.Ordinal))
                    return true;
            }
            else
            {
                // Tek kelimelik kalıp - kelime sınırıyla ara, yoksa "sel" kelimesi
                // "Selin" gibi isimlerin içinde yanlışlıkla eşleşir.
                if (Regex.IsMatch(lower, $@"\b{Regex.Escape(k)}\b"))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Anlamsız klavye-mashing harf dizilerini (örn. "asdasdsad") eler.
    /// Gerçek Türkçe kelimeler/isimler en az bir ünlü içerir ve art arda
    /// 4'ten fazla ünsüz barındırmaz.
    /// </summary>
    private static bool GercekciKelimeMi(string kelime)
    {
        var lower = kelime.ToLower(new CultureInfo("tr-TR"));
        const string unluler = "aeiouıöüâî";

        if (!lower.Any(c => unluler.Contains(c)))
            return false;

        if (Regex.IsMatch(lower, $@"[^{unluler}\s]{{4,}}"))
            return false;

        return true;
    }

    /// <summary>
    /// Bir kelime grubunun "kişi adı" gibi göründüğünü kontrol eder.
    /// Artık sadece "unvan/şehir listesinde değil" demek yetmiyor; ayrıca
    /// genel/işlevsel kelime, anlamsız harf dizisi de olmamalı ve gerçekçi
    /// bir isim uzunluğunda olmalı.
    /// </summary>
    private static bool AllNameLike(string[] words)
    {
        var culture = new CultureInfo("tr-TR");
        return words.All(w =>
        {
            var kelime = Kelime_BuyukHarfle(w);
            return kelime.Length >= 2
                && kelime.Length <= 20
                && char.IsLetter(kelime[0])
                && !Unvanlar.Contains(kelime)
                && !Sehirler.Contains(kelime)
                && !GenelKelimeler.Contains(kelime)
                && !HasFiilEki(kelime.ToLower(culture))
                && !kelime.Any(char.IsDigit)
                && GercekciKelimeMi(kelime);
        });
    }

    public static YayinZekasi.ModelOutput Predict(string metin)
    {
        var normalized = Normalize(metin);
        var ruleLabel = TryRuleBased(normalized);

        var input = new YayinZekasi.ModelInput { Text = normalized };
        var mlResult = YayinZekasi.Predict(input);

        if (ruleLabel != null)
        {
            mlResult.PredictedLabel = ruleLabel;
        }

        return mlResult;
    }
}