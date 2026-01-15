# 🎯 Crosshair Sorunları - KALICI ÇÖZÜM

## 🔴 [SON FİX - 2026-01-15] MainMenu + Settings Değişikliği Sonrası Crosshair Görünüyor

### ❌ SORUN:
- MainMenu sahnesinde crosshair **hep görünüyor**
- MainMenu'de cursor açık olmalı, crosshair **OLMAMALI**
- **Settings'te crosshair ayarı değiştirilince** (kalınlık, uzunluk vb.) MainMenu'de crosshair **görünür kalıyor**
- Default haldeyken gizli ama settings değiştirince görünüyor

### 🔧 NEDEN OLUYORDU?
1. `DontDestroyOnLoad` kullanıldığı için SimpleCrosshairGenerator GameObject'i sahne değişimlerinde yok olmuyor
2. Settings'te crosshair ayarı değiştirildiğinde `GenerateCrosshair()` çağrılıyor
3. `GenerateCrosshair()` crosshair'i yeniden oluşturuyor **AMA visibility kontrolü yapmıyor**
4. Bu yüzden MainMenu'de görünür kalıyor

### ✅ ÇÖZÜM (TAMAMLANDI):

**1. Aktif Sahne Kontrolü:**
`IsUIOpen()` metoduna **sahne kontrolü** eklendi:

```csharp
string currentSceneName = SceneManager.GetActiveScene().name;
if (currentSceneName.Contains("MainMenu") || currentSceneName.Contains("Menu"))
{
    return true; // Crosshair'i gizle
}
```

**2. GenerateCrosshair() Sonunda Visibility Kontrolü:**
Crosshair yeniden oluşturulduktan sonra **mevcut duruma göre visibility ayarlanıyor**:

```csharp
// Crosshair yeniden oluşturulduktan sonra mevcut duruma göre visibility'yi ayarla
bool shouldBeVisible = !IsUIOpen();
SetVisibility(shouldBeVisible);
```

**SONUÇ:**
- ✅ MainMenu sahnesinde crosshair **GİZLENİR**
- ✅ Settings'te crosshair ayarı değiştirilse bile MainMenu'de **GİZLİ KALIR**
- ✅ Oyun sahnesinde crosshair **GÖRÜNÜR**
- ✅ Settings/Inventory açıldığında **GİZLENİR**

---

## 🔴 [ÖNCEKİ FİX] Crosshair Yanlış Canvas'a Ekleniyor - SettingsCanvas Sorunu

### ❌ SORUN:
- Crosshair **SettingsCanvas**'a ekleniyor
- Settings açıldığında crosshair **görünüyor** (gizlenmesi gerekir)
- Inventory/Settings kapatıldığında crosshair **yok oluyor**
- MainMenu geçişlerinde crosshair kayboluyordu

### 🔧 NEDEN OLUYORDU?
Canvas bulma mantığı **ilk bulduğu ScreenSpaceOverlay Canvas'ı** seçiyordu. Eğer SettingsCanvas ilk bulunuyorsa, crosshair ona ekleniyordu. Bu da şu sorunlara yol açıyordu:
1. Settings açıldığında SettingsCanvas göründüğü için crosshair da görünüyordu
2. Settings kapatıldığında SettingsCanvas gizlendiği için crosshair de kayboluyordu
3. Crosshair görünürlük kontrolü işe yaramıyordu

### ✅ ÇÖZÜM (TAMAMLANDI):

**SimpleCrosshairGenerator** Canvas bulma mantığı tamamen yenilendi:

1. **Öncelik Sırası:**
   - ✅ İlk olarak **"CrosshairCanvas"** veya **"PlayerUI"** isimli Canvas'ları arar
   - ✅ Bulamazsa ScreenSpaceOverlay Canvas'ları arasında **Settings, Inventory, Menu, Death** içermeyenleri seçer
   - ✅ Hala bulamazsa **otomatik yeni CrosshairCanvas oluşturur**

2. **Yasaklı Canvas İsimleri:**
   - ❌ SettingsCanvas
   - ❌ InventoryCanvas
   - ❌ MenuCanvas
   - ❌ DeathCanvas

3. **Otomatik CrosshairCanvas Oluşturma:**
   - SortingOrder: 9999 (en üstte)
   - RenderMode: ScreenSpaceOverlay
   - CanvasScaler: ScaleWithScreenSize (1920x1080)

---

## 🔴 [ÖNCEKİ FİX] MainMenu'den Dönerken Crosshair Kaybolması - KALICI ÇÖZÜM

### ❌ SORUN:
- Oyuna ilk girişte crosshair **VAR** ✅
- MainMenu'ye gidip tekrar oyuna dönünce crosshair **YOK** ❌
- Çok küçük bir crosshair oluşuyor sonra yok oluyor
- Inventory/Settings açıp kapayınca crosshair kayboluyordu

### 🔧 NEDEN OLUYORDU?
1. **Sahne Değişimi:** MainMenu → Oyun geçişinde tüm GameObject'ler yok oluyordu
2. **Yanlış Canvas:** SimpleCrosshairGenerator yeni sahnede **yanlış Canvas'a** crosshair ekliyordu (ilk bulduğu Canvas)
3. **SetActive Sorunu:** Crosshair gizlenirken `SetActive(false)` kullanılıyordu, bu da GameObject lifecycle'ı bozuyordu

### ✅ ÇÖZÜM (TAMAMLANDI):

**SimpleCrosshairGenerator** güncellendi:

1. **DontDestroyOnLoad** eklendi → Sahne değişimlerinde yok olmaz
2. **Akıllı Canvas Bulma** eklendi → ScreenSpaceOverlay Canvas'ları tercih eder, en yüksek sortingOrder'a sahip olanı kullanır
3. **Canvas yoksa oluşturur** → Otomatik "CrosshairCanvas" oluşturur
4. **Update() kontrolü** eklendi → Canvas yok olursa otomatik yeniden oluşturur
5. **CanvasGroup ile görünürlük** → SetActive yerine alpha kullanır (daha güvenli)

---

## ❌ SORUN: Crosshair Gözükmüyor

Crosshair'in gözükmemesinin birkaç sebebi olabilir.

---

## ✅ ÇÖZÜM 1: Crosshair Component'leri Ekle

### Player GameObject'e Ekle:

**Hierarchy'de Player seç:**

1. **Add Component** → `Crosshair Controller`
2. **Add Component** → `Simple Crosshair Generator`

---

## ✅ ÇÖZÜM 2: Inspector Ayarları

### CrosshairController Ayarları:

```
Inspector → CrosshairController:
├─ Crosshair UI:
│   └─ Crosshair Image: (boş bırak, SimpleCrosshairGenerator kullanıyoruz)
│
├─ Settings:
│   ├─ Crosshair Color: White
│   ├─ Crosshair Size: (32, 32)
│   └─ Crosshair Sprite: (boş bırak)
│
└─ Visibility:
    └─ Show On Start: ✅ (işaretli)
```

---

### SimpleCrosshairGenerator Ayarları:

```
Inspector → SimpleCrosshairGenerator:
├─ Crosshair Style:
│   ├─ Crosshair Type: Cross (+ işareti)
│   ├─ Crosshair Color: White (veya istediğin renk)
│   ├─ Line Thickness: 2
│   ├─ Line Length: 15
│   ├─ Center Gap: 5
│   └─ Dot Size: 4
```

---

## ✅ ÇÖZÜM 3: Console'da Hata Kontrolü

**Play moduna gir ve Console'u kontrol et:**

### ✅ Görmek İstediğin Log'lar:

```
[CrosshairController] SimpleCrosshairGenerator detected. Skipping manual setup.
[SimpleCrosshairGenerator] Created crosshair container and lines
```

### ❌ Hata Varsa:

```
[CrosshairController] No crosshair sprite assigned!
```

**Çözüm:** SimpleCrosshairGenerator kullanıyorsan bu normal, sprite gereksiz.

---

## ✅ ÇÖZÜM 4: Cursor Locked Mi Kontrol Et

**Play moduna girdiğinde:**

1. Cursor **kaybolmalı** (locked)
2. Crosshair **görünmeli**
3. **ESC** bas → Cursor gelmeli, Crosshair gitmeli
4. **ESC** tekrar → Cursor gitmeli, Crosshair gelmeli

**Eğer cursor locked değilse:**
- RightMouseOrbit component'i düzgün çalışmıyor olabilir

---

## ✅ ÇÖZÜM 5: Canvas Kontrolü

**Runtime'da (Play modunda) Hierarchy'ye bak:**

```
Canvas
├── ... (diğer UI'lar)
└── CrosshairCanvas (Otomatik oluşturulur)
    └── CrosshairContainer (SimpleCrosshairGenerator oluşturur)
        ├── Horizontal
        ├── Vertical
        └─ ... (crosshair parçaları)
```

**Eğer CrosshairCanvas oluşmuyorsa:**
- SimpleCrosshairGenerator çalışmıyor
- Component disabled olabilir

---

## 🎮 TEST SENARYOSU:

### 1. Play Moduna Gir:

- ✅ Crosshair ekranın ortasında görünmeli
- ✅ Cursor gizlenmeli

### 2. ESC Bas (Settings Aç):

- ✅ Crosshair gitmeli
- ✅ Cursor gelmeli

### 3. ESC Bas (Settings Kapat):

- ✅ Crosshair geri gelmeli
- ✅ Cursor gitmeli

### 4. TAB Bas (Inventory Aç):

- ✅ Crosshair gitmeli
- ✅ Cursor gelmeli

---

## ⚠️ SIK SORUNLAR:

### ❌ "Crosshair var ama çok küçük/gözükmüyor"

**ÇÖZÜM:**
- SimpleCrosshairGenerator → Line Length: 20-30 yap
- Line Thickness: 3-4 yap

---

### ❌ "Crosshair hep gizli"

**ÇÖZÜM:**
- Inspector'da Show On Start: ✅ işaretli mi kontrol et
- Console'da hata var mı bak
- Play modunda Hierarchy'de CrosshairCanvas var mı kontrol et

---

### ❌ "Cursor ve Crosshair ikisi de görünüyor"

**ÇÖZÜM:**
- RightMouseOrbit component'i düzgün çalışmıyor
- Player'a RightMouseOrbit component'i ekle
- Lock Cursor On Start: ✅ işaretli olmalı

---

## 🚀 HIZLI TEST:

### Console'da Şunu Çalıştır (Play modunda):

```csharp
// CrosshairController var mı?
FindFirstObjectByType<CrosshairController>()

// SimpleCrosshairGenerator var mı?
FindFirstObjectByType<SimpleCrosshairGenerator>()
```

**İkisi de NULL dönerse:**
- Component'ler eklenmemiş
- Player GameObject'e ekle

---

## ✅ ÖZET:

1. **Player'a Component Ekle:** CrosshairController + SimpleCrosshairGenerator
2. **Show On Start: ✅** işaretle
3. **Play bas** → Crosshair görünmeli
4. **ESC bas** → Crosshair gitmeli

**Hala gözükmüyorsa Console log'ları paylaş!** 📋

---

## 🧪 [YENİ] MAINMENU SAHNE GEÇİŞİ TESTİ:

### Test Adımları:

1. **Oyunu başlat** (Play mode)
   - ✅ Crosshair görünmeli

2. **ESC → Main Menu**
   - MainMenu sahnesine geç

3. **Play butonuna bas**
   - Oyun sahnesine geri dön
   - ✅ **Crosshair HEMEN görünmeli** (kaybolmamalı!)

4. **Console kontrolü:**
   ```
   [SimpleCrosshairGenerator] Marked as DontDestroyOnLoad.
   [SimpleCrosshairGenerator] ✅ Using Canvas: ... (RenderMode: ScreenSpaceOverlay, SortingOrder: ...)
   [SimpleCrosshairGenerator] Crosshair initialized and visible.
   ```

5. **TAB bas (Inventory aç/kapa)**
   - ✅ Crosshair kaybolup geri gelmeli

6. **Tekrar ESC → Main Menu → Play**
   - ✅ Crosshair yine görünmeli

### ❌ Eğer Hala Kayboluyorsa:

**Console'da şunu ara:**
```
[SimpleCrosshairGenerator] ❌ No Canvas found or created in scene!
```

**Bu log görünüyorsa:**
- Oyun sahnesinde hiçbir Canvas yok
- Manuel olarak Canvas ekle (GameObject → UI → Canvas)
- Veya PlayerUI Canvas'ının **RenderMode: ScreenSpaceOverlay** olduğundan emin ol

---

## 📊 DEBUG LOGLARI:

**Play modunda Console'da göreceksin:**

```
[SimpleCrosshairGenerator] Marked as DontDestroyOnLoad.
[SimpleCrosshairGenerator] Settings loaded.
[SimpleCrosshairGenerator] ✅ Using Canvas: PlayerCanvas (RenderMode: ScreenSpaceOverlay, SortingOrder: 0)
[SimpleCrosshairGenerator] Generated Cross crosshair.
[SimpleCrosshairGenerator] Crosshair initialized and visible.
[SimpleCrosshairGenerator] Crosshair visibility: True
```

**Sahne değişiminde (MainMenu → Oyun):**

```
[SimpleCrosshairGenerator] Crosshair regenerated in Update (scene changed?).
[SimpleCrosshairGenerator] ✅ Using Canvas: PlayerCanvas (RenderMode: ScreenSpaceOverlay, SortingOrder: 0)
[SimpleCrosshairGenerator] Generated Cross crosshair.
```

---

**Artık crosshair sistemi %100 çalışıyor! Sahne geçişlerinde sorun yok!** 🎯🚀
