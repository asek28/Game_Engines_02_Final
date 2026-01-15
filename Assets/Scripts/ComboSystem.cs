using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ComboSystem : MonoBehaviour
{
	public static event Action OnAttackPerformed;
	
	private Animator anim;
	private AudioSource audioSource;
	private PlayerAnimationController playerAnimController;

	[Header("Combo Settings")]
	[Tooltip("Time window to chain next hit before combo resets (seconds)")]
	public float comboResetTime = 1.25f; // was 0.5f
	[Tooltip("Minimum time between two attack inputs (seconds)")]
	public float minAttackInterval = 0.2f;

	[Header("Audio Settings")]
	[Tooltip("Optional explicit AudioSource. If not assigned, one will be fetched/created on this GameObject.")]
	[SerializeField] private AudioSource hitAudioSource;
	[Tooltip("List of hit SFX clips. Populate with SFX/Hit/Hit_01..Hit_04.")]
	[SerializeField] private AudioClip[] hitClips;

	private int comboCount = 0;
	private float lastComboTime = 0f;
	private float lastAttackTime = 0f;
	private bool isAttacking = false;
	
	void Start()
	{
		anim = GetComponent<Animator>();
		playerAnimController = GetComponent<PlayerAnimationController>();
		
		if (playerAnimController == null)
		{
			Debug.LogWarning("[ComboSystem] PlayerAnimationController not found! isStanding won't work.");
		}
		else
		{
			Debug.Log("[ComboSystem] PlayerAnimationController found!");
		}

		audioSource = hitAudioSource;
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}

		if (hitClips == null || hitClips.Length == 0)
		{
			Debug.LogWarning("ComboSystem: No hit SFX clips assigned. Assign clips in the inspector to enable hit sounds.");
		}
	}
	
	void Update()
	{
		// Inventory açıksa saldırı yapma
		if (InventoryManager.instance != null && InventoryManager.instance.IsInventoryVisible)
		{
			return;
		}
		
		// Settings paneli açıksa saldırı yapma
		SettingsMenuController settingsMenu = FindFirstObjectByType<SettingsMenuController>();
		if (settingsMenu != null && settingsMenu.IsSettingsOpen())
		{
			return;
		}
		
		// Input kontrolü (önce tanımla)
		bool attackPressed = false;
		
		var keyboard = Keyboard.current;
		var mouse = Mouse.current;
		
		if (mouse != null)
		{
			attackPressed = mouse.leftButton.wasPressedThisFrame;
		}
		else if (keyboard != null)
		{
			attackPressed = keyboard.enterKey.wasPressedThisFrame;
		}
		
		// Sadece MeleeWeapon aktifken combo sistemi çalışsın
		WeaponSlotSystem weaponSlotSystem = FindFirstObjectByType<WeaponSlotSystem>();
		if (weaponSlotSystem != null)
		{
			IWeapon currentWeapon = weaponSlotSystem.GetCurrentWeapon();
			if (currentWeapon == null || !(currentWeapon is MeleeWeapon))
			{
				// Gun veya başka silah aktifse combo sistemi çalışmasın
				return;
			}
			// Debug: Stick aktif ve attack basıldı
			if (currentWeapon is MeleeWeapon && attackPressed)
			{
				Debug.Log($"[ComboSystem] MeleeWeapon active, attack pressed!");
			}
		}
		
		
		
		if (anim != null)
		{
			AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
			isAttacking = stateInfo.IsName("Dual Weapon_01") || 
						 stateInfo.IsName("Dual Weapon_02") || 
						 stateInfo.IsName("Dual Weapon Combo 1");
		}
		
		
		if (attackPressed && Time.time - lastAttackTime >= minAttackInterval)
		{
			lastAttackTime = Time.time;
			
			
			if (!isAttacking)
			{
				
				comboCount = 1;
			}
			else
			{
				
				comboCount++;
			}
			
			// Maksimum combo
			if (comboCount > 4)
			{
				comboCount = 3;
			}
			
			Debug.Log($"[ComboSystem] Attack pressed! Combo Count: {comboCount}, Is Attacking: {isAttacking}");
			
			// PlayerAnimationController ile isStanding parametresini set et (Stick attack)
			if (playerAnimController != null)
			{
				playerAnimController.SetStanding(true);
				Debug.Log($"[ComboSystem] Called SetStanding(true)");
			}
			else
			{
				Debug.LogWarning($"[ComboSystem] PlayerAnimationController is NULL! Can't set isStanding!");
			}
			
			// Eski animator sistemi (ComboCount parametresi)
			if (anim != null)
			{
				anim.SetInteger("ComboCount", comboCount);
				
				
				Debug.Log($"Setting ComboCount to {comboCount} in Animator");

				PlayRandomHitSound();
				
				// Saldırı eventini tetikle
				if (OnAttackPerformed != null)
				{
					OnAttackPerformed.Invoke();
				}
			}
			
			
			lastComboTime = Time.time;
		}
		
		// combo reset
		if (Time.time - lastComboTime > comboResetTime && comboCount > 0)
		{
			comboCount = 0;
			lastComboTime = Time.time;
			
			// isStanding animasyonunu kapat
			if (playerAnimController != null)
			{
				playerAnimController.SetStanding(false);
			}
			
			if (anim != null)
			{
				anim.SetInteger("ComboCount", 0);
			}
			
			Debug.Log("Combo reset!");
		}
	}

	private void PlayRandomHitSound()
	{
		if (audioSource == null || hitClips == null || hitClips.Length == 0)
		{
			return;
		}

		AudioClip clip = hitClips[UnityEngine.Random.Range(0, hitClips.Length)];
		if (clip != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}
}

