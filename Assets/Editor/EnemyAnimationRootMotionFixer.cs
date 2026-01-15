using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Enemy animasyonlarının root motion ayarlarını düzenler
/// - Root Transform Position'ı devre dışı bırakır (ışınlanma sorununu çözer)
/// - Animasyonların pozisyon değiştirmesini önler
/// NOT: Root motion ayarları AnimationClip import settings'inde değil, Animator component'inde kontrol edilir.
/// Bu script sadece bilgilendirme amaçlıdır. Root motion EnemyAIController.cs'de zaten devre dışı bırakıldı.
/// </summary>
public class EnemyAnimationRootMotionFixer : EditorWindow
{
    private AnimatorController animatorController;
    private AnimationClip[] animationClips;
    
    [MenuItem("Tools/Fix Enemy Animation Root Motion")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAnimationRootMotionFixer>("Enemy Root Motion Fixer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enemy Animation Root Motion Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "IMPORTANT: Root motion is already disabled in EnemyAIController.cs!\n" +
            "The script sets animator.applyRootMotion = false in Awake().\n\n" +
            "If you still experience teleporting issues, check:\n" +
            "1. Animator component's 'Apply Root Motion' checkbox (should be unchecked)\n" +
            "2. Animation clips don't have root motion curves (check in Animation window)\n" +
            "3. CharacterController is handling all movement",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        animatorController = EditorGUILayout.ObjectField(
            "Animator Controller",
            animatorController,
            typeof(AnimatorController),
            false
        ) as AnimatorController;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Find All Animation Clips", GUILayout.Height(30)))
        {
            FindAnimationClips();
        }
        
        GUILayout.Space(10);
        
        if (animationClips != null && animationClips.Length > 0)
        {
            GUILayout.Label($"Found {animationClips.Length} animation clips:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(300));
            foreach (AnimationClip clip in animationClips)
            {
                if (clip != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(clip.name, GUILayout.Width(300));
                    
                    // Root motion curve kontrolü
                    bool hasRootMotion = HasRootMotionCurves(clip);
                    EditorGUILayout.LabelField(hasRootMotion ? "⚠️ Has Root Motion" : "✅ No Root Motion", GUILayout.Width(150));
                    
                    if (hasRootMotion && GUILayout.Button("View in Animation", GUILayout.Width(150)))
                    {
                        // Animation window'u aç (manuel kontrol için)
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = clip;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "NOTE: Root motion curves in animation clips cannot be removed programmatically.\n" +
            "You need to:\n" +
            "1. Open the animation clip in Animation window\n" +
            "2. Delete root transform position/rotation curves manually\n" +
            "OR\n" +
            "3. Re-import the FBX with 'Root Transform Position' and 'Root Transform Rotation' disabled in Import Settings",
            MessageType.Warning
        );
    }
    
    private void FindAnimationClips()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        System.Collections.Generic.List<AnimationClip> clips = new System.Collections.Generic.List<AnimationClip>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }
        
        animationClips = clips.ToArray();
        Debug.Log($"[EnemyRootMotionFixer] Found {animationClips.Length} animation clips.");
    }
    
    /// <summary>
    /// Animasyon clip'inin root motion curve'leri olup olmadığını kontrol eder
    /// </summary>
    private bool HasRootMotionCurves(AnimationClip clip)
    {
        if (clip == null) return false;
        
        // Root transform position/rotation curve'lerini kontrol et
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        
        foreach (EditorCurveBinding binding in bindings)
        {
            // Root transform path'i genellikle boş string veya "Root" olur
            if (string.IsNullOrEmpty(binding.path) || binding.path == "Root")
            {
                // Position veya rotation curve'leri varsa root motion var demektir
                if (binding.propertyName.Contains("m_LocalPosition") || 
                    binding.propertyName.Contains("m_LocalRotation") ||
                    binding.propertyName.Contains("localPosition") ||
                    binding.propertyName.Contains("localRotation"))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Tüm animasyon clip'lerinden root motion curve'lerini kaldırır (ışınlanma sorununu çözer)
    /// </summary>
    private void RemoveRootMotionCurves()
    {
        Debug.Log("[EnemyRootMotionFixer] Starting to remove root motion curves...");
        
        // Tüm animasyon clip'lerini bul
        FindAnimationClips();
        
        if (animationClips == null || animationClips.Length == 0)
        {
            Debug.LogWarning("[EnemyRootMotionFixer] ⚠️ No animation clips found!");
            return;
        }
        
        int removedCount = 0;
        
        foreach (AnimationClip clip in animationClips)
        {
            if (clip == null) continue;
            
            // Root motion curve'lerini bul
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            System.Collections.Generic.List<EditorCurveBinding> rootMotionBindings = 
                new System.Collections.Generic.List<EditorCurveBinding>();
            
            foreach (EditorCurveBinding binding in bindings)
            {
                // Root transform path'i genellikle boş string veya "Root" olur
                if (string.IsNullOrEmpty(binding.path) || binding.path == "Root")
                {
                    // Position veya rotation curve'leri varsa root motion var demektir
                    if (binding.propertyName.Contains("m_LocalPosition") || 
                        binding.propertyName.Contains("m_LocalRotation") ||
                        binding.propertyName.Contains("localPosition") ||
                        binding.propertyName.Contains("localRotation"))
                    {
                        rootMotionBindings.Add(binding);
                    }
                }
            }
            
            // Root motion curve'lerini kaldır
            if (rootMotionBindings.Count > 0)
            {
                foreach (EditorCurveBinding binding in rootMotionBindings)
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
                
                EditorUtility.SetDirty(clip);
                removedCount++;
                Debug.Log($"[EnemyRootMotionFixer] ✅ Removed {rootMotionBindings.Count} root motion curves from '{clip.name}'");
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[EnemyRootMotionFixer] ✅ Removed root motion curves from {removedCount} animation clips!");
    }
}
