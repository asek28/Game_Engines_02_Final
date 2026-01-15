# 🎬 Player Animation System - Yeni Karakter Kurulum Rehberi

## 📋 Animator Controller Parametreleri

Yeni Player karakteriniz için animator parametreleri:

| Parametre | Tip | Ne Zaman Aktif? |
|-----------|-----|----------------|
| **isWalking** | Bool | W tuşuna basınca (yürüme) |
| **isRunning** | Bool | W + Shift tuşuna basınca (koşma) |
| **isHitting** | Bool | Boş elle sol tık (hiçbir silah yok) |
| **isStanding** | Bool | Stick ile sol tık (melee attack) |
| **isShooting** | Bool | Gun ile sol tık (ateş etme) |

---

## 🎯 Kurulum Adımları

### Adım 1: Player GameObject'i Hazırlama

1. **Yeni Player karakterinizi sahneye ekleyin**
   - Hierarchy > Drag & Drop
   - İsim: `Player` (önemli!)
   
2. **Gerekli Component'leri kontrol edin:**
   - ✅ `Animator` (Animator Controller atanmış olmalı)
   - ✅ `CharacterController` (hareket için)
   - ✅ `AudioSource` (ses efektleri için)

---

### Adım 2: Animation Scripts'i Ekleme

#### PlayerAnimationController.cs (Ana Controller)
1. **Player GameObject'ini seç**
2. **Add Component > Player Animation Controller**
3. **Animator referansını ayarla:**
   - Eğer otomatik bulunmadıysa, Animator component'ini sürükle

#### SimplePlayerMovement.cs (Zaten Var)
- Bu script zaten var (hareket için)
- Otomatik olarak `PlayerAnimationController`'a bağlanır

#### ComboSystem.cs (Stick Attack - Güncellendi)
- Bu script zaten var (Stick ile vuruş için)
- Artık `isStanding` parametresini kullanıyor

#### GunWeapon.cs (Gun Attack - Güncellendi)
- Bu script zaten var (Gun ile ateş için)
- Artık `isShooting` parametresini kullanıyor

#### PlayerUnarmedAttack.cs (Boş Elle Vuruş - YENİ)
1. **Player GameObject'ini seç**
2. **Add Component > Player Unarmed Attack**
3. **Ayarlar:**
   - **Unarmed Damage** = 2 (düşük hasar)
   - **Unarmed Range** = 2 (kısa menzil)
   - **Attack Cooldown** = 0.5
   - **Animation Duration** = 0.5
4. **Audio (Opsiyonel):**
   - **Punch Sound** = Yumruk sesi AudioClip'i

---

### Adım 3: Animator Controller Ayarları

#### Animator Controller Açma:
1. **Project'te Animator Controller'ınızı bulun**
2. **Double-click** ile açın (Animator window açılır)

#### Parametreleri Kontrol Edin:
Animator window'un sol tarafındaki **Parameters** tab'ında şunlar olmalı:

- ✅ `isWalking` (Bool)
- ✅ `isRunning` (Bool)
- ✅ `isHitting` (Bool)
- ✅ `isStanding` (Bool)
- ✅ `isShooting` (Bool)

**Eğer yoksa ekleyin:**
1. Parameters tab'ında "+" butonuna tıklayın
2. "Bool" seçin
3. İsmi yazın (örn: "isWalking")
4. Tüm parametreler için tekrarlayın

---

### Adım 4: Animator Transitions (Geçişler)

Her parametreyi ilgili animasyona bağlayın:

#### Entry → Breathing Idle (Başlangıç)
- **Conditions**: Yok (varsayılan)

#### Any State → Walking
- **Conditions**: `isWalking == true`
- **Transition Duration**: 0.1-0.2 (yumuşak geçiş)
- **Exit Time**: Unchecked (hemen geçiş)

#### Any State → Run Forward
- **Conditions**: `isRunning == true`
- **Transition Duration**: 0.1-0.2
- **Exit Time**: Unchecked

#### Any State → Punching
- **Conditions**: `isHitting == true`
- **Transition Duration**: 0.1
- **Exit Time**: Unchecked

#### Any State → Standing (veya Stick Attack)
- **Conditions**: `isStanding == true`
- **Transition Duration**: 0.1
- **Exit Time**: Unchecked

#### Any State → Gunplay (veya Shooting)
- **Conditions**: `isShooting == true`
- **Transition Duration**: 0.1
- **Exit Time**: Unchecked

#### Geri Dönüş (Idle'a):
Her animasyondan **Breathing Idle**'a geri dönüş:
- **Conditions**: İlgili parametre `== false`
- **Has Exit Time**: Checked (animasyon bitince dön)
- **Exit Time**: 0.8-0.9 (animasyonun %80-90'ı)

---

## 🎮 Kullanım

### Oyun İçinde Test:
1. **Play Mode'a geçin**
2. **W** tuşuna basın → Walking animasyonu çalmalı
3. **W + Shift** basın → Run Forward animasyonu çalmalı
4. **Hiçbir silah yokken Sol Tık** → Punching animasyonu (isHitting)
5. **Stick aktifken Sol Tık** → Standing/Attack animasyonu (isStanding)
6. **Gun aktifken Sol Tık** → Gunplay/Shooting animasyonu (isShooting)

### Animator Window'da İzleme:
1. **Play Mode** açıkken **Animator window**'u açık tutun
2. Parametrelerin gerçek zamanlı değişimini göreceksiniz
3. Geçişlerin doğru çalıştığını kontrol edin

---

## 🔧 Sorun Giderme

### Animasyonlar Çalışmıyor
✅ **PlayerAnimationController** component'i Player'da mı?
✅ **Animator** referansı atanmış mı?
✅ Animator Controller'da **parametreler** var mı?
✅ Animator **transitions** doğru kurulmuş mu?

### Walking/Running Çalışmıyor
✅ **SimplePlayerMovement** component'i var mı?
✅ `isWalking` ve `isRunning` parametreleri Animator'da mı?
✅ Console'da hata var mı?

### isStanding (Stick Attack) Çalışmıyor
✅ **ComboSystem** component'i var mı?
✅ **WeaponSlotSystem** component'i var mı?
✅ Stick **Slot 1**'e atanmış mı?
✅ **MeleeWeapon** component'i Stick'te mi?

### isShooting (Gun Attack) Çalışmıyor
✅ **GunWeapon** component'i Gun'da mı?
✅ **WeaponSlotSystem** component'i var mı?
✅ Gun **Slot 2**'ye atanmış mı?

### isHitting (Boş Elle) Çalışmıyor
✅ **PlayerUnarmedAttack** component'i Player'da mı?
✅ **Hiçbir silah aktif değil** mi? (tüm slotlar boş veya unequip)
✅ `isHitting` parametresi Animator'da mı?

---

## 📊 Console Debug Mesajları

### Başarılı Animasyonlar:
```
[PlayerAnimationController] isWalking = true
[PlayerAnimationController] isRunning = true
[PlayerAnimationController] isHitting = true
[PlayerAnimationController] isStanding = true
[PlayerAnimationController] isShooting = true
```

### Animasyon Sorunları:
```
⚠️ [PlayerAnimationController] Animator parameter 'isWalking' not found!
```
→ Animator Controller'da parametre eksik, ekleyin!

---

## 🎨 İleri Seviye: Animasyon Blending

### Hareket Yönü (Strafe):
Eğer A/D tuşları ile yana hareket ediyorsanız, blend tree kullanabilirsiniz:
1. Animator'da **Blend Tree** oluşturun
2. Forward, Left, Right, Backward animasyonları ekleyin
3. `SimplePlayerMovement`'dan yön bilgisi alın

### Smooth Transitions:
- **Transition Duration** değerlerini ayarlayın (0.1-0.3)
- **Transition Offset** ile başlangıç noktası ayarlayın
- **Interruption Source** ile geçiş sırasında diğer animasyona geçişe izin verin

---

## ✅ Kontrol Listesi

### Player GameObject:
- [ ] PlayerAnimationController.cs eklendi
- [ ] SimplePlayerMovement.cs var
- [ ] ComboSystem.cs var
- [ ] PlayerUnarmedAttack.cs eklendi
- [ ] WeaponSlotSystem.cs var
- [ ] Animator component'i atanmış

### Animator Controller:
- [ ] isWalking parametresi var
- [ ] isRunning parametresi var
- [ ] isHitting parametresi var
- [ ] isStanding parametresi var
- [ ] isShooting parametresi var
- [ ] Tüm transition'lar kurulmuş

### Weapon System:
- [ ] Stick'e MeleeWeapon.cs eklendi
- [ ] Gun'a GunWeapon.cs eklendi
- [ ] WeaponSlotSystem slot'ları atanmış

### Test:
- [ ] W tuşu → Walking
- [ ] W+Shift → Running
- [ ] Boş elle sol tık → Punching
- [ ] Stick ile sol tık → Standing
- [ ] Gun ile sol tık → Shooting

---

## 🎉 Tamamlandı!

Yeni Player karakterinizin animasyon sistemi hazır!

### Özellikler:
- ✅ **isWalking**: Yürüme (W)
- ✅ **isRunning**: Koşma (W+Shift)
- ✅ **isHitting**: Boş elle vuruş (silah yok)
- ✅ **isStanding**: Stick ile vuruş (melee)
- ✅ **isShooting**: Gun ile ateş (ranged)

### Sistem Avantajları:
- 🔄 **Otomatik Geçişler** (isWalking/isRunning)
- 🎯 **Silah Bazlı Animasyonlar** (isStanding/isShooting)
- 👊 **Boş Elle Savaş** (isHitting)
- 🎨 **Genişletilebilir** (yeni silah türleri eklenebilir)

Artık karakteriniz tüm hareketleri ve saldırıları doğru animasyonlarla yapacak! 🚀

---

## 💡 Ek Notlar

### ComboCount Sistemi (Eski Sistem):
Eğer eski `ComboCount` parametreniz hala varsa:
- `isStanding` ile birlikte kullanılabilir
- ComboSystem hem `isStanding` hem de `ComboCount` set ediyor
- İkisini birlikte kullanarak combo animasyonları yapabilirsiniz

### Animasyon Öncelikleri:
Animator'da şu sırayla öncelik verin:
1. **isShooting** / **isStanding** / **isHitting** (saldırılar)
2. **isRunning** (koşma)
3. **isWalking** (yürüme)
4. **Idle** (varsayılan)

Bu sayede saldırı animasyonları hareket animasyonlarını kesebilir.

İyi oyunlar! 🎮
