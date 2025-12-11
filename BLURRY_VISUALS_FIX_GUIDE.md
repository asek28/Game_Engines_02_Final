# Unity 3D Oyun Bulanıklık Düzeltme Rehberi

## 🔍 Adım Adım Troubleshooting Checklist

### 1. ✅ URP Render Pipeline Asset Ayarları

**Konum:** `Edit > Project Settings > Graphics` veya `Assets/Settings/PC_RPAsset.asset`

#### Render Scale Kontrolü
- [ ] **Render Scale = 1.0** olmalı
  - Render Scale < 1.0 oyunu düşük çözünürlükte render eder
  - **Nasıl kontrol edilir:**
    1. `Assets/Settings/PC_RPAsset.asset` dosyasını seç
    2. Inspector'da `Render Scale` değerini kontrol et
    3. Eğer 0.5-0.8 arasındaysa, **1.0'a çıkar**

#### Anti-Aliasing (AA) Ayarları
- [ ] **FXAA yerine SMAA veya MSAA kullan**
  - FXAA genellikle daha bulanık görünür
  - **Önerilen ayarlar:**
    - **SMAA (Subpixel Morphological AA)**: En keskin görüntü, orta performans
    - **MSAA (Multi-Sample AA)**: İyi kalite, yüksek performans maliyeti
    - **FXAA**: En bulanık, en düşük performans maliyeti
  
  **Nasıl değiştirilir:**
  1. `PC_RPAsset.asset` dosyasını seç
  2. Inspector'da `Anti Aliasing (MSAA)` ayarını bul
  3. **2x, 4x, veya 8x MSAA** seç (veya SMAA aktifse onu kullan)
  4. FXAA'yi **kapat**

---

### 2. ✅ Quality Settings (Kalite Ayarları)

**Konum:** `Edit > Project Settings > Quality`

#### Texture Quality
- [ ] **Texture Quality = Full Res** olmalı
  - **Kontrol:**
    1. `Edit > Project Settings > Quality`
    2. Aktif quality level'i seç (PC)
    3. `Global Texture Mipmap Limit = 0` olmalı (Full Res)
    4. Eğer 1, 2, veya 3 ise, **0'a çıkar**

#### Anti-Aliasing (Quality Settings)
- [ ] **Quality Settings'te AA = Disabled veya MSAA**
  - Quality Settings'teki AA, Render Pipeline'daki ayarlarla çakışabilir
  - **Önerilen:** Quality Settings'te **Disabled** bırak, URP Asset'te MSAA kullan

---

### 3. ✅ Post-Processing Volume Profile

**Konum:** `Assets/Settings/DefaultVolumeProfile.asset` veya Scene'deki Volume component

#### Depth of Field (DoF) Kontrolü
- [ ] **DoF'u geçici olarak kapat ve test et**
  - DoF yanlış ayarlanmışsa tüm sahne bulanık görünebilir
  - **Kontrol:**
    1. Scene'deki `Volume` component'ini bul
    2. Volume Profile'ı aç
    3. `Depth of Field` efektini bul
    4. **Geçici olarak kapat** (checkbox'ı kaldır)
    5. Oyunu test et - eğer keskinleştiyse, DoF ayarlarını düzelt:
       - `Focus Distance`: Player'a yakın objeler net olmalı
       - `Aperture`: Düşük değer (f/1.4 - f/2.8) daha fazla blur
       - `Focal Length`: 50mm civarı normal görünüm

#### Chromatic Aberration
- [ ] **Chromatic Aberration = 0 veya çok düşük**
  - Yüksek değerler bulanıklık yaratır
  - **Önerilen:** 0.1-0.2 arası veya tamamen kapat

#### Motion Blur
- [ ] **Motion Blur'u kapat veya çok düşük yap**
  - Hareket sırasında bulanıklık yaratır
  - **Önerilen:** Kapat (0) veya çok düşük (0.1-0.2)

#### Bloom
- [ ] **Bloom Intensity kontrolü**
  - Çok yüksek bloom bulanıklık yaratabilir
  - **Önerilen:** 0.5-1.0 arası

---

### 4. ✅ Unity Editor Game View Ayarları

#### Game View Scale
- [ ] **Scale = 1x** olmalı
  - **Kontrol:**
    1. Game View penceresini aç
    2. Sağ üstteki **Scale** slider'ını kontrol et
    3. **1x** olmalı (100%)
    4. Eğer 0.5x, 0.75x gibi düşükse, **1x'e çıkar**

#### Low Resolution Aspect Ratios
- [ ] **Low Resolution Aspect Ratios KAPALI olmalı**
  - **Kontrol:**
    1. Game View penceresinde sağ üstteki **dropdown** menüyü aç
    2. `Low Resolution Aspect Ratios` seçeneğini kontrol et
    3. **KAPALI** olmalı (işaretli değilse)

#### Game View Resolution
- [ ] **Game View çözünürlüğü yeterince yüksek mi?**
  - Çok düşük çözünürlük bulanık görünebilir
  - **Önerilen:** En az 1920x1080 veya monitörünüzün native çözünürlüğü

---

### 5. ✅ Camera Ayarları

#### Camera Render Texture
- [ ] **Camera'nın Render Texture kullanmadığından emin ol**
  - Eğer Camera bir Render Texture'a render ediyorsa, çözünürlüğü kontrol et
  - **Kontrol:**
    1. Main Camera'yı seç
    2. Inspector'da `Output Target` = `Screen` olmalı
    3. Eğer `Render Texture` seçiliyse, Render Texture'ın çözünürlüğünü kontrol et

#### Camera Far Clipping Plane
- [ ] **Far Clipping Plane çok yüksek değil mi?**
  - Çok yüksek değerler precision sorunlarına yol açabilir
  - **Önerilen:** 1000-5000 arası

---

### 6. ✅ Texture Import Ayarları

#### Texture Compression
- [ ] **Texture'ların sıkıştırma ayarlarını kontrol et**
  - Aşırı sıkıştırma bulanıklık yaratabilir
  - **Kontrol:**
    1. Bir texture asset'ini seç
    2. Inspector'da `Max Size` yeterince yüksek mi? (2048, 4096)
    3. `Compression` = `None` veya `High Quality` test et

---

### 7. ✅ URP Asset - Diğer Ayarlar

#### HDR
- [ ] **HDR aktif mi?**
  - HDR kapalıysa renk aralığı sınırlı olabilir
  - **Kontrol:** `PC_RPAsset.asset` > `HDR` = **Enabled**

#### Shadow Distance
- [ ] **Shadow Distance çok düşük değil mi?**
  - Çok düşük shadow distance görsel kaliteyi etkileyebilir
  - **Önerilen:** 50-100 arası

---

## 🎯 Hızlı Test Adımları

1. **Render Scale'i 1.0 yap** (en önemli!)
2. **FXAA'yı kapat, MSAA 4x aç**
3. **Post-Processing'teki DoF, Motion Blur, Chromatic Aberration'ı kapat**
4. **Game View Scale'i 1x yap**
5. **Oyunu test et**

Eğer hala bulanıksa:
- Texture Quality'yi kontrol et
- Camera ayarlarını kontrol et
- Texture import ayarlarını kontrol et

---

## 📝 Önerilen Optimal Ayarlar

### URP Render Pipeline Asset
- **Render Scale:** 1.0
- **Anti-Aliasing:** MSAA 4x (veya SMAA)
- **HDR:** Enabled
- **Shadow Distance:** 50-100

### Quality Settings
- **Global Texture Mipmap Limit:** 0 (Full Res)
- **Anti-Aliasing:** Disabled (URP Asset'teki ayarı kullan)

### Post-Processing
- **Depth of Field:** Kapat veya çok düşük
- **Motion Blur:** Kapat
- **Chromatic Aberration:** 0-0.2
- **Bloom:** 0.5-1.0

### Game View
- **Scale:** 1x (100%)
- **Low Resolution Aspect Ratios:** Kapalı

---

## ⚠️ Performans vs. Kalite Dengesi

Eğer yukarıdaki ayarlar performans sorunlarına yol açarsa:

1. **MSAA 4x → MSAA 2x** (daha az keskin ama daha iyi performans)
2. **Render Scale 1.0 → 0.9** (hafif bulanıklık ama daha iyi FPS)
3. **Texture Quality:** Full Res yerine -1 mipmap (hafif kalite kaybı)

---

## 🔧 Script ile Otomatik Kontrol

Aşağıdaki script'i kullanarak ayarları otomatik kontrol edebilirsiniz:

```csharp
// Editor script olarak kullanılabilir
// Assets/Editor/CheckRenderSettings.cs
```

Bu rehberi takip ederek oyununuzun görsel kalitesini önemli ölçüde artırabilirsiniz!

