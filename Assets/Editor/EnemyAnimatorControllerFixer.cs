using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Enemy Animator Controller'ı otomatik olarak düzenler
/// - Death animasyonu geçişlerini ayarlar
/// - Transition ayarlarını düzeltir (donma sorununu çözer)
/// - Exit Time ve Interruption ayarlarını optimize eder
/// </summary>
public class EnemyAnimatorControllerFixer : EditorWindow
{
    private AnimatorController animatorController;
    
    [MenuItem("Tools/Fix Enemy Animator Controller")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAnimatorControllerFixer>("Enemy Animator Fixer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enemy Animator Controller Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        animatorController = EditorGUILayout.ObjectField(
            "Animator Controller",
            animatorController,
            typeof(AnimatorController),
            false
        ) as AnimatorController;
        
        GUILayout.Space(10);
        
        if (animatorController == null)
        {
            EditorGUILayout.HelpBox("Please assign an Animator Controller to fix.", MessageType.Warning);
            return;
        }
        
        if (GUILayout.Button("Fix Animator Controller", GUILayout.Height(30)))
        {
            FixAnimatorController();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Fix Death Parameter (Int → Trigger)", GUILayout.Height(30)))
        {
            FixDeathParameter();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Fix Death parameter from Int to Trigger in all transitions\n" +
            "2. Fix Death animation transitions\n" +
            "3. Fix animation freezing issues\n" +
            "4. Optimize transition settings\n" +
            "5. Set proper Exit Time and Interruption settings",
            MessageType.Info
        );
    }
    
    private void FixAnimatorController()
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
                
                // Tüm transition'ları kontrol et
                foreach (AnimatorStateTransition transition in animatorState.transitions)
                {
                    // 1. Exit Time ayarlarını düzelt (animasyonların donmasını önlemek için)
                    // Sadece belirli animasyonlar için Exit Time kullan (Death gibi)
                    bool isDeathTransition = transition.destinationState.name.ToLower().Contains("death") ||
                                            transition.destinationState.name.ToLower().Contains("die");
                    
                    if (isDeathTransition)
                    {
                        // Death animasyonu için Exit Time kullan (animasyon bitene kadar bekle)
                        if (!transition.hasExitTime)
                        {
                            transition.hasExitTime = true;
                            transition.exitTime = 0.9f; // Animasyonun %90'ında geçiş yap
                            hasChanges = true;
                            Debug.Log($"[EnemyAnimatorFixer] Fixed Death transition Exit Time for {animatorState.name} -> {transition.destinationState.name}");
                        }
                    }
                    else
                    {
                        // Diğer animasyonlar için Exit Time kullanma (anında geçiş)
                        if (transition.hasExitTime)
                        {
                            transition.hasExitTime = false;
                            hasChanges = true;
                            Debug.Log($"[EnemyAnimatorFixer] Disabled Exit Time for {animatorState.name} -> {transition.destinationState.name} (prevents freezing)");
                        }
                    }
                    
                    // 2. Transition Duration'ı kısalt (daha hızlı geçiş)
                    if (transition.duration > 0.1f && !isDeathTransition)
                    {
                        transition.duration = 0.05f; // 0.05 saniye (çok hızlı geçiş)
                        hasChanges = true;
                        Debug.Log($"[EnemyAnimatorFixer] Shortened transition duration for {animatorState.name} -> {transition.destinationState.name}");
                    }
                    
                    // 3. Interruption Source ayarlarını optimize et
                    // Death animasyonu kesilemez, diğerleri kesilebilir
                    if (isDeathTransition)
                    {
                        transition.interruptionSource = TransitionInterruptionSource.None; // Death kesilemez
                        hasChanges = true;
                    }
                    else
                    {
                        transition.interruptionSource = TransitionInterruptionSource.Destination; // Diğerleri kesilebilir
                        hasChanges = true;
                    }
                    
                    // 4. Ordered Interruption'ı etkinleştir (öncelikli geçişler için)
                    if (!transition.orderedInterruption)
                    {
                        transition.orderedInterruption = true;
                        hasChanges = true;
                    }
                }
            }
            
            // Any State'den Death'e geçiş kontrolü
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                bool isDeathTransition = transition.destinationState.name.ToLower().Contains("death") ||
                                        transition.destinationState.name.ToLower().Contains("die");
                
                if (isDeathTransition)
                {
                    // Death geçişi için Exit Time kullanma (anında geçiş)
                    if (transition.hasExitTime)
                    {
                        transition.hasExitTime = false;
                        hasChanges = true;
                        Debug.Log($"[EnemyAnimatorFixer] Fixed Any State -> Death transition (removed Exit Time)");
                    }
                    
                    // Death geçişi kesilemez
                    if (transition.interruptionSource != TransitionInterruptionSource.None)
                    {
                        transition.interruptionSource = TransitionInterruptionSource.None;
                        hasChanges = true;
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            EditorUtility.SetDirty(animatorController);
            AssetDatabase.SaveAssets();
            Debug.Log($"[EnemyAnimatorFixer] ✅ Animator Controller '{animatorController.name}' fixed successfully!");
        }
        else
        {
            Debug.Log($"[EnemyAnimatorFixer] ℹ️ No changes needed for '{animatorController.name}'.");
        }
    }
    
    /// <summary>
    /// Death parametresini Int'den Trigger'a çevirir (tüm transition'larda)
    /// </summary>
    private void FixDeathParameter()
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
            
            // Tüm state'lerin transition'larını kontrol et
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                AnimatorState animatorState = state.state;
                
                // Tüm transition'ları kontrol et
                foreach (AnimatorStateTransition transition in animatorState.transitions)
                {
                    // Death parametresini kullanan condition'ları bul ve düzelt
                    for (int i = transition.conditions.Length - 1; i >= 0; i--)
                    {
                        AnimatorCondition condition = transition.conditions[i];
                        
                        if (condition.parameter == "Death")
                        {
                            // Death parametresi Int olarak kullanılıyor, Trigger'a çevir
                            // Int condition'ları kaldır (Greater, Less, Equals, NotEqual)
                            if (condition.mode == AnimatorConditionMode.Greater ||
                                condition.mode == AnimatorConditionMode.Less ||
                                condition.mode == AnimatorConditionMode.Equals ||
                                condition.mode == AnimatorConditionMode.NotEqual)
                            {
                                // Int condition'ı kaldır
                                System.Collections.Generic.List<AnimatorCondition> newConditions = 
                                    new System.Collections.Generic.List<AnimatorCondition>(transition.conditions);
                                newConditions.RemoveAt(i);
                                transition.conditions = newConditions.ToArray();
                                
                                // Trigger condition ekle
                                System.Collections.Generic.List<AnimatorCondition> updatedConditions = 
                                    new System.Collections.Generic.List<AnimatorCondition>(transition.conditions);
                                updatedConditions.Add(new AnimatorCondition
                                {
                                    mode = AnimatorConditionMode.If,
                                    parameter = "Death",
                                    threshold = 0f
                                });
                                transition.conditions = updatedConditions.ToArray();
                                
                                hasChanges = true;
                                Debug.Log($"[EnemyAnimatorFixer] Fixed Death parameter in transition from '{animatorState.name}' (Int → Trigger)");
                            }
                            // Zaten Trigger ise (If, IfNot), değişiklik yapma
                        }
                    }
                }
            }
            
            // Any State transition'larını kontrol et
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                // Death parametresini kullanan condition'ları bul ve düzelt
                for (int i = transition.conditions.Length - 1; i >= 0; i--)
                {
                    AnimatorCondition condition = transition.conditions[i];
                    
                    if (condition.parameter == "Death")
                    {
                        // Death parametresi Int olarak kullanılıyor, Trigger'a çevir
                        if (condition.mode == AnimatorConditionMode.Greater ||
                            condition.mode == AnimatorConditionMode.Less ||
                            condition.mode == AnimatorConditionMode.Equals ||
                            condition.mode == AnimatorConditionMode.NotEqual)
                        {
                            // Int condition'ı kaldır
                            System.Collections.Generic.List<AnimatorCondition> newConditions = 
                                new System.Collections.Generic.List<AnimatorCondition>(transition.conditions);
                            newConditions.RemoveAt(i);
                            transition.conditions = newConditions.ToArray();
                            
                            // Trigger condition ekle
                            System.Collections.Generic.List<AnimatorCondition> updatedConditions = 
                                new System.Collections.Generic.List<AnimatorCondition>(transition.conditions);
                            updatedConditions.Add(new AnimatorCondition
                            {
                                mode = AnimatorConditionMode.If,
                                parameter = "Death",
                                threshold = 0f
                            });
                            transition.conditions = updatedConditions.ToArray();
                            
                            hasChanges = true;
                            Debug.Log($"[EnemyAnimatorFixer] Fixed Death parameter in Any State transition (Int → Trigger)");
                        }
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            EditorUtility.SetDirty(animatorController);
            AssetDatabase.SaveAssets();
            Debug.Log($"[EnemyAnimatorFixer] ✅ Death parameter fixed in '{animatorController.name}'! (Int → Trigger)");
        }
        else
        {
            Debug.Log($"[EnemyAnimatorFixer] ℹ️ No Death parameter issues found in '{animatorController.name}'.");
        }
    }
}
