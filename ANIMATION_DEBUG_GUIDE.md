# 🐛 Animation Debug Rehberi - isRunning ve isStanding Sorunları

## 🔍 Console Mesajlarını Kontrol Edin

Debug log'ları eklendi! Play Mode'da Console'u açın ve şu mesajları arayın.

---

## ✅ Başlangıç Mesajları (Awake/Start)

Play Mode'a geçtiğinizde şu mesajları **görmelisiniz**:

```
[PlayerAnimationController] Animator found: Player
[PlayerAnimationController] SimplePlayerMovement found!
[PlayerAnimationController] WeaponSlotSystem found!
[ComboSystem] PlayerAnimationController found!
```

### ❌ Eğer Bu Mesajları Görmüyorsanız:
- **PlayerAnimationController not found** → Player GameObject'e `PlayerAnimationController.cs` ekleyin!
- **SimplePlayerMovement not found** → Player GameObject'e `SimplePlayerMovement.cs` ekleyin!
- **WeaponSlotSystem not found** → Player GameObject'e `WeaponSlotSystem.cs` ekleyin!

---

## 🏃 isRunning Sorunu - Test Adımları

### Test 1: W + Shift Basın (Koşma)
1. Play Mode
2. **W + Shift** tuşlarına basın
3. Console'da şu mesajları **arayin**:

**Görülmesi Gereken:**
```
[SimplePlayerMovement] isRunning = true (sprintPressed: True, move.sqrMagnitude: 1.00)
[PlayerAnimationController] Movement - shouldWalk: true, shouldRun: true
[PlayerAnimationController] isRunning = true
```

### ❌ Problem: isRunning = false Kalıyorsa

#### Senaryo A: SimplePlayerMovement mesajı yok
```
[SimplePlayerMovement] isRunning = true
```
**Çözüm:**
- ✅ **Shift** tuşuna **basılı tuttuğunuzdan** emin olun (Caps Lock değil!)
- ✅ `SimplePlayerMovement.cs` script'inin Player'da olduğunu kontrol edin
- ✅ Keyboard.current null değil mi kontrol edin

#### Senaryo B: PlayerAnimationController mesajı yok
```
[PlayerAnimationController] isRunning = true
```
**Çözüm:**
- ✅ `PlayerAnimationController.cs` script'inin Player'da olduğunu kontrol edin
- ✅ Inspector'da `SimplePlayerMovement` referansının atandığını kontrol edin

#### Senaryo C: Animator parameter bulunamıyor
```
⚠️ [PlayerAnimationController] Animator parameter 'isRunning' not found!
```
**Çözüm:**
- ✅ Animator Controller'ı açın (Project > Double-click)
- ✅ **Parameters** tab'ında `isRunning` (Bool) olduğunu kontrol edin
- ✅ Yoksa ekleyin: Parameters > "+" > Bool > İsim: "isRunning"

---

## 🥊 isStanding Sorunu - Test Adımları

### Test 1: Stick İle Vuruş
1. Play Mode
2. **1** tuşuna basın (Stick ekiple)
3. **Sol Tık** yapın
4. Console'da şu mesajları **arayın**:

**Görülmesi Gereken:**
```
[ComboSystem] MeleeWeapon active, attack pressed!
[ComboSystem] Attack pressed! Combo Count: 1, Is Attacking: False
[ComboSystem] Called SetStanding(true)
[PlayerAnimationController] isStanding = true
```

### ❌ Problem: isStanding Çalışmıyorsa

#### Senaryo A: ComboSystem çalışmıyor
```
[ComboSystem] MeleeWeapon active, attack pressed!
```
**Çözüm:**
- ✅ `ComboSystem.cs` script'inin Player'da olduğunu kontrol edin
- ✅ **1** tuşuna basıp Stick'in aktif olduğunu kontrol edin
- ✅ Stick GameObject'inde `MeleeWeapon.cs` component'i olduğunu kontrol edin

#### Senaryo B: PlayerAnimationController bulunamıyor
```
⚠️ [ComboSystem] PlayerAnimationController is NULL! Can't set isStanding!
```
**Çözüm:**
- ✅ `PlayerAnimationController.cs` script'inin Player'da olduğunu kontrol edin
- ✅ `ComboSystem.cs` ve `PlayerAnimationController.cs` **aynı GameObject'te** olmalı (Player)

#### Senaryo C: Animator parameter bulunamıyor
```
⚠️ [PlayerAnimationController] Animator parameter 'isStanding' not found!
```
**Çözüm:**
- ✅ Animator Controller'ı açın (Project > Double-click)
- ✅ **Parameters** tab'ında `isStanding` (Bool) olduğunu kontrol edin
- ✅ Yoksa ekleyin: Parameters > "+" > Bool > İsim: "isStanding"

#### Senaryo D: WeaponSlotSystem yok
```
// Hiçbir mesaj yok - combo system çalışmıyor
```
**Çözüm:**
- ✅ `WeaponSlotSystem.cs` script'inin Player'da olduğunu kontrol edin
- ✅ WeaponSlotSystem > Weapon Slot 1 = Stick GameObject'i olduğunu kontrol edin
- ✅ Stick GameObject'inde `MeleeWeapon.cs` component'i olduğunu kontrol edin

---

## 🎯 Hızlı Kontrol Listesi

### Player GameObject'de Olması Gerekenler:
- [ ] `PlayerAnimationController.cs`
- [ ] `SimplePlayerMovement.cs`
- [ ] `ComboSystem.cs`
- [ ] `WeaponSlotSystem.cs`
- [ ] `PlayerUnarmedAttack.cs`
- [ ] `Animator` component (Controller atanmış)
- [ ] `CharacterController` component

### Animator Controller Parametreleri:
- [ ] `isWalking` (Bool)
- [ ] `isRunning` (Bool)
- [ ] `isHitting` (Bool)
- [ ] `isStanding` (Bool)
- [ ] `isShooting` (Bool)

### Stick Weapon Setup:
- [ ] Stick GameObject (Player'ın child'ı)
- [ ] `MeleeWeapon.cs` component Stick'te
- [ ] WeaponSlotSystem > Weapon Slot 1 = Stick

---

## 📊 Console Temizliği

Çok fazla log varsa:
1. Console'u temizleyin (Clear button)
2. Console sağ üst > Filter > Collapse ON
3. Console sağ üst > Filter > Info/Warning/Error butonlarını kullanın

---

## 🔧 Sorun Giderme Akış Şeması

### isRunning Çalışmıyor:
```
1. W+Shift basıyorsunuz ↓
2. Console'da "[SimplePlayerMovement] isRunning = true" var mı?
   ├─ HAYIR → SimplePlayerMovement eksik veya Shift basılmıyor
   └─ EVET → Devam et ↓
3. Console'da "[PlayerAnimationController] isRunning = true" var mı?
   ├─ HAYIR → PlayerAnimationController eksik veya SimplePlayerMovement referansı yok
   └─ EVET → Devam et ↓
4. Console'da "Animator parameter 'isRunning' not found" var mı?
   ├─ EVET → Animator Controller'a isRunning parametresini ekle
   └─ HAYIR → Animator transition'ları kontrol et (Any State → Run Forward)
```

### isStanding Çalışmıyor:
```
1. 1 tuşu + Sol Tık ↓
2. Console'da "[ComboSystem] MeleeWeapon active" var mı?
   ├─ HAYIR → Stick ekipli değil veya MeleeWeapon component yok
   └─ EVET → Devam et ↓
3. Console'da "[ComboSystem] Called SetStanding(true)" var mı?
   ├─ HAYIR → PlayerAnimationController eksik (ComboSystem tarafından bulunamıyor)
   └─ EVET → Devam et ↓
4. Console'da "[PlayerAnimationController] isStanding = true" var mı?
   ├─ HAYIR → Animator parameter 'isStanding' yok
   └─ EVET → Animator transition'ları kontrol et (Any State → Standing)
```

---

## 🎬 Animator Transition Kontrol

### Transition Ayarları (Her biri için):
1. **Animator window** açın (Project > Animator Controller double-click)
2. **Any State** → **Run Forward** transition'ını seçin
3. Inspector'da kontrol edin:

**Olması Gerekenler:**
- ✅ **Conditions**: `isRunning` `Equals` `true`
- ✅ **Has Exit Time**: UNCHECKED (hemen geçiş)
- ✅ **Transition Duration**: 0.1-0.25 (yumuşak geçiş)

**Any State → Standing için de aynı şekilde:**
- ✅ **Conditions**: `isStanding` `Equals` `true`
- ✅ **Has Exit Time**: UNCHECKED
- ✅ **Transition Duration**: 0.1

---

## 🎮 Animator Window'da Real-Time İzleme

1. **Play Mode** açıkken **Animator window**'u açık tutun
2. **Parameters** tab'ını açın
3. Şu parametrelerin **gerçek zamanlı** değişimini göreceksiniz:

| Parametre | Ne Zaman TRUE? |
|-----------|---------------|
| isWalking | W tuşu basılı |
| isRunning | W+Shift basılı |
| isStanding | 1 tuşu + Sol Tık (Stick) |

Eğer parametreler **değişmiyorsa**, Console'daki hata mesajlarına bakın!

---

## ✅ Test Senaryosu

### Adım 1: Play Mode
- Console'u açın (Ctrl+Shift+C)
- Animator window'u açın
- Scene view'u da açık tutun

### Adım 2: isRunning Testi
1. **W+Shift** basın
2. **Console'da** log'ları kontrol edin
3. **Animator window'da** `isRunning` parametresinin TRUE olduğunu görün
4. **Scene/Game view'da** Run Forward animasyonunu görün

### Adım 3: isStanding Testi
1. **1** tuşuna basın (Stick ekiple)
2. **Sol Tık** yapın
3. **Console'da** log'ları kontrol edin
4. **Animator window'da** `isStanding` parametresinin TRUE olduğunu görün
5. **Scene/Game view'da** Standing/Attack animasyonunu görün

---

## 🚨 En Yaygın Sorunlar

### 1. "PlayerAnimationController not found!"
**Çözüm:** Player GameObject'e `PlayerAnimationController.cs` ekleyin!

### 2. "Animator parameter 'isRunning' not found!"
**Çözüm:** Animator Controller'a `isRunning` (Bool) parametresi ekleyin!

### 3. "Animator parameter 'isStanding' not found!"
**Çözüm:** Animator Controller'a `isStanding` (Bool) parametresi ekleyin!

### 4. isRunning true oluyor ama animasyon çalışmıyor
**Çözüm:** Animator Transition'ları kontrol edin (Any State → Run Forward)

### 5. isStanding true oluyor ama animasyon çalışmıyor
**Çözüm:** Animator Transition'ları kontrol edin (Any State → Standing)

---

## 📝 Console'a Yapıştırın

Test sırasında Console'daki **TÜM mesajları** buraya yapıştırın:
- Başlangıç mesajları (Awake/Start)
- isRunning test mesajları (W+Shift)
- isStanding test mesajları (1 + Sol Tık)

Bu şekilde sorunu hızlıca bulabiliriz! 🎯

---

## ✨ Başarılı Test Çıktısı

Eğer her şey doğru çalışıyorsa Console'da şunları göreceksiniz:

```
// Başlangıç
[PlayerAnimationController] Animator found: Player
[PlayerAnimationController] SimplePlayerMovement found!
[PlayerAnimationController] WeaponSlotSystem found!
[ComboSystem] PlayerAnimationController found!

// W+Shift testi
[SimplePlayerMovement] isRunning = true (sprintPressed: True, move.sqrMagnitude: 1.00)
[PlayerAnimationController] Movement - shouldWalk: true, shouldRun: true
[PlayerAnimationController] isRunning = true

// 1 + Sol Tık testi
[ComboSystem] MeleeWeapon active, attack pressed!
[ComboSystem] Attack pressed! Combo Count: 1, Is Attacking: False
[ComboSystem] Called SetStanding(true)
[PlayerAnimationController] isStanding = true
```

Bu mesajları görüyorsanız, sistem çalışıyor! 🎉

---

**Sorun devam ederse Console çıktısını buraya yapıştırın!** 🔍
