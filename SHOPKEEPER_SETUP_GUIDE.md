# 🏪 Shopkeeper System - Kurulum Rehberi

## ✅ Özellikler

1. **Rastgele Oturma Animasyonları** - Shopkeeper oturarak rastgele animasyonlar arası geçiş yapar
2. **Proximity Detection** - Player yaklaşınca "Press E to Shop" mesajı gösterir
3. **E Tuşu ile Shop Açma** - Shop Canvas açılır
4. **ESC ile Kapatma** - Shop Canvas kapanır
5. **Pause & Cursor Control** - Shop açıldığında oyun durur, cursor görünür

---

## 📋 Adım 1: Shopkeeper GameObject Hazırlığı

### Hierarchy'de Shopkeeper GameObject'i Oluştur:

1. **Hierarchy → Right Click → Create Empty**
2. **İsim:** `Shopkeeper`
3. **Position:** Shop sahnesinde istediğin yere koy (örn: `X: 0, Y: 0, Z: 0`)

### Shopkeeper'a Model Ekle:

1. **Assets → Animation → Shopkeeper** klasöründen Shopkeeper modelini sürükle
2. **Shopkeeper GameObject'inin child'ı yap**

---

## 📋 Adım 2: Animator Ayarları

### Animator Controller Oluştur:

1. **Assets → Right Click → Create → Animator Controller**
2. **İsim:** `ShopkeeperAnimator`
3. **Shopkeeper GameObject'e Animator component ekle**
4. **Animator Controller:** `ShopkeeperAnimator` ata

### Animasyon State'lerini Ekle:

**Animator window'da:**

1. **Sitting Angry** state'i ekle
   - Animation clip: `Sitting Angry`
   
2. **Sitting Disbelief** state'i ekle
   - Animation clip: `Sitting Disbelief`

3. **Entry → Sitting Angry** transition oluştur (varsayılan başlangıç)

**NOT:** Transition'lar otomatik olacak, ShopkeeperController script'i yönetecek!

---

## 📋 Adım 3: ShopkeeperController Script Kurulumu

### Shopkeeper GameObject'e Script Ekle:

1. **Shopkeeper GameObject'i seç**
2. **Add Component → ShopkeeperController**

### Inspector Ayarları:

```
ShopkeeperController:
├─ Animator:
│   └─ Shopkeeper Animator (Animator component'i sürükle)
│
├─ Sitting Animation States:
│   ├─ Size: 2
│   ├─ Element 0: "Sitting Angry"
│   └─ Element 1: "Sitting Disbelief"
│
├─ Animation Switch Interval: 5 (saniye)
├─ Random Switching: ✅ (işaretli)
│
├─ Interaction Range: 3 (metre)
├─ Press E Prompt: (şimdi oluşturacağız ↓)
│
└─ Shop Canvas: (şimdi oluşturacağız ↓)
```

---

## 📋 Adım 4: "Press E" Prompt UI Oluştur

### Hierarchy'de Canvas Oluştur:

1. **Hierarchy → Right Click → UI → Canvas**
2. **İsim:** `ShopPromptCanvas`
3. **Render Mode:** `World Space`
4. **Position:** Shopkeeper'ın üstünde (örn: `Y: 2.5`)
5. **Scale:** `0.01, 0.01, 0.01` (küçült)

### Canvas ayarları:

```
ShopPromptCanvas (Canvas):
├─ Render Mode: World Space
├─ Position: (0, 2.5, 0) - Shopkeeper'ın üstünde
├─ Width: 200
├─ Height: 50
└─ Scale: (0.01, 0.01, 0.01)
```

### Text Oluştur:

1. **ShopPromptCanvas → Right Click → UI → Text - TextMeshPro**
2. **İsim:** `PressEText`
3. **Text:** `Press E to Shop`
4. **Font Size:** `36`
5. **Alignment:** `Center & Middle`
6. **Color:** `White` veya `Yellow`

### Canvas'ı Shopkeeper'a Bağla:

1. **Shopkeeper GameObject'i seç**
2. **ShopkeeperController → Press E Prompt:** `ShopPromptCanvas` sürükle

---

## 📋 Adım 5: Shop UI Canvas Oluştur

### Hierarchy'de Shop Canvas Oluştur:

1. **Hierarchy → Right Click → UI → Canvas**
2. **İsim:** `ShopCanvas`
3. **Render Mode:** `Screen Space - Overlay`

### Background Panel Oluştur:

1. **ShopCanvas → Right Click → UI → Panel**
2. **İsim:** `ShopPanel`
3. **Color:** Koyu renk (semi-transparent)

### Shop UI Elemanları:

**Başlık Text:**
1. **ShopPanel → Right Click → UI → Text - TextMeshPro**
2. **İsim:** `TitleText`
3. **Text:** `Shop`
4. **Font Size:** `48`
5. **Position:** Üst ortada

**Close Button:**
1. **ShopPanel → Right Click → UI → Button - TextMeshPro**
2. **İsim:** `CloseButton`
3. **Text:** `X` veya `Close`
4. **Position:** Sağ üst köşe

**Money Text:**
1. **ShopPanel → Right Click → UI → Text - TextMeshPro**
2. **İsim:** `MoneyText`
3. **Text:** `$0`
4. **Position:** Sağ üst (Close button'ın yanında)

**Welcome Text:**
1. **ShopPanel → Right Click → UI → Text - TextMeshPro**
2. **İsim:** `WelcomeText`
3. **Text:** `Welcome to the Shop!`
4. **Position:** Başlığın altında

### ShopCanvas'ı Başlangıçta Gizle:

1. **ShopCanvas GameObject'i seç**
2. **Inspector'da sağ üstteki checkbox'ı KAPAT** (disabled)

---

## 📋 Adım 6: ShopUI Script Kurulumu

### ShopCanvas'a Script Ekle:

1. **ShopCanvas GameObject'i seç**
2. **Add Component → ShopUI**

### Inspector Ayarları:

```
ShopUI:
├─ Close Button: CloseButton (sürükle)
├─ Money Text: MoneyText (sürükle)
├─ Welcome Text: WelcomeText (sürükle)
└─ Welcome Message: "Welcome to the Shop!"
```

### Close Button Event Bağla:

1. **CloseButton GameObject'i seç**
2. **Button Component → OnClick()**
3. **+ butonuna tıkla**
4. **ShopCanvas'ı sürükle**
5. **Function:** `ShopUI → OnCloseButtonClicked`

---

## 📋 Adım 7: Shopkeeper'a ShopCanvas'ı Bağla

1. **Shopkeeper GameObject'i seç**
2. **ShopkeeperController → Shop Canvas:** `ShopCanvas` sürükle
3. **Pause Game When Shop Open:** ✅ (işaretli)
4. **Show Cursor In Shop:** ✅ (işaretli)

---

## 📋 Adım 8: Player Tag Kontrolü

**Player GameObject'inin Tag'ini kontrol et:**

1. **Hierarchy → Player**
2. **Inspector → Tag:** `Player` olmalı
3. **Değilse:** Tag → Add Tag → `Player` oluştur ve ata

---

## 🎮 TEST ET!

### 1. Play Moduna Gir:

- ✅ Shopkeeper oturuyor mu?
- ✅ Animasyonlar rastgele değişiyor mu? (5 saniyede bir)

### 2. Shopkeeper'a Yaklaş:

- ✅ "Press E to Shop" mesajı görünüyor mu?
- ✅ 3 metre mesafede görünmeli

### 3. E Tuşuna Bas:

- ✅ Shop Canvas açılıyor mu?
- ✅ Oyun duruyor mu? (Time.timeScale = 0)
- ✅ Cursor görünüyor mu?
- ✅ Para miktarı doğru gösteriliyor mu?

### 4. ESC veya Close Button'a Bas:

- ✅ Shop Canvas kapanıyor mu?
- ✅ Oyun devam ediyor mu?
- ✅ Cursor gizleniyor mu?
- ✅ Player hala yakınsa "Press E" mesajı tekrar görünüyor mu?

---

## 🎨 OPSİYONEL İYİLEŞTİRMELER

### 1. Daha Fazla Animasyon Ekle:

**ShopkeeperController → Sitting Animation States:**
```
Size: 4
Element 0: "Sitting Angry"
Element 1: "Sitting Disbelief"
Element 2: "Sitting Idle" (varsa)
Element 3: "Sitting Talking" (varsa)
```

### 2. Audio Efektleri Ekle:

**Assets klasörüne ses dosyaları ekle:**
- `shop_open.mp3`
- `shop_close.mp3`

**ShopkeeperController → Audio:**
```
Shop Open Sound: shop_open
Shop Close Sound: shop_close
```

### 3. Shop Item'ları Ekle:

**ShopUI'da ürün satışı için:**
- Item prefab oluştur (Image + Text + Button)
- ShopUI script'ine item ekleme metodu ekle
- Satın alma sistemi entegre et

---

## 🐛 SORUN GİDERME

### ❌ "Press E" Mesajı Gözükmüyor:

**Kontrol Et:**
1. Player'ın Tag'i "Player" mi?
2. Interaction Range yeterli mi? (3 metre)
3. ShopPromptCanvas aktif mi? (Shopkeeper yaklaşmadan önce)
4. Press E Prompt field'i dolu mu?

### ❌ E Tuşu Çalışmıyor:

**Kontrol Et:**
1. Player Shopkeeper'a yeterince yakın mı?
2. Console'da hata var mı?
3. ShopCanvas assigned mi?

### ❌ Shop Kapanmıyor:

**Kontrol Et:**
1. Close Button event'i bağlı mı?
2. ESC tuşu çalışıyor mu?
3. ShopUI script ShopCanvas'ta mı?

### ❌ Animasyonlar Geçiş Yapmıyor:

**Kontrol Et:**
1. Animator Controller doğru mu?
2. Sitting Animation States doğru yazılmış mı? (büyük/küçük harf önemli!)
3. Random Switching işaretli mi?
4. Animation Switch Interval > 0 mı?

---

## ✅ ÖZET

**Gerekli Bileşenler:**

1. **Shopkeeper GameObject:**
   - Animator
   - ShopkeeperController
   - Audio Source (otomatik eklenir)

2. **ShopPromptCanvas (World Space):**
   - PressEText (TextMeshPro)

3. **ShopCanvas (Screen Space Overlay):**
   - ShopPanel
   - TitleText
   - CloseButton
   - MoneyText
   - WelcomeText
   - ShopUI script

4. **Player:**
   - Tag: "Player"

**Inspector Bağlantıları:**
- ShopkeeperController → Animator, Press E Prompt, Shop Canvas
- ShopUI → Close Button, Money Text, Welcome Text

---

**Artık Shopkeeper sistemi hazır! Test et ve eğer sorun varsa Console loglarını paylaş!** 🏪✨
