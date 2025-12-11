using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Render kalite ayarlarını kontrol eden editor tool
/// Bulanıklık sorunlarını tespit etmek için kullanılır
/// </summary>
public class RenderQualityChecker : EditorWindow
{
    [MenuItem("Tools/Render Quality Checker")]
    public static void ShowWindow()
    {
        GetWindow<RenderQualityChecker>("Render Quality Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Render Quality Settings Checker", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // URP Asset Kontrolü
        GUILayout.Label("URP Render Pipeline Asset", EditorStyles.boldLabel);
        
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            // Render Scale
            float renderScale = urpAsset.renderScale;
            EditorGUILayout.LabelField("Render Scale:", renderScale.ToString("F2"));
            if (renderScale < 1.0f)
            {
                EditorGUILayout.HelpBox($"⚠️ Render Scale {renderScale} - Bu bulanıklığa neden olabilir! 1.0 olmalı.", MessageType.Warning);
                if (GUILayout.Button("Render Scale'i 1.0 Yap"))
                {
                    SerializedObject so = new SerializedObject(urpAsset);
                    so.FindProperty("m_RenderScale").floatValue = 1.0f;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                    Debug.Log("Render Scale 1.0 olarak ayarlandı!");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Render Scale 1.0 - İyi!", MessageType.Info);
            }

            // MSAA
            int msaa = urpAsset.msaaSampleCount;
            EditorGUILayout.LabelField("MSAA:", msaa.ToString() + "x");
            if (msaa < 2)
            {
                EditorGUILayout.HelpBox($"⚠️ MSAA {msaa}x - FXAA veya AA yok. MSAA 4x önerilir.", MessageType.Warning);
                if (GUILayout.Button("MSAA'yi 4x Yap"))
                {
                    SerializedObject so = new SerializedObject(urpAsset);
                    so.FindProperty("m_MSAA").intValue = 4;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                    Debug.Log("MSAA 4x olarak ayarlandı!");
                }
            }
            else if (msaa >= 4)
            {
                EditorGUILayout.HelpBox("✅ MSAA " + msaa + "x - İyi!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("ℹ️ MSAA " + msaa + "x - İyi ama 4x daha keskin olabilir.", MessageType.Info);
            }

            // HDR
            bool hdr = urpAsset.supportsHDR;
            EditorGUILayout.LabelField("HDR:", hdr ? "Enabled" : "Disabled");
            if (!hdr)
            {
                EditorGUILayout.HelpBox("⚠️ HDR kapalı - Renk aralığı sınırlı olabilir.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ HDR Enabled - İyi!", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("❌ URP Asset bulunamadı!", MessageType.Error);
        }

        GUILayout.Space(20);

        // Quality Settings Kontrolü
        GUILayout.Label("Quality Settings", EditorStyles.boldLabel);
        
        int currentQuality = QualitySettings.GetQualityLevel();
        string qualityName = QualitySettings.names[currentQuality];
        EditorGUILayout.LabelField("Current Quality Level:", qualityName);

        // Texture Quality
        int textureMipmapLimit = QualitySettings.globalTextureMipmapLimit;
        EditorGUILayout.LabelField("Global Texture Mipmap Limit:", textureMipmapLimit.ToString());
        if (textureMipmapLimit > 0)
        {
            EditorGUILayout.HelpBox($"⚠️ Texture Mipmap Limit {textureMipmapLimit} - Texture'lar düşük çözünürlükte! 0 (Full Res) olmalı.", MessageType.Warning);
            if (GUILayout.Button("Texture Mipmap Limit'i 0 Yap"))
            {
                QualitySettings.globalTextureMipmapLimit = 0;
                Debug.Log("Texture Mipmap Limit 0 olarak ayarlandı!");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✅ Texture Mipmap Limit 0 (Full Res) - İyi!", MessageType.Info);
        }

        // Anti-Aliasing (Quality Settings)
        int qualityAA = QualitySettings.antiAliasing;
        EditorGUILayout.LabelField("Quality Settings AA:", qualityAA == 0 ? "Disabled" : qualityAA.ToString() + "x");
        if (qualityAA > 0)
        {
            EditorGUILayout.HelpBox("ℹ️ Quality Settings'te AA aktif - URP Asset'teki ayarı kullanmak daha iyi olabilir.", MessageType.Info);
        }

        GUILayout.Space(20);

        // Post-Processing Kontrolü
        GUILayout.Label("Post-Processing Volume Profile", EditorStyles.boldLabel);
        
        // Volume Profile'i SerializedObject ile kontrol et
        if (urpAsset != null)
        {
            SerializedObject urpSo = new SerializedObject(urpAsset);
            SerializedProperty volumeProfileProp = urpSo.FindProperty("m_VolumeProfile");
            
            if (volumeProfileProp != null && volumeProfileProp.objectReferenceValue != null)
            {
                VolumeProfile profile = volumeProfileProp.objectReferenceValue as VolumeProfile;
                if (profile != null)
                {
                    EditorGUILayout.LabelField("Default Volume Profile:", profile.name);
                    
                    if (GUILayout.Button("Volume Profile'i Inspector'da Aç"))
                    {
                        Selection.activeObject = profile;
                        EditorGUIUtility.PingObject(profile);
                    }
                }
            }

            EditorGUILayout.HelpBox("Volume Profile'de şunları kontrol edin:\n" +
                "• Depth of Field: Kapat veya çok düşük yap\n" +
                "• Motion Blur: Kapat\n" +
                "• Chromatic Aberration: 0-0.2 arası\n" +
                "• Bloom Intensity: 0.5-1.0 arası\n\n" +
                "Not: Scene'deki Volume component'lerini de kontrol edin!", MessageType.Info);
        }

        GUILayout.Space(20);

        // Game View Kontrolü - EN ÖNEMLİ!
        GUILayout.Label("⚠️ Game View Settings (Scene Net Ama Game Bulanıksa)", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("🔴 Scene View net ama Game View bulanıksa, bu %99 Game View ayarlarından kaynaklanıyor!", MessageType.Error);
        
        EditorGUILayout.HelpBox("Game View'da MANUEL olarak şunları kontrol edin:\n\n" +
            "1️⃣ Game View penceresini açın (Window > General > Game)\n\n" +
            "2️⃣ Sağ üstteki SCALE SLIDER'ı kontrol edin:\n" +
            "   • 1x (100%) olmalı\n" +
            "   • 0.5x, 0.75x gibi düşük değerler bulanıklık yaratır\n" +
            "   • Scale slider'ı 1x'e çekin\n\n" +
            "3️⃣ Game View dropdown menüsünü açın (sağ üstte, resolution yanında):\n" +
            "   • 'Low Resolution Aspect Ratios' KAPALI olmalı (işaretli değilse)\n" +
            "   • 'Free Aspect' yerine sabit bir resolution seçin\n" +
            "   • Örn: 1920x1080, 2560x1440 gibi\n\n" +
            "4️⃣ Game View çözünürlüğünü kontrol edin:\n" +
            "   • En az 1920x1080 olmalı\n" +
            "   • Çok düşük resolution (640x480, 800x600) bulanık görünür", MessageType.Warning);
        
        if (GUILayout.Button("Game View'u Aç", GUILayout.Height(25)))
        {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
        
        GUILayout.Space(10);
        
        // Camera Kontrolü
        GUILayout.Label("Camera Settings", EditorStyles.boldLabel);
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            EditorGUILayout.LabelField("Main Camera:", mainCamera.name);
            
            // Render Texture kontrolü
            if (mainCamera.targetTexture != null)
            {
                EditorGUILayout.HelpBox($"⚠️ Camera bir Render Texture kullanıyor! ({mainCamera.targetTexture.width}x{mainCamera.targetTexture.height})\n" +
                    "Bu bulanıklığa neden olabilir. Render Texture çözünürlüğünü kontrol edin.", MessageType.Warning);
                
                if (GUILayout.Button("Render Texture'ı Inspector'da Aç"))
                {
                    Selection.activeObject = mainCamera.targetTexture;
                    EditorGUIUtility.PingObject(mainCamera.targetTexture);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Camera Screen'e render ediyor - İyi!", MessageType.Info);
            }
            
            // Camera çözünürlüğü bilgisi
            EditorGUILayout.LabelField("Camera Pixel Rect:", $"{mainCamera.pixelWidth}x{mainCamera.pixelHeight}");
            
            if (GUILayout.Button("Camera'yı Inspector'da Aç"))
            {
                Selection.activeObject = mainCamera.gameObject;
                EditorGUIUtility.PingObject(mainCamera.gameObject);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ Main Camera bulunamadı!", MessageType.Warning);
        }

        GUILayout.Space(20);

        // Özet
        GUILayout.Label("Hızlı Düzeltme", EditorStyles.boldLabel);
        if (GUILayout.Button("Tüm Ayarları Optimal Yap", GUILayout.Height(30)))
        {
            ApplyOptimalSettings();
        }
    }

    private void ApplyOptimalSettings()
    {
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            SerializedObject so = new SerializedObject(urpAsset);
            
            // Render Scale = 1.0
            so.FindProperty("m_RenderScale").floatValue = 1.0f;
            
            // MSAA = 4x
            so.FindProperty("m_MSAA").intValue = 4;
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urpAsset);
        }

        // Texture Quality = Full Res
        QualitySettings.globalTextureMipmapLimit = 0;

        Debug.Log("✅ Tüm ayarlar optimal değerlere ayarlandı!");
        EditorUtility.DisplayDialog("Tamamlandı", 
            "Ayarlar güncellendi:\n" +
            "• Render Scale: 1.0\n" +
            "• MSAA: 4x\n" +
            "• Texture Mipmap Limit: 0 (Full Res)", 
            "OK");
    }
}

