# NavMesh Setup Rehberi - Unity

## Adım 1: Navigation Window'u Açın

1. Unity Editor'da üst menüden **Window** > **AI** > **Navigation** seçin
2. Navigation penceresi açılacak (genelde Inspector'un yanında)

## Adım 2: Navigation Ayarlarını Yapın

### Object Tab (Navigation Window'un altında):
1. Scene'deki **zemin/yer** GameObject'ini seçin (enemy'lerin yürüyeceği yüzey)
2. Navigation penceresinde **Object** tab'ına gidin
3. **Navigation Static** checkbox'ını işaretleyin
4. **Navigation Area** dropdown'ından **Walkable** seçin (veya default bırakın)

**ÖNEMLİ:** Enemy'lerin yürümesini istediğiniz TÜM zemin objelerini seçip Navigation Static yapın!

### Areas Tab:
- Genelde değiştirmenize gerek yok
- Walkable, Not Walkable, Jump gibi alanlar var

### Bake Tab:
1. **Agent Radius**: Enemy'nin collider yarıçapı (genelde 0.5)
2. **Agent Height**: Enemy'nin yüksekliği (genelde 2)
3. **Max Slope**: Enemy'nin tırmanabileceği maksimum eğim (genelde 45 derece)
4. **Step Height**: Enemy'nin atlayabileceği yükseklik (genelde 0.4)

## Adım 3: NavMesh Bake Edin

1. Navigation penceresinde **Bake** tab'ına gidin
2. Sağ altta **Bake** butonuna tıklayın
3. Unity NavMesh'i bake edecek (birkaç saniye sürebilir)
4. Bake tamamlandığında Scene view'da mavi bir mesh göreceksiniz (NavMesh)

## Adım 4: Enemy'yi NavMesh Üzerine Yerleştirin

1. Enemy GameObject'ini seçin
2. Scene view'da Enemy'yi **mavi NavMesh üzerine** sürükleyin
3. Enemy'nin ayakları NavMesh'e değmeli

## Adım 5: NavMeshAgent Component Ayarları

Enemy GameObject'inde:
1. **NavMeshAgent** component'ini kontrol edin
2. **Agent Size** ayarları:
   - **Radius**: 0.5 (enemy'nin genişliği)
   - **Height**: 2 (enemy'nin yüksekliği)
   - **Base Offset**: 0 (genelde 0)
3. **Steering** ayarları:
   - **Speed**: 3.5 (yürüme hızı)
   - **Angular Speed**: 120 (dönüş hızı)
   - **Acceleration**: 8 (hızlanma)
   - **Stopping Distance**: 0.5 (durma mesafesi)
4. **Obstacle Avoidance**:
   - **Quality**: Medium veya High
   - **Radius**: 0.5

## Adım 6: Test Edin

1. Play moduna geçin
2. Enemy'nin hareket ettiğini kontrol edin
3. Console'da hata olmamalı

## Sorun Giderme

### NavMesh görünmüyor:
- Scene view'da **Gizmos** açık olmalı
- Navigation penceresinde **Show NavMesh** işaretli olmalı

### Enemy NavMesh üzerinde değil:
- Enemy'yi Scene view'da mavi NavMesh üzerine taşıyın
- Enemy'nin Y pozisyonunu NavMesh seviyesine ayarlayın

### Enemy hareket etmiyor:
- NavMeshAgent component'inin **enabled** olduğundan emin olun
- Enemy'nin başlangıç pozisyonu NavMesh üzerinde olmalı
- Console'da hata var mı kontrol edin

## Hızlı Ayarlar (Önerilen Değerler)

**NavMeshAgent:**
- Radius: 0.5
- Height: 2
- Speed: 3.5
- Angular Speed: 120
- Acceleration: 8
- Stopping Distance: 0.5

**Bake Settings:**
- Agent Radius: 0.5
- Agent Height: 2
- Max Slope: 45
- Step Height: 0.4
