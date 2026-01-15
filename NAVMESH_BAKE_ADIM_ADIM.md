# NavMesh Bake - Adım Adım Rehber

## Adım 1: Navigation Window'u Açın

1. Unity Editor'da üst menüden **Window** > **AI** > **Navigation** seçin
2. Navigation penceresi açılacak (genelde Inspector'un yanında veya altında)

## Adım 2: Zemin Objelerini Seçin

1. **Hierarchy** penceresinde zemin/yer GameObject'ini seçin
   - Örnek: "Ground", "Floor", "Plane", "Terrain" gibi isimler olabilir
   - Eğer birden fazla zemin objesi varsa, hepsini seçin (Ctrl/Cmd + Click)

2. **Navigation** penceresinde **Object** tab'ına gidin (Navigation penceresinin altında)

3. **Navigation Static** checkbox'ını işaretleyin
   - Bu, Unity'ye bu objenin NavMesh için kullanılacağını söyler

## Adım 3: Navigation Ayarlarını Kontrol Edin

1. **Navigation** penceresinde **Bake** tab'ına gidin

2. Ayarları kontrol edin (genelde bu değerler iyi çalışır):
   - **Agent Radius**: 0.5
   - **Agent Height**: 2
   - **Max Slope**: 45
   - **Step Height**: 0.4

## Adım 4: NavMesh Bake Edin

1. **Navigation** penceresinde **Bake** tab'ında
2. Sağ altta **Bake** butonuna tıklayın
3. Unity NavMesh'i bake edecek (birkaç saniye sürebilir)
4. Bake tamamlandığında **Scene view'da mavi bir mesh** göreceksiniz (NavMesh)

## Adım 5: NavMesh'i Görüntüleme

1. **Scene view**'da üst menüden **Gizmos** açık olmalı
2. **Navigation** penceresinde **Show NavMesh** işaretli olmalı
3. Mavi NavMesh mesh'ini görmelisiniz

## Sorun Giderme

### NavMesh görünmüyor:
- **Scene view**'da **Gizmos** açık mı kontrol edin
- **Navigation** penceresinde **Show NavMesh** işaretli mi kontrol edin
- Zemin objeleri **Navigation Static** işaretli mi kontrol edin

### Bake butonu çalışmıyor:
- Zemin objeleri seçili mi kontrol edin
- Zemin objeleri **Navigation Static** işaretli mi kontrol edin
- Unity Editor'ı yeniden başlatmayı deneyin

### Enemy NavMesh üzerinde değil:
- Enemy GameObject'ini **Scene view**'da seçin
- Enemy'yi **mavi NavMesh üzerine** sürükleyin
- Enemy'nin ayakları NavMesh'e değmeli

## Hızlı Test

1. **Scene view**'da mavi NavMesh mesh'ini görüyor musunuz?
   - Evet → NavMesh başarıyla bake edilmiş
   - Hayır → Adım 2-4'ü tekrar kontrol edin

2. Enemy GameObject'ini seçin ve **Scene view**'da mavi NavMesh üzerine yerleştirin

3. **Play** moduna geçin ve Enemy'nin hareket ettiğini kontrol edin

## Alternatif: NavMesh Olmadan Test

Eğer NavMesh bake edemiyorsanız, EnemyAIController'ı geçici olarak devre dışı bırakıp eski Enemy.cs script'ini kullanabilirsiniz. Ancak NavMesh kullanmak daha iyi sonuçlar verir.
