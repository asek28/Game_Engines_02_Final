# Shopkeeper Basit Animator - Hızlı Kurulum

## 🎯 Hedef

Karmaşık animator yerine **basit, çalışan** bir animator oluştur.

---

## ⚡ 5 Dakikada Çözüm

### 1. Yeni Animator Controller Oluştur

1. **Project → Assets/Animation/Shopkeeper/Shopkeeper/**

2. **Right Click → Create → Animator Controller**
   - İsim: **ShopkeeperSimple**

3. **Çift tıkla** (açılır)

---

### 2. Tek State Oluştur (En Basit)

**Sadece oturma animasyonu:**

1. Animator window'da **Right Click → Create State → Empty**

2. State'e tıkla → **Inspector'da:**
   - İsim: **Sitting**
   - Motion: `Shopkeeper@Sitting` animation'ı **sürükle**

3. State'e **Right Click → Set as Layer Default State**
   - Turuncu olmalı

4. **Kaydet** (Ctrl+S)

**ÖNEMLİ:** Başka state ekleme, transition ekleme! Bu kadar basit.

---

### 3. Shopkeeper'a Ata

1. **Shop.unity** sahnesini aç

2. **Shopkeeper** objesini seç (veya Armature)

3. **Inspector → Animator component:**
   - Controller: **ShopkeeperSimple** sürükle

4. **Play'e bas → Test et**

---

## 🎨 Birden Fazla Animasyon İstersen

**3 animasyonlu versiyon (ama transition YOK!):**

### State'leri Oluştur:

1. **Sitting** (default - turuncu)
   - Motion: `Shopkeeper@Sitting`

2. **Sitting Angry**
   - Motion: `Shopkeeper@Sitting Angry`

3. **Sitting Disbelief**
   - Motion: `Shopkeeper@Sitting Disbelief`

**Transition EKLEME!** Sadece state'ler olsun.

### ShopkeeperController'ı Güncelle:

```csharp
// ShopkeeperController.cs içinde:

private void PlayRandomAnimation()
{
    if (sittingAnimationStates.Length == 0) return;
    
    int randomIndex = Random.Range(0, sittingAnimationStates.Length);
    string animName = sittingAnimationStates[randomIndex];
    
    if (animator != null)
    {
        animator.Play(animName, 0, 0f);
        Debug.Log($"[ShopkeeperController] Playing: {animName}");
    }
}
```

**Inspector'da ayarla:**
```
Sitting Animation States:
- Element 0: "Sitting"
- Element 1: "Sitting Angry"
- Element 2: "Sitting Disbelief"

Animation Switch Interval: 5
Random Switching: ✓
```

---

## ✅ Test

1. **Play'e bas**

2. **Shopkeeper'a bak:**
   - Oturma animasyonu oynuyor mu? ✓
   - T-pose yok değil mi? ✓

3. **5 saniye bekle:**
   - Animasyon değişiyor mu? ✓

---

## 🆚 Eski vs Yeni

### Eski (Bozuk):
- ❌ State isimleri "mixamo_com" (anlaşılmaz)
- ❌ 10+ transition (karmaşık)
- ❌ Condition'lar (gereksiz)
- ❌ NullReferenceException hatası

### Yeni (Basit):
- ✓ State isimleri açık (Sitting, Sitting Angry...)
- ✓ 0 transition (basit)
- ✓ Condition yok
- ✓ Hatasız

---

## 🔧 Sorun Giderme

### Animasyon oynamıyor:

1. **Shopkeeper seç → Inspector:**
   - Animator → Controller: ShopkeeperSimple mi?
   - Animator → Avatar: Var mı?

2. **ShopkeeperSimple aç:**
   - Default state turuncu mu?
   - Motion dolu mu?

3. **Play mode'da Console:**
   - "[ShopkeeperController] Playing: Sitting" görünmeli

### Hala T-pose:

1. **Shopkeeper.fbx seç:**
   - Inspector → Rig tab
   - Animation Type: **Humanoid**
   - Apply

2. **Her animasyon FBX'ini seç:**
   - `Shopkeeper@Sitting.fbx`
   - `Shopkeeper@Sitting Angry.fbx`
   - `Shopkeeper@Sitting Disbelief.fbx`
   
   Her birinde:
   - Rig → Animation Type: **Humanoid**
   - Apply

---

## 📝 Özet

1. Yeni **ShopkeeperSimple** animator oluştur
2. Tek state ekle: **Sitting**
3. Motion: `Shopkeeper@Sitting`
4. Set as Default
5. Shopkeeper'a ata
6. Test et

**Transition ekleme, condition ekleme, karmaşık yapma!**
