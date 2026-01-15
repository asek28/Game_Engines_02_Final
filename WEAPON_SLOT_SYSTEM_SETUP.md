# 🔫 Silah Slot Sistemi - Kurulum Rehberi

## 📋 Sistem Özellikleri
- **3 Silah Slotu** (1, 2, 3 tuşları ile değiştirme)
- **Stick** (Melee - mevcut silah)
- **Gun** (Ateşli silah - YENİ)
- **Inventory'de Dropdown** ile slot ataması
- **IWeapon Interface** (genişletilebilir sistem)

---

## 🎯 Adım 1: Player Hierarchy Ayarları

### Mevcut Stick Silahını Ayarla
1. **Stick GameObject'ini bul** (Player > ... > Stick)
2. Stick'e **MeleeWeapon.cs** scriptini ekle:
   - Inspector > Add Component > Melee Weapon
3. **Weapon Name** = "Stick" olarak ayarla
4. WeaponHitDetector component'i zaten var (otomatik bulunur)
5. **Stick'i pasif et** (SetActive false) - WeaponSlotSystem aktif edecek

---

## 🎯 Adım 2: Gun Silahını Oluştur

### Gun Prefab/GameObject Oluştur
1. **Yeni bir GameObject oluştur** (Hierarchy'de Player'ın child'ı olarak)
   - İsim: `Gun`
   - Stick ile aynı elde (Stick'in parent'ı ile aynı parent)
   
2. **Gun'a GunWeapon.cs scriptini ekle**:
   - Inspector > Add Component > Gun Weapon
   
3. **Gun Settings:**
   - **Weapon Name** = "Gun"
   - **Damage** = 10 (veya istediğiniz değer)
   - **Range** = 50 (menzil)
   - **Fire Rate** = 0.25 (saniye başına ateş)
   
4. **Fire Point** (mermi çıkış noktası):
   - Gun'ın child'ı olarak boş bir GameObject oluştur
   - İsim: `FirePoint`
   - Position'ını silahın namlu ucuna ayarla
   - GunWeapon.cs > Fire Point'e bu transform'u sürükle
   
5. **Visual Effects (Opsiyonel):**
   - **Bullet Trail**: LineRenderer component ekle, GunWeapon > Bullet Trail'e sürükle
   - **Muzzle Flash**: ParticleSystem ekle, GunWeapon > Muzzle Flash'e sürükle
   - **Impact Effect**: Prefab oluştur, GunWeapon > Impact Effect'e sürükle
   
6. **Audio (Opsiyonel):**
   - GunWeapon > Fire Sound: Ateş sesi AudioClip'i sürükle
   
7. **Gun'ı pasif et** (SetActive false)

---

## 🎯 Adım 3: WeaponSlotSystem Kurulumu

### Player'a WeaponSlotSystem Ekle
1. **Player GameObject'i seç**
2. **Add Component > Weapon Slot System**
3. **Weapon Slots ayarları:**
   - **Weapon Slot 1** = Stick GameObject'ini sürükle
   - **Weapon Slot 2** = Gun GameObject'ini sürükle
   - **Weapon Slot 3** = Boş bırak (gelecekte başka silah)
   - **Default Slot** = 1 (Stick ile başla)

---

## 🎯 Adım 4: Inventory UI - Weapon Slot Dropdown'ları

### Inventory Canvas'ına Weapon Slot UI Ekle
1. **Inventory Canvas'ını aç** (Scene'de)
2. **Sağ tarafta yeni bir Panel oluştur**:
   - İsim: `WeaponSlotsPanel`
   - Anchor: Right-Middle veya Top-Right
   
3. **WeaponSlotsPanel'e 3 Dropdown ekle**:
   - **Slot1Dropdown**
   - **Slot2Dropdown**
   - **Slot3Dropdown**
   
4. **Her dropdown'un yanına Label ekle**:
   - "Slot 1", "Slot 2", "Slot 3"

### WeaponSlotUI Script'i Ekle
1. **WeaponSlotsPanel'e WeaponSlotUI.cs scriptini ekle**
2. **Dropdown References:**
   - **Slot 1 Dropdown** = Slot1Dropdown'u sürükle
   - **Slot 2 Dropdown** = Slot2Dropdown'u sürükle
   - **Slot 3 Dropdown** = Slot3Dropdown'u sürükle
   
3. **Weapon References:**
   - **Available Weapons** array size = 2
   - Element 0 = Stick
   - Element 1 = Gun
   
4. **System Reference:**
   - **Weapon Slot System** = Player'daki WeaponSlotSystem'i sürükle

---

## 🎮 Kullanım

### Oyun İçinde:
- **1** tuşuna bas → Stick ekiple
- **2** tuşuna bas → Gun ekiple
- **3** tuşuna bas → Slot 3 silahı (eğer atanmışsa)

### Gun ile Ateş Etme:
- **Mouse Left Click** → Ateş et
- Crosshair merkezine nişan alır
- Enemy'lere hasar verir

### Inventory'de Slot Ataması:
1. **TAB** (veya Inventory tuşu) ile inventory'yi aç
2. Sağ taraftaki **Weapon Slots** dropdown'larını kullan
3. Her slot'a istediğin silahı ata
4. Ayarlar otomatik kaydedilir (PlayerPrefs)

---

## 🔧 Sorun Giderme

### Gun Ateş Etmiyor
✅ **GunWeapon.cs > Fire Point** atanmış mı?
✅ Gun GameObject'i **SetActive true** olmalı (slot değiştirince otomatik açılır)
✅ **Camera.main** var mı? (raycast için gerekli)

### Stick Combo Çalışmıyor
✅ **MeleeWeapon.cs** component'i Stick'e eklendi mi?
✅ **WeaponHitDetector** Stick'te var mı?
✅ ComboSystem sadece **MeleeWeapon** aktifken çalışır (Gun aktifken çalışmaz)

### Slot Değiştirme Çalışmıyor
✅ **WeaponSlotSystem** Player'da mı?
✅ **Weapon Slot 1/2** referansları atanmış mı?
✅ Silahlar **Player'ın child'ı** olmalı (aynı hierarchy'de)

### Dropdown'lar Boş
✅ **WeaponSlotUI > Available Weapons** array'i dolduruldu mu?
✅ Stick ve Gun **IWeapon** component'lerine sahip mi?

---

## 🚀 Gelecek Geliştirmeler

### Yeni Silah Eklemek İçin:
1. Yeni silah GameObject'i oluştur
2. `IWeapon` interface'ini implement et:
   - `Equip()`, `Unequip()`, `Use()` fonksiyonları
3. Player'a child olarak ekle
4. WeaponSlotUI > Available Weapons array'ine ekle
5. Dropdown'dan seçilebilir!

### Örnek Yeni Silah Türleri:
- **Shotgun** (yakın menzil, geniş alan)
- **Sniper** (uzun menzil, yüksek hasar)
- **Grenade** (patlayıcı, alan hasarı)
- **Bow** (sessiz, menzilli)

---

## 📝 Script Açıklamaları

### IWeapon.cs
Interface - Tüm silahlar için ortak arayüz

### WeaponSlotSystem.cs
Slot yönetimi - 1,2,3 tuşları ile silah değiştirme

### MeleeWeapon.cs
Stick gibi yakın dövüş silahları için wrapper

### GunWeapon.cs
Ateşli silah - Raycast ile hasar verme

### WeaponSlotUI.cs
Inventory UI - Dropdown ile slot ataması

---

## ✅ Test Adımları

1. ✅ Play Mode'a geç
2. ✅ **1** tuşuna bas → Stick görünür olmalı
3. ✅ **2** tuşuna bas → Gun görünür olmalı, Stick gizlenmeli
4. ✅ Gun ile Mouse Left Click → Ateş etmeli
5. ✅ **1** tuşuna bas → Tekrar Stick'e dön
6. ✅ Stick ile Mouse Left Click → Combo sistemi çalışmalı
7. ✅ TAB ile Inventory aç → Weapon Slots dropdown'ları görünür olmalı
8. ✅ Dropdown'dan slot'ları değiştir → Oyunda yansımalı

---

## 🎉 Tamamlandı!

Silah slot sisteminiz hazır! Artık:
- **1, 2, 3** tuşları ile silah değiştirebilirsiniz
- **Inventory'den** slot ataması yapabilirsiniz
- **Gun** ile Enemy'lere uzaktan hasar verebilirsiniz
- **Yeni silahlar** ekleyebilirsiniz (IWeapon interface'i)

İyi oyunlar! 🚀
