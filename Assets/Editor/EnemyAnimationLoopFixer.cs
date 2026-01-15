using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Enemy animasyonlarının loop ayarlarını otomatik olarak düzenler
/// - Walking, Idle gibi animasyonları loop yapar
/// - Animator Controller state'lerinin loop ayarlarını kontrol eder
/// </summary>
public class EnemyAnimationLoopFixer : EditorWindow
{
    private AnimatorController animatorController;
    private AnimationClip[] animationClips;
    
    [MenuItem("Tools/Fix Enemy Animation Loops")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAnimationLoopFixer>("Enemy Animation Loop Fixer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enemy Animation Loop Fixer", EditorStyles.boldLabel);
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
            foreach (AnimationClip clip in animationClips)
            {
                if (clip != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(clip.name);
                    bool isLooping = EditorGUILayout.Toggle(clip.isLooping);
                    if (isLooping != clip.isLooping)
                    {
                        SetAnimationLoop(clip, isLooping);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        GUILayout.Space(10);
        
        if (animatorController == null)
        {
            EditorGUILayout.HelpBox("Please assign an Animator Controller to fix state loops.", MessageType.Warning);
            return;
        }
        
        if (GUILayout.Button("Fix Animator Controller Loops", GUILayout.Height(30)))
        {
            FixAnimatorControllerLoops();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Auto-Fix All (Recommended)", GUILayout.Height(40)))
        {
            AutoFixAll();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Set Walking, Idle, and other movement animations to loop\n" +
            "2. Fix Animator Controller state loop settings\n" +
            "3. Ensure animations play continuously\n\n" +
            "NOTE: Works with any Animator Controller name (Homless, Enemy, etc.)\n" +
            "It checks animation clip names, not controller name.",
            MessageType.Info
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
        Debug.Log($"[EnemyAnimationLoopFixer] Found {animationClips.Length} animation clips.");
    }
    
    private void SetAnimationLoop(AnimationClip clip, bool loop)
    {
        if (clip == null) return;
        
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[EnemyAnimationLoopFixer] Set '{clip.name}' loop to {loop}");
    }
    
    private void FixAnimatorControllerLoops()
    {
        if (animatorController == null)
        {
            Debug.LogError("Animator Controller is null!");
            return;
        }
        
        bool hasChanges = false;
        
        // Tüm layer'ları kontrol et
        foreach (AnimatorControllerLayer layer in animatorController.layers)
        {
            AnimatorStateMachine stateMachine = layer.stateMachine;
            
            // Tüm state'leri kontrol et
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                AnimatorState animatorState = state.state;
                
                // State'in motion'unu kontrol et
                if (animatorState.motion != null)
                {
                    AnimationClip clip = animatorState.motion as AnimationClip;
                    if (clip != null)
                    {
                    // Walking, Idle, Run gibi animasyonları loop yap
                    // NOT: Animator Controller ismi önemli değil, animasyon clip isimlerine bakıyoruz
                    // ÖNEMLİ: Wounded/Damaged walking animasyonları da loop olmalı!
                    string clipName = clip.name.ToLower();
                    bool shouldLoop = clipName.Contains("walk") || 
                                    clipName.Contains("idle") || 
                                    clipName.Contains("run") ||
                                    clipName.Contains("wounded") ||
                                    clipName.Contains("damaged") ||
                                    clipName.Contains("standing") ||
                                    clipName.Contains("move") ||
                                    clipName.Contains("walking_damaged") ||
                                    clipName.Contains("walking_dameged") || // Typo kontrolü
                                    clipName.Contains("walk_damaged") ||
                                    clipName.Contains("walk_dameged"); // Typo kontrolü
                    
                    // Death, Attack, Hit gibi animasyonları loop yapma
                    // NOT: "damaged" walking animasyonları loop olmalı, sadece "damage" (impact) animasyonları loop olmamalı
                    bool shouldNotLoop = clipName.Contains("death") || 
                                        clipName.Contains("die") ||
                                        clipName.Contains("attack") ||
                                        clipName.Contains("hit") ||
                                        (clipName.Contains("damage") && !clipName.Contains("damaged") && !clipName.Contains("dameged")) || // "damage" ama "damaged" değil
                                        clipName.Contains("loot") ||
                                        clipName.Contains("collect") ||
                                        clipName.Contains("react") ||
                                        clipName.Contains("combo");
                        
                        if (shouldLoop && !shouldNotLoop)
                        {
                            // Animasyon clip'inin loop ayarını kontrol et
                            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                            if (!settings.loopTime)
                            {
                                settings.loopTime = true;
                                AnimationUtility.SetAnimationClipSettings(clip, settings);
                                EditorUtility.SetDirty(clip);
                                hasChanges = true;
                                Debug.Log($"[EnemyAnimationLoopFixer] Set '{clip.name}' to loop (should loop: {shouldLoop})");
                            }
                        }
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"[EnemyAnimationLoopFixer] ✅ Animator Controller '{animatorController.name}' loops fixed successfully!");
        }
        else
        {
            Debug.Log($"[EnemyAnimationLoopFixer] ℹ️ No changes needed for '{animatorController.name}'.");
        }
    }
    
    private void AutoFixAll()
    {
        Debug.Log("[EnemyAnimationLoopFixer] Starting auto-fix...");
        
        // 1. Tüm animasyon clip'lerini bul ve loop yap
        FindAnimationClips();
        
        if (animationClips != null && animationClips.Length > 0)
        {
            foreach (AnimationClip clip in animationClips)
            {
                if (clip != null)
                {
                    string clipName = clip.name.ToLower();
                    
                    // Walking, Idle, Run gibi animasyonları loop yap
                    // NOT: Animator Controller ismi önemli değil, animasyon clip isimlerine bakıyoruz
                    // ÖNEMLİ: Wounded/Damaged walking animasyonları da loop olmalı!
                    bool shouldLoop = clipName.Contains("walk") || 
                                    clipName.Contains("idle") || 
                                    clipName.Contains("run") ||
                                    clipName.Contains("wounded") ||
                                    clipName.Contains("damaged") ||
                                    clipName.Contains("standing") ||
                                    clipName.Contains("move") ||
                                    clipName.Contains("walking_damaged") ||
                                    clipName.Contains("walking_dameged") || // Typo kontrolü
                                    clipName.Contains("walk_damaged") ||
                                    clipName.Contains("walk_dameged"); // Typo kontrolü
                    
                    // Death, Attack, Hit gibi animasyonları loop yapma
                    // NOT: "damaged" walking animasyonları loop olmalı, sadece "damage" (impact) animasyonları loop olmamalı
                    bool shouldNotLoop = clipName.Contains("death") || 
                                      clipName.Contains("die") ||
                                      clipName.Contains("attack") ||
                                      clipName.Contains("hit") ||
                                      (clipName.Contains("damage") && !clipName.Contains("damaged") && !clipName.Contains("dameged")) || // "damage" ama "damaged" değil
                                      clipName.Contains("loot") ||
                                      clipName.Contains("collect") ||
                                      clipName.Contains("react") ||
                                      clipName.Contains("combo");
                    
                    if (shouldLoop && !shouldNotLoop)
                    {
                        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                        if (!settings.loopTime)
                        {
                            settings.loopTime = true;
                            AnimationUtility.SetAnimationClipSettings(clip, settings);
                            EditorUtility.SetDirty(clip);
                            Debug.Log($"[EnemyAnimationLoopFixer] ✅ Set '{clip.name}' clip loop to true");
                        }
                    }
                }
            }
        }
        
        // 2. Animator Controller'ı düzelt
        if (animatorController != null)
        {
            FixAnimatorControllerLoops();
        }
        else
        {
            Debug.LogWarning("[EnemyAnimationLoopFixer] ⚠️ Animator Controller not assigned. Please assign it and run 'Fix Animator Controller Loops' manually.");
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("[EnemyAnimationLoopFixer] ✅ Auto-fix completed!");
    }
}
