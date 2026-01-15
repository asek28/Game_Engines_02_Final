using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Animasyonları import ederken otomatik olarak loop ayarlarını düzeltir
/// Build'de sorun olmaması için kalıcı çözüm
/// </summary>
public class AnimationLoopAutoFixer : AssetPostprocessor
{
    // Loop olması gereken animasyon isimleri (küçük harf)
    private static readonly HashSet<string> loopKeywords = new HashSet<string>
    {
        "walk", "walking", "run", "running", "idle", "standing",
        "move", "moving", "wounded", "damaged", "dameged",
        "walking_damaged", "walking_dameged", "walk_damaged", "walk_dameged",
        "stand", "wait", "breath", "breathe"
    };

    // Loop OLMAMASI gereken animasyon isimleri (küçük harf)
    private static readonly HashSet<string> noLoopKeywords = new HashSet<string>
    {
        "death", "die", "dying", "dead",
        "attack", "attacking", "hit", "hitting",
        "damage", // "damaged" değil, sadece "damage" (impact)
        "loot", "collect", "pickup",
        "react", "combo", "shoot", "fire",
        "jump", "land", "fall"
    };

    /// <summary>
    /// Model (FBX) import edildiğinde otomatik çalışır
    /// </summary>
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        if (modelImporter == null) return;

        // Sadece animasyon içeren FBX dosyalarını işle
        if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            return;

        Debug.Log($"[AnimationLoopAutoFixer] Preprocessing model: {assetPath}");
    }

    /// <summary>
    /// Model import edildikten SONRA animasyonları düzelt
    /// </summary>
    void OnPostprocessModel(GameObject go)
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        if (modelImporter == null) return;

        // AnimationClip'leri al
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;
        if (clipAnimations == null || clipAnimations.Length == 0)
            return;

        bool madeChanges = false;

        foreach (ModelImporterClipAnimation clip in clipAnimations)
        {
            string clipName = clip.name.ToLower();
            bool shouldLoop = ShouldAnimationLoop(clipName);
            
            // Eğer loop ayarı yanlışsa düzelt
            if (clip.loopTime != shouldLoop)
            {
                clip.loopTime = shouldLoop;
                madeChanges = true;
                
                Debug.Log($"[AnimationLoopAutoFixer] Fixed '{clip.name}': Loop = {shouldLoop}");
            }
        }

        // Değişiklik yapıldıysa kaydet
        if (madeChanges)
        {
            modelImporter.clipAnimations = clipAnimations;
            modelImporter.SaveAndReimport();
            
            Debug.Log($"<color=green>[AnimationLoopAutoFixer] ✅ Fixed animations in: {assetPath}</color>");
        }
    }

    /// <summary>
    /// Animasyon ismine göre loop olup olmayacağını belirle
    /// </summary>
    private bool ShouldAnimationLoop(string clipName)
    {
        clipName = clipName.ToLower();

        // Önce "loop olmamalı" kontrolü yap
        foreach (string keyword in noLoopKeywords)
        {
            if (clipName.Contains(keyword))
            {
                // Özel durum: "damaged" içeriyorsa ama "damage" kelimesi sadece başında değilse loop olsun
                if (keyword == "damage" && (clipName.Contains("damaged") || clipName.Contains("dameged")))
                {
                    continue; // Bu durumda loop olabilir
                }
                
                return false; // Loop olmamalı
            }
        }

        // Sonra "loop olmalı" kontrolü yap
        foreach (string keyword in loopKeywords)
        {
            if (clipName.Contains(keyword))
            {
                return true; // Loop olmalı
            }
        }

        // Varsayılan: Bilinmeyen animasyonlar loop olmasın
        return false;
    }
}

/// <summary>
/// Var olan tüm animasyonları tek seferde düzeltmek için Editor Window
/// </summary>
public class AnimationLoopBatchFixer : EditorWindow
{
    private string targetFolder = "Assets/Animation";
    private bool fixPlayerAnimations = true;
    private bool fixEnemyAnimations = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Animation/Batch Fix All Animations Loop")]
    public static void ShowWindow()
    {
        GetWindow<AnimationLoopBatchFixer>("Animation Loop Batch Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Animation Loop Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Bu tool TÜM animasyonları tek seferde düzeltir.\n" +
            "Yeni eklenen animasyonlar otomatik düzeltilir (AnimationLoopAutoFixer sayesinde).",
            MessageType.Info
        );

        GUILayout.Space(10);

        targetFolder = EditorGUILayout.TextField("Target Folder:", targetFolder);
        fixPlayerAnimations = EditorGUILayout.Toggle("Fix Player Animations", fixPlayerAnimations);
        fixEnemyAnimations = EditorGUILayout.Toggle("Fix Enemy Animations", fixEnemyAnimations);

        GUILayout.Space(20);

        if (GUILayout.Button("🔧 FIX ALL ANIMATIONS NOW", GUILayout.Height(40)))
        {
            FixAllAnimations();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("📋 List All FBX Files"))
        {
            ListAllFBXFiles();
        }
    }

    private void FixAllAnimations()
    {
        if (!EditorUtility.DisplayDialog(
            "Confirm Batch Fix",
            $"Bu işlem '{targetFolder}' klasöründeki TÜM animasyonları düzeltecek.\n\nDevam edilsin mi?",
            "Evet, Düzelt",
            "İptal"))
        {
            return;
        }

        int fixedCount = 0;
        int totalCount = 0;

        // Tüm FBX dosyalarını bul
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { targetFolder });

        EditorUtility.DisplayProgressBar("Fixing Animations", "Processing...", 0f);

        try
        {
            for (int i = 0; i < fbxGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fbxGuids[i]);
                
                EditorUtility.DisplayProgressBar(
                    "Fixing Animations",
                    $"Processing: {System.IO.Path.GetFileName(assetPath)} ({i + 1}/{fbxGuids.Length})",
                    (float)i / fbxGuids.Length
                );

                if (FixFBXAnimations(assetPath))
                {
                    fixedCount++;
                }
                totalCount++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Batch Fix Complete",
            $"✅ İşlem tamamlandı!\n\nToplam: {totalCount} FBX\nDüzeltilen: {fixedCount} FBX",
            "Tamam"
        );

        Debug.Log($"<color=green>[AnimationLoopBatchFixer] ✅ Fixed {fixedCount}/{totalCount} FBX files!</color>");
    }

    private bool FixFBXAnimations(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null) return false;

        ModelImporterClipAnimation[] clipAnimations = importer.defaultClipAnimations;
        if (clipAnimations == null || clipAnimations.Length == 0) return false;

        bool madeChanges = false;

        foreach (ModelImporterClipAnimation clip in clipAnimations)
        {
            string clipName = clip.name.ToLower();
            bool shouldLoop = ShouldAnimationLoop(clipName);

            if (clip.loopTime != shouldLoop)
            {
                clip.loopTime = shouldLoop;
                madeChanges = true;

                Debug.Log($"[Batch] Fixed '{clip.name}' in {assetPath}: Loop = {shouldLoop}");
            }
        }

        if (madeChanges)
        {
            importer.clipAnimations = clipAnimations;
            importer.SaveAndReimport();
        }

        return madeChanges;
    }

    private bool ShouldAnimationLoop(string clipName)
    {
        clipName = clipName.ToLower();

        // Loop olmamalı
        if (clipName.Contains("death") || clipName.Contains("die") ||
            clipName.Contains("attack") || clipName.Contains("hit") ||
            clipName.Contains("loot") || clipName.Contains("collect") ||
            clipName.Contains("react") || clipName.Contains("combo") ||
            clipName.Contains("shoot") || clipName.Contains("fire") ||
            clipName.Contains("jump") || clipName.Contains("land"))
        {
            // Özel durum: "damage" vs "damaged"
            if ((clipName.Contains("damage") && !clipName.Contains("damaged") && !clipName.Contains("dameged")))
            {
                return false;
            }
        }

        // Loop olmalı
        if (clipName.Contains("walk") || clipName.Contains("idle") || clipName.Contains("run") ||
            clipName.Contains("wounded") || clipName.Contains("damaged") || clipName.Contains("dameged") ||
            clipName.Contains("standing") || clipName.Contains("move") || clipName.Contains("stand") ||
            clipName.Contains("breath"))
        {
            return true;
        }

        return false;
    }

    private void ListAllFBXFiles()
    {
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { targetFolder });
        
        Debug.Log($"<color=cyan>========== FBX Files in '{targetFolder}' ==========</color>");
        
        foreach (string guid in fbxGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"  📁 {assetPath}");
        }
        
        Debug.Log($"<color=cyan>========== Total: {fbxGuids.Length} FBX files ==========</color>");
    }
}
