# RunnerPal – Unity Kurulum Rehberi

İşe geç kalan, don ve atletle evden fırlayan adamın 3 şeritli koşu oyunu.

## Oyun kuralları (kodda uygulananlar)
- 3 şerit; sağa/sola kaydır = şerit değiştir, yukarı = zıpla, aşağı = eğil (editörde ok tuşları / WASD).
- Hız bölüm boyunca artar (`LevelData.speedCurve`), her bölüm bir öncekinden hızlı başlar.
- 3 can. Engele çarpınca 1 can gider, 1.2 sn dokunulmazlık olur. Can biterse game over.
- **Kalkan**: süre boyunca çarpışmada can gitmez. **Hızlanma**: süreli hız artışı.
- **Yavaşlama tuzağı**: süreli hız düşüşü (süre sınırı yüzünden tehlikeli).
- **Süre sınırı**: süre dolmadan bitişe varılmazsa game over.
- **Bölüm sonu**:
  - 0 eksik → geçti
  - 1 eksik → bölüm altınının %50'si ödenir ve geçilir (altın 0 veya 1 ise başarısız)
  - 2+ eksik → game over
- Başarısız bölümde toplanan altın kasaya eklenmez. Başarılıysa mağazada harcanabilir.
- Her 10 bölümde iş, tema ve gereksinim değişir: 11+ Saat, 21+ Telefon, 31+ Laptop.

## Dosyalar
```
Scripts/
  Data/    ItemType, LevelData (bölüm ayarları), CharacterData (mağaza karakteri)
  Core/    GameManager (kurallar), SaveSystem (kayıt)
  Player/  PlayerController (hareket), PlayerOutfit (kıyafet görünümü), CameraFollow
  World/   TrackSpawner (yolu üretir), Pickup (altın/eşya/güçlendirici/tuzak), Obstacle, FinishLine
  UI/      GameUI (briefing + HUD + sonuç), ShopManager + ShopItemView (mağaza)
  Editor/  LevelGenerator (40 bölümü tek tıkla üretir)
```

## Kurulum adımları
1. Unity Hub > yeni proje > **Universal 3D** şablonu. Android modülü kurulu olsun.
2. `Scripts` klasörünü `Assets/RunnerPal/` altına kopyala.
3. **Player > Input Handling** ayarını *Both* yap (Edit > Project Settings > Player > Other Settings). Kod eski Input sistemini kullanıyor.
4. Üst menüden **RunnerPal > 40 Bölüm Oluştur**. `Assets/RunnerPal/Levels` altında 40 bölüm oluşur.

### Game sahnesi
5. `Game` adında sahne oluştur.
6. **Player** objesi:
   - Tag: `Player`
   - `CapsuleCollider` (Is Trigger kapalı), `Rigidbody` (kod kinematic yapar), `PlayerController`
   - İçine boş bir child `ModelRoot` → PlayerController'daki *Model Root* alanına sürükle
   - Model içine gömlek, pantolon, ceket, ayakkabı, saat, telefon, laptop objelerini koy. Modelin köküne `PlayerOutfit` ekle ve her eşyayı ilgili objeyle eşleştir. Başta hepsi gizlenir, adam don+atletle başlar.
7. **Main Camera**: `CameraFollow`, Target = Player.
8. **GameManager** objesi: `GameManager` ekle. *Levels* dizisine Level_01…Level_40'ı sırayla sürükle. Player ve Spawner alanlarını doldur.
9. **TrackSpawner** objesi: prefabları ata (aşağıya bak).
10. **Canvas**: `GameUI` ekle. Briefing, HUD, Win, Fail panellerini oluşturup alanlara bağla.
    - Win paneli butonu → `GameManager.NextLevel`, Fail paneli butonu → `GameManager.RestartLevel`, Menü butonları → `GameManager.GoToMenu`.

### Prefablar (başta küp/kapsül yeterli, sonra modelle değiştirirsin)
| Prefab | Bileşenler |
|---|---|
| Altın | Collider (Is Trigger), `Pickup` Kind=Gold |
| Gömlek, Pantolon… (her eşya) | Collider (Is Trigger), `Pickup` Kind=Item, Item=… |
| Hızlanma | `Pickup` Kind=SpeedBoost, Speed Multiplier=1.5 |
| Kalkan | `Pickup` Kind=Shield, Duration=5 |
| Yavaşlama tuzağı (ör. dökülmüş kahve) | `Pickup` Kind=SlowTrap, Speed Multiplier=0.5 |
| Engeller | Collider (Is Trigger), `Obstacle`. Alçak (zıpla), yüksek bariyer (eğil) |
| Bitiş (iş yeri kapısı) | Geniş collider (Is Trigger), `FinishLine` |
| Yol parçası | 20 m uzunluğunda, 3 şeritli zemin (her dünyaya ayrı set) |

Yol parçalarını ve skybox'ı her dünyanın 10 bölümüne atamak için Levels klasöründe 10 asset'i birlikte seçip Inspector'dan tek seferde doldurabilirsin.

### MainMenu sahnesi
11. `MainMenu` sahnesi: `ShopManager` ekle. Karakter kartı prefabının köküne `ShopItemView` koy.
12. İlk karakterin `id` değeri **default** ve fiyatı 0 olmalı.
13. Aynı karakter listesini PlayerController'ın *Characters* alanına da ver.
14. File > Build Profiles: sahneleri **MainMenu, Game** sırasıyla ekle.

## Animator parametreleri
`Running` (bool), `Speed` (float), `Jump`, `Slide`, `Hit` (trigger). Animator yoksa kod hata vermez.

## Sonraki adımlar
- Reklamlar (ödüllü reklamla "1 can daha" veya "eksiği bedava al" uygun olur)
- Ses ve efektler, obje havuzlama (performans), ayarlar menüsü
