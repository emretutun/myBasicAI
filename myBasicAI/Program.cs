using System;
using System.Linq;
using MyBasicAI;

// --- TEST MODU BAŞLANGICI ---
if (args.Length > 0 && args[0] == "--test")
{
    Console.WriteLine("=== YAYIN GRAFİĞİ (KJ) TEST SÜRECİ BAŞLIYOR ===");
    Console.WriteLine("--------------------------------------------------");

    var testCases = new (string Input, string Expected)[]
    {
        ("AKP Kapatıldı", "KJ METIN"),
        ("akp kapatıldı yerine ahır yaptılar", "KJ METIN"),
        ("Emre Tütün Doktor", "KJ ISIMLIK"),
        ("asdasdsadsad asdasdsadaskgjasga asdasfakgsa", "KJ METIN"),
        ("Ankara", "KJ YER"),
        ("Haber: Emre Tutun Kamera: Caner Tas", "MUHABIR KAMERAMAN"),
        ("Prof. Dr. Ilber Ortayli", "KJ ISIMLIK"),
        ("altin fiyatlari rekor kirdi", "KJ METIN"),
        ("Emre Tutun Caner Tas", "MUHABIR KAMERAMAN"),
        ("Merkez Mahallesi Ankara", "KJ YER"),
        ("Cumhurbaşkanı Yurt Dışı Temaslarına Devam Ediyor", "KJ METIN"), // Yeni veri setinden test
        ("Muhabir - Selin Acar / Kamera - Cengiz Under", "MUHABIR KAMERAMAN") // Yeni varyasyon testi
    };

    int passed = 0;

    foreach (var (input, expected) in testCases)
    {
        // 1. Önce metni normalize et (Temizle)
        string temizMetin = KjSiniflandirici.Normalize(input);

        // 2. Kural tabanlı (Rule-Based) motoru kontrol et
        string? ruleBasedResult = KjSiniflandirici.TryRuleBased(temizMetin);

        // 3. Makine Öğrenmesi (ML) tahminini al
        var mlResult = KjSiniflandirici.Predict(temizMetin);

        // 4. Nihai kararı ver (Kural varsa kuralı ez, yoksa ML'i kullan)
        string finalPrediction = ruleBasedResult ?? mlResult.PredictedLabel;

        bool isSuccess = finalPrediction == expected;
        if (isSuccess) passed++;

        // Çıktıyı renklendir
        Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write($"[{(isSuccess ? "BAŞARILI" : "HATALI")}] ");

        Console.ResetColor();
        Console.WriteLine($"Beklenen: {expected,-20} | Tahmin: {finalPrediction,-20} | Kural: {ruleBasedResult ?? "ML Motoru",-10}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"      -> Giriş: '{input}'");
        Console.ResetColor();
    }

    Console.WriteLine("--------------------------------------------------");
    double successRate = (100.0 * passed) / testCases.Length;
    Console.ForegroundColor = successRate >= 85 ? ConsoleColor.Green : ConsoleColor.Yellow;
    Console.WriteLine($"TEST SONUCU: {passed}/{testCases.Length} Başarı (Oran: %{successRate:F1})");
    Console.ResetColor();
    return;
}

// --- İNTERAKTİF CANLI TEST MODU ---
Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=================================================");
Console.WriteLine("   AI TABANLI KJ OTOMATİK SINIFLANDIRMA SİSTEMİ  ");
Console.WriteLine("=================================================");
Console.ResetColor();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("\nCanlı KJ Metni Giriniz (Çıkmak için 'q'): ");
    Console.ResetColor();

    string? girilenMetin = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(girilenMetin)) continue;
    if (girilenMetin.ToLower() == "q") break;

    // Ön işleme
    string temizMetin = KjSiniflandirici.Normalize(girilenMetin);
    var kuralEtiketi = KjSiniflandirici.TryRuleBased(temizMetin);

    var inputModel = new YayinZekasi.ModelInput { Text = temizMetin };

    Console.WriteLine("\n--- Analiz Raporu ---");
    Console.WriteLine($"[Orijinal Metin] : {girilenMetin}");
    Console.WriteLine($"[Temiz Metin]    : {temizMetin}");

    if (kuralEtiketi != null)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Sistem Kararı]  : {kuralEtiketi} (Kural Tabanlı Hızlı Atama)");
        Console.ResetColor();
    }
    else
    {
        // Kurala takılmadıysa ML'i devreye sok
        var mlSonuc = YayinZekasi.Predict(inputModel);
        var tumSonuclar = YayinZekasi.PredictAllLabels(inputModel);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[Yapay Zeka]     : {mlSonuc.PredictedLabel}");
        Console.ResetColor();

        Console.WriteLine("\n-- Etiket Güven Oranları --");
        foreach (var item in tumSonuclar.OrderByDescending(x => x.Value))
        {
            // %80 üzeri güvenilirlik varsa yeşil, %50-%80 arası sarı, altı kırmızı yazdır
            if (item.Value >= 0.80f) Console.ForegroundColor = ConsoleColor.Green;
            else if (item.Value >= 0.50f) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine($"{item.Key,-20} -> %{(item.Value * 100):F2}");
            Console.ResetColor();
        }
    }
    Console.WriteLine("---------------------");
}