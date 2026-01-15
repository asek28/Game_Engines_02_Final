# 💚 Player Healthbar Sistemi - Kurulum Rehberi

## 📋 Özellikler

### PlayerHealth.cs
- ✅ Can yönetimi (hasar, heal, ölüm)
- ✅ Event system (UnityEvent)
- ✅ Can yenilenme (opsiyonel)
- ✅ Ses efektleri (hasar, ölüm)
- ✅ Damage flash (ekran kırmızı yanıp söner)

### PlayerHealthbarUI.cs
- ✅ **2 Mod**: Slider veya Image Fill
- ✅ **Kendi görselinizi kullanabilirsiniz!**
- ✅ Renk değişimi (yeşil → sarı → kırmızı)
- ✅ Yumuşak geçiş animasyonu
- ✅ Text display (80/100 HP)

---

## 🎯 Kurulum Adımları

### Adım 1: Player GameObject'e Script'leri Ekle

1. **Hierarchy > Player'ı seç**
2. **Add Component > Player Health**
3. **Add Component > Player Damage Test** (test için)

### Adım 2: Canvas Oluştur (Healthbar için)

1. **Hierarchy > Right Click > UI > Canvas**
2. İsim: `PlayerUI`
3. Canvas ayarları:
   - **Render Mode**: Screen Space - Overlay
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920x1080

---

## 🎨 Seçenek A: Image Fill Healthbar (Kendi Görselinizle)

### Adım 1: UI Hierarchy Oluştur

```
PlayerUI (Canvas)
└── HealthbarPanel
    ├── HealthbarBackground (Image)
    └── HealthbarFill (Image)
    └── HealthText (TextMeshPro) [Opsiyonel]
```

### Adım 2: HealthbarPanel Oluştur

1. **PlayerUI > Right Click > Create Empty**
2. İsim: `HealthbarPanel`
3. **RectTransform** ayarları:
   - **Anchor**: Top-Left
   - **Position**: X=20, Y=-20 (sol üst köşe)
   - **Width**: 300, **Height**: 40

### Adım 3: HealthbarBackground (Arka Plan)

1. **HealthbarPanel > Right Click > UI > Image**
2. İsim: `HealthbarBackground`
3. **RectTransform**:
   - **Anchor Preset**: Stretch (Alt+Shift tıklayın)
   - **Left/Right/Top/Bottom**: 0
4. **Image** component:
   - **Source Image**: UI/Skin (varsayılan) veya kendi sprite'ınız
   - **Color**: Koyu gri veya siyah (arka plan rengi)
   - **Image Type**: Sliced (sprite'ınız 9-slice ise)

### Adım 4: HealthbarFill (Can Göstergesi)

1. **HealthbarPanel > Right Click > UI > Image**
2. İsim: `HealthbarFill`
3. **RectTransform**:
   - **Anchor Preset**: Stretch
   - **Left/Right/Top/Bottom**: 2 (border padding)
4. **Image** component:
   - **Source Image**: **KENDI SPRITE'INIZI BURAYA SÜRÜKLE!**
   - **Color**: Yeşil (başlangıç rengi - kod değiştirecek)
   - **Image Type**: **Filled**
   - **Fill Method**: Horizontal
   - **Fill Origin**: Left
   - **Fill Amount**: 1 (tam dolu)

### Adım 5: HealthText (Opsiyonel)

1. **HealthbarPanel > Right Click > UI > Text - TextMeshPro**
2. İsim: `HealthText`
3. **RectTransform**:
   - **Anchor Preset**: Stretch
   - **Left/Right/Top/Bottom**: 0
4. **TextMeshPro** ayarları:
   - **Text**: "100/100"
   - **Font Size**: 24
   - **Alignment**: Center
   - **Color**: Beyaz
   - **Outline** (opsiyonel): Siyah, Width=2

### Adım 6: PlayerHealthbarUI Script'i Ekle

1. **HealthbarPanel'i seç**
2. **Add Component > Player Healthbar UI**
3. **Inspector ayarları:**
   - **Healthbar Type** = **Image Fill**
   - **Healthbar Fill Image** = HealthbarFill GameObject'ini sürükle
   - **Healthbar Background Image** = HealthbarBackground GameObject'ini sürükle
   - **Health Text** = HealthText'i sürükle (opsiyonel)
   - **Health Text Format** = "{0}/{1}" (80/100 formatı)
   - **High Health Color** = Yeşil
   - **Medium Health Color** = Sarı
   - **Low Health Color** = Kırmızı
   - **Smooth Transition** = TRUE (yumuşak animasyon)

---

## 🎨 Seçenek B: Slider Healthbar (Basit)

### Adım 1: Slider Oluştur

1. **PlayerUI > Right Click > UI > Slider**
2. İsim: `HealthbarSlider`
3. **RectTransform**:
   - **Anchor**: Top-Left
   - **Position**: X=150, Y=-30
   - **Width**: 200, **Height**: 20

### Adım 2: Slider Ayarları

1. **Slider component:**
   - **Min Value**: 0
   - **Max Value**: 1
   - **Value**: 1 (tam dolu)
   - **Whole Numbers**: FALSE

### Adım 3: Slider Görselleştirme

1. **Slider > Background** → Arka plan rengi (koyu gri)
2. **Slider > Fill Area > Fill** → Can rengi (yeşil)
   - **Image** component: Kendi sprite'ınızı atayabilirsiniz!

### Adım 4: PlayerHealthbarUI Script'i Ekle

1. **HealthbarSlider'ı seç**
2. **Add Component > Player Healthbar UI**
3. **Inspector ayarları:**
   - **Healthbar Type** = **Slider**
   - **Healthbar Slider** = HealthbarSlider component'ini sürükle
   - **Health Text** = (opsiyonel)
   - **High Health Color** = Yeşil
   - **Medium Health Color** = Sarı
   - **Low Health Color** = Kırmızı

---

## 🎮 Test Etme

### Adım 1: Test Script'i Kullan

Play Mode'a geç ve şu tuşlara bas:

| Tuş | Aksiyon |
|-----|---------|
| **F1** | 10 hasar al |
| **F2** | 20 can kazan |
| **F3** | Tam iyileş |
| **F4** | Öldür (test) |

### Adım 2: Console Kontrolü

```
[PlayerHealth] Took 10 damage! Health: 90/100
[PlayerHealthbarUI] Updated: 90/100 (90%)
```

### Adım 3: Visual Kontrol

- ✅ Healthbar azalıyor mu? (F1)
- ✅ Healthbar artıyor mu? (F2)
- ✅ Renk değişiyor mu? (yeşil → sarı → kırmızı)
- ✅ Yumuşak geçiş var mı? (smooth animation)

---

## 🎨 Kendi Görselinizi Kullanma

### Healthbar Sprite Hazırlama:

1. **Photoshop/GIMP'te** healthbar sprite'ı çiz
   - Boyut: 256x32 pixel (veya istediğiniz boyut)
   - Kenarlıklı veya kenarl

ıksız
   - PNG formatında kaydet (şeffaf arka plan)

2. **Unity'ye Import**:
   - Sprite'ı Assets klasörüne sürükle
   - Inspector'da **Texture Type** = Sprite (2D and UI)
   - **Sprite Mode** = Single
   - **Pixels Per Unit** = 100
   - Apply

3. **9-Slice (Opsiyonel)**:
   - Eğer kenarlıklı bir sprite ise
   - Inspector > **Sprite Editor** aç
   - Kenarlardan border'ları ayarla
   - Apply

4. **Healthbar'a Ata**:
   - HealthbarFill GameObject'ini seç
   - Image component > **Source Image** = Kendi sprite'ınız
   - **Image Type** = Filled (veya Sliced)

---

## ⚙️ Inspector Ayarları (PlayerHealth)

### Health Settings:
- **Max Health** = 100 (maksimum can)
- **Start Health** = 100 (başlangıç canı)

### Damage Feedback:
- **Enable Damage Flash** = TRUE (ekran kırmızı yanıp söner)
- **Damage Flash Duration** = 0.2 (efekt süresi)
- **Damage Sound** = Hasar sesi AudioClip (opsiyonel)
- **Death Sound** = Ölüm sesi AudioClip (opsiyonel)

### Regeneration (Opsiyonel):
- **Enable Regeneration** = FALSE (can yenilensin mi?)
- **Regen Per Second** = 1 (saniyede kaç can)
- **Regen Delay** = 5 (hasar aldıktan kaç saniye sonra)

---

## ⚙️ Inspector Ayarları (PlayerHealthbarUI)

### Healthbar Type:
- **Image Fill** → Kendi sprite'ınızı kullanın (önerilen)
- **Slider** → Unity varsayılan slider

### Color Settings:
- **High Health Color** = `#00FF00` (yeşil)
- **Medium Health Color** = `#FFFF00` (sarı)
- **Low Health Color** = `#FF0000` (kırmızı)
- **High Health Threshold** = 0.6 (60% üstü yeşil)
- **Low Health Threshold** = 0.3 (30% altı kırmızı)

### Animation:
- **Smooth Transition** = TRUE (yumuşak azalma)
- **Smooth Speed** = 5 (animasyon hızı)

---

## 🔌 Enemy'den Hasar Alma Entegrasyonu

### EnemyAIController'a Ekle:

```csharp
// EnemyAIController.cs içinde Attack() fonksiyonunda:
private void Attack()
{
    // ... (mevcut kod)
    
    // Player'a hasar ver
    PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
    if (playerHealth != null && !playerHealth.IsDead)
    {
        playerHealth.TakeDamage(attackDamage);
        Debug.Log($"[Enemy] Dealt {attackDamage} damage to player!");
    }
}
```

---

## 🎯 Gelişmiş Özellikler

### Shield/Armor Sistemi Eklemek:

```csharp
// PlayerHealth.cs'ye ekle:
[Header("Shield")]
public int currentShield = 0;
public int maxShield = 50;

public void TakeDamage(int damage)
{
    if (currentShield > 0)
    {
        // Önce shield'a hasar
        int shieldDamage = Mathf.Min(damage, currentShield);
        currentShield -= shieldDamage;
        damage -= shieldDamage;
    }
    
    if (damage > 0)
    {
        // Kalan hasar health'e
        currentHealth -= damage;
        // ...
    }
}
```

### Healthbar Animasyonu (Shake):

```csharp
// PlayerHealthbarUI.cs'ye ekle:
private void UpdateHealthbar(int currentHealth, int maxHealth)
{
    // ... (mevcut kod)
    
    // Hasar alındığında sarsılma
    if (currentHealth < previousHealth)
    {
        StartCoroutine(ShakeHealthbar());
    }
}

private IEnumerator ShakeHealthbar()
{
    Vector3 originalPos = transform.localPosition;
    float elapsed = 0f;
    float duration = 0.2f;
    
    while (elapsed < duration)
    {
        float x = Random.Range(-5f, 5f);
        float y = Random.Range(-5f, 5f);
        transform.localPosition = originalPos + new Vector3(x, y, 0);
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    transform.localPosition = originalPos;
}
```

---

## ✅ Kontrol Listesi

### Script'ler:
- [ ] PlayerHealth.cs eklendi (Player GameObject'e)
- [ ] PlayerHealthbarUI.cs eklendi (Healthbar GameObject'e)
- [ ] PlayerDamageTest.cs eklendi (test için)

### UI:
- [ ] Canvas oluşturuldu (PlayerUI)
- [ ] HealthbarPanel oluşturuldu
- [ ] HealthbarBackground image eklendi
- [ ] HealthbarFill image eklendi (**kendi sprite'ınız**)
- [ ] HealthText eklendi (opsiyonel)

### Ayarlar:
- [ ] PlayerHealthbarUI > Healthbar Type seçildi
- [ ] PlayerHealthbarUI > Image/Slider referansları atandı
- [ ] PlayerHealth > Max Health ayarlandı
- [ ] PlayerHealth > Audio Clips atandı (opsiyonel)

### Test:
- [ ] Play Mode > F1 hasar alıyor
- [ ] Play Mode > F2 can kazanıyor
- [ ] Healthbar yumuşak değişiyor
- [ ] Renk değişiyor (yeşil → sarı → kırmızı)

---

## 🎉 Tamamlandı!

Artık Player'ınızın:
- ✅ Can sistemi var
- ✅ Healthbar görünüyor (kendi görselinizle!)
- ✅ Hasar alıyor ve iyileşiyor
- ✅ Renk değişiyor (yeşil → kırmızı)
- ✅ Yumuşak animasyon var

**Enemy'ler player'a hasar vermek için PlayerHealth.TakeDamage() çağırabilir!**

Test tuşları:
- **F1**: Hasar al
- **F2**: İyileş
- **F3**: Tam iyileş
- **F4**: Öldür

İyi oyunlar! 💚
