# 🎨 Animasyonlu Healthbar + Death Screen Kurulum Rehberi

## 📋 Sistem Özellikleri

### ✅ Healthbar Sistemi:
- **8 Aşamalı Animasyonlu Healthbar**
- Her aşamada 2 sprite (01 ve 02) arası otomatik geçiş
- Can durumuna göre otomatik görsel değişimi:
  - **Full (76-100%)**: Healthbar_Full_01.png ↔ Healthbar_Full_02.png
  - **Middle (51-75%)**: Healthbar_Middle_01.png ↔ Healthbar_Middle_02.png
  - **Little (26-50%)**: Healthbar_Little_01.png ↔ Healthbar_Little_02.png
  - **Critical (1-25%)**: Healthbar_Kritik_01.png ↔ Healthbar_Kritik_02.png
  - **Dead (0%)**: Healthbar_Defual_t.png

### ✅ Death Screen:
- Kaç para ile öldüğünüz
- Kaç gün hayatta kaldığınız
- MainMenu'ye dönüş butonu
- Otomatik pause ve cursor gösterme

---

## 🎯 1. ADIM: Unity'de Import Ayarları

### Healthbar Görsellerini Import Et:

```
Assets/UI/Player/Healthbars/
  ├── Healthbar_Defual_t.png     ← Boş healthbar (0 can)
  ├── Healthbar_Full_01.png      ← Tam can (variant 1)
  ├── Healthbar_Full_02.png      ← Tam can (variant 2)
  ├── Healthbar_Middle_01.png    ← Orta can (variant 1)
  ├── Healthbar_Middle_02.png    ← Orta can (variant 2)
  ├── Healthbar_Little_01.png    ← Az can (variant 1)
  ├── Healthbar_Little_02.png    ← Az can (variant 2)
  ├── Healthbar_Kritik_01.png    ← Kritik can (variant 1)
  └── Healthbar_Kritik_02.png    ← Kritik can (variant 2)
```

**Her Görsel İçin Import Ayarları:**
1. **Görseli seç** (Project window'da)
2. **Inspector → Texture Type: Sprite (2D and UI)**
3. **Pixels Per Unit: 100**
4. **Filter Mode: Bilinear**
5. **Compression: None** (daha net görünür)
6. ✅ **Apply** butonuna bas

---

## 🎮 2. ADIM: Canvas ve UI Kurulumu

### A. Healthbar UI Oluştur:

**Hierarchy'de sağ tık:**

```
Canvas (eğer yoksa: UI > Canvas)
├── PlayerHealthbarPanel (UI > Panel)
    └── HealthbarImage (UI > Image) ← Buraya healthbar sprite'ları gelecek
```

#### **PlayerHealthbarPanel Ayarları:**
```
RectTransform:
- Anchor: Top-Left
- Pos X: 50, Pos Y: -50
- Width: 256, Height: 64 (veya görselinin boyutu)
```

#### **HealthbarImage Ayarları:**
```
Inspector:
- Image Component:
  - Source Image: Healthbar_Full_01 (başlangıç için)
  - Color: White (#FFFFFF)
  - Preserve Aspect: ✅ (işaretle)
  
- RectTransform:
  - Anchor: Stretch-Stretch
  - Left: 0, Right: 0, Top: 0, Bottom: 0
```

---

### B. Death Screen UI Oluştur:

**Hierarchy'de sağ tık:**

```
Canvas
├── DeathScreenPanel (UI > Panel)
    ├── BackgroundImage (UI > Image) ← Ölüm ekranı arkaplanı
    ├── MoneyText (UI > Text - TextMeshPro)
    ├── DaysText (UI > Text - TextMeshPro)
    └── MainMenuButton (UI > Button - TextMeshPro)
```

#### **DeathScreenPanel Ayarları:**
```
RectTransform:
- Anchor: Stretch-Stretch (tam ekran)
- Left: 0, Right: 0, Top: 0, Bottom: 0
```

#### **BackgroundImage Ayarları:**
```
Inspector:
- Source Image: Senin ölüm ekranı görselini
- Color: Yarı şeffaf siyah (#000000, Alpha: 200)
```

#### **MoneyText Ayarları:**
```
TextMeshProUGUI:
- Text: "Toplam Para: 0$"
- Font Size: 36
- Alignment: Center-Middle
- Color: Sarı (#FFD700)

RectTransform:
- Anchor: Center-Middle
- Pos Y: 50
- Width: 500, Height: 60
```

#### **DaysText Ayarları:**
```
TextMeshProUGUI:
- Text: "0 Gün Hayatta Kaldın"
- Font Size: 30
- Alignment: Center-Middle
- Color: Beyaz (#FFFFFF)

RectTransform:
- Anchor: Center-Middle
- Pos Y: -20
- Width: 500, Height: 50
```

#### **MainMenuButton Ayarları:**
```
Button:
- Text: "ANA MENÜYE DÖN"

RectTransform:
- Anchor: Center-Middle
- Pos Y: -120
- Width: 300, Height: 60
```

---

## 🔧 3. ADIM: Script Bağlantıları

### A. Player GameObject'e Component Ekle:

**Player GameObject'i seç:**

1. **Add Component** → `PlayerHealth`
2. **Add Component** → `PlayerHealthbarUI`
3. **Add Component** → `DeathScreenUI`

---

### B. PlayerHealthbarUI Inspector Ayarları:

```csharp
PlayerHealthbarUI:
  ┌─ Healthbar Type: AnimatedSprites ✅
  │
  ├─ [Animated Sprites Settings]
  │   ├─ Healthbar Image: HealthbarImage (obje sürükle) ✅
  │   ├─ Empty Healthbar: Healthbar_Defual_t.png
  │   ├─ Full Healthbars (Size: 2):
  │   │   ├─ [0]: Healthbar_Full_01.png
  │   │   └─ [1]: Healthbar_Full_02.png
  │   ├─ Middle Healthbars (Size: 2):
  │   │   ├─ [0]: Healthbar_Middle_01.png
  │   │   └─ [1]: Healthbar_Middle_02.png
  │   ├─ Little Healthbars (Size: 2):
  │   │   ├─ [0]: Healthbar_Little_01.png
  │   │   └─ [1]: Healthbar_Little_02.png
  │   ├─ Critical Healthbars (Size: 2):
  │   │   ├─ [0]: Healthbar_Kritik_01.png
  │   │   └─ [1]: Healthbar_Kritik_02.png
  │   └─ Animation Speed: 0.3 (saniye, 01-02 geçiş hızı)
  │
  ├─ [Optional Text]
  │   ├─ Health Text: (opsiyonel, can yazısı için)
  │   └─ Health Text Format: "{0}/{1}"
  │
  └─ [Player Reference]
      └─ Player Health: (otomatik bulunur)
```

**Array'lere sprite ekleme:**
1. **Full Healthbars** yanındaki ok'a tıkla
2. **Size: 2** yap
3. **Element 0:** `Healthbar_Full_01.png` sürükle
4. **Element 1:** `Healthbar_Full_02.png` sürükle
5. Diğer array'ler için tekrarla!

---

### C. DeathScreenUI Inspector Ayarları:

```csharp
DeathScreenUI:
  ┌─ [UI References]
  │   ├─ Death Screen Panel: DeathScreenPanel (obje sürükle) ✅
  │   ├─ Background Image: BackgroundImage
  │   ├─ Money Text: MoneyText
  │   ├─ Days Text: DaysText
  │   └─ Main Menu Button: MainMenuButton
  │
  ├─ [Text Formats]
  │   ├─ Money Text Format: "Toplam Para: {0}$"
  │   └─ Days Text Format: "{0} Gün Hayatta Kaldın"
  │
  ├─ [Settings]
  │   ├─ Pause Game On Death: ✅
  │   ├─ Show Cursor On Death: ✅
  │   └─ Main Menu Scene Name: "MainMenu"
  │
  └─ [Player References]
      └─ Player Health: (otomatik bulunur)
```

---

## 🎮 4. ADIM: Test Et!

### F1-F4 Tuşları ile Test:

1. **Play** butonuna bas
2. **F1** - 10 hasar al (healthbar değişmeli!)
3. **F2** - 20 can kazan
4. **F3** - Tam can doldur
5. **F4** - Öl (Death Screen görünmeli!)

### Can Aşamaları Test:

```
100 Can → Healthbar_Full_01 ↔ Healthbar_Full_02 (animasyonlu)
75 Can  → Healthbar_Middle_01 ↔ Healthbar_Middle_02
50 Can  → Healthbar_Little_01 ↔ Healthbar_Little_02
20 Can  → Healthbar_Kritik_01 ↔ Healthbar_Kritik_02 (hızlı yanıp söner)
0 Can   → Healthbar_Defual_t (boş) + Death Screen açılır!
```

---

## 💰 5. Para ve Gün Sistemi

### Para Sistemi (InventoryManager):

Death Screen'de **kaç para ile öldüğünüz** otomatik gösterilir.

`DeathScreenUI.cs` şu fonksiyonu kullanır:
```csharp
InventoryManager.GetCurrentMoney()
```

### Gün Sistemi (PlayerPrefs):

**Kaç gün hayatta kaldığınız** için bir sistem eklemen gerekiyor:

```csharp
// Oyun başladığında (GameManager veya başka bir yerde):
PlayerPrefs.SetInt("DaysSurvived", 0);

// Her gün geçtiğinde:
int days = PlayerPrefs.GetInt("DaysSurvived", 0);
PlayerPrefs.SetInt("DaysSurvived", days + 1);
PlayerPrefs.Save();
```

---

## ⚙️ 6. Ayarlar

### Animasyon Hızı Değiştir:

`PlayerHealthbarUI` → **Animation Speed:**
- **0.1** saniye = Çok hızlı yanıp sönme
- **0.3** saniye = Normal (önerilen)
- **0.5** saniye = Yavaş geçiş

### Can Aralıklarını Değiştir:

`PlayerHealthbarUI.cs` → `UpdateAnimatedHealthbar()` metodunda:

```csharp
if (healthPercentage > 0.75f)       // 76-100% (Full)
else if (healthPercentage > 0.50f)  // 51-75% (Middle)
else if (healthPercentage > 0.25f)  // 26-50% (Little)
else                                // 1-25% (Critical)
```

Bu değerleri istediğin gibi değiştirebilirsin!

---

## 🎨 7. Görsel İpuçları

### Healthbar Sprite Boyutları:

- **256x64 px** (önerilen)
- **512x128 px** (yüksek çözünürlük)
- **128x32 px** (minimalist)

### Death Screen Arkaplan:

- **1920x1080 px** (Full HD)
- **Transparent PNG** veya **koyu arkaplan**
- Örnek: Karanlık odada masa, kan efekti, vb.

---

## ✅ Tamamlandı!

Artık:
- ✅ 8 aşamalı animasyonlu healthbar çalışıyor
- ✅ Can durumuna göre otomatik sprite değişimi
- ✅ Ölüm ekranı para ve gün gösteriyor
- ✅ MainMenu'ye dönüş butonu

**Daha fazla soru varsa sor!** 🚀
