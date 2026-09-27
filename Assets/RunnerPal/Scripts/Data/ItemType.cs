// RunnerPal - Toplanabilir kıyafet/eşya türleri
// Yeni eşya eklemek için buraya satır ekle (ör. Kravat, Canta).
public enum ItemType
{
    Gomlek,
    Pantolon,
    Ceket,
    Ayakkabi,
    Saat,      // 11. bölümden itibaren
    Telefon,   // 21. bölümden itibaren
    Laptop     // 31. bölümden itibaren
}

// Oyuncuya gösterilen Türkçe adlar (enum adları kodda ASCII kalır).
public static class ItemNames
{
    public static string Get(ItemType item) => item switch
    {
        ItemType.Gomlek => "Gömlek",
        ItemType.Pantolon => "Pantolon",
        ItemType.Ceket => "Ceket",
        ItemType.Ayakkabi => "Ayakkabı",
        ItemType.Saat => "Saat",
        ItemType.Telefon => "Telefon",
        ItemType.Laptop => "Laptop",
        _ => item.ToString(),
    };
}
