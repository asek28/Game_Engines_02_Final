using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS (First Person Shooter) kamera kontrolü
/// Oyuncu karakterin bakış açısına göre kamera kontrolü sağlar
/// </summary>
public class RightMouseOrbit : MonoBehaviour
{
	[Header("Player Target")]
	[Tooltip("Oyuncu karakter transform'u (Player GameObject)")]
	public Transform target; // Assign your player here

	[Header("Head Tracking")]
	[Tooltip("Karakterin kafasına odaklan (FPS için önerilir)")]
	public bool followHeadBone = true;
	[Tooltip("Otomatik kafatası arama için kemik adı")]
	public string headBoneName = "mixamorig:Head";
	[Tooltip("Baş kemiğini manuel atamak isterseniz")]
	public Transform headBoneOverride;
	[Tooltip("Baş kemiğinden kameraya uygulanacak ekstra offset")]
	public Vector3 headOffset = new Vector3(0f, 0.08f, 0f);
	[Tooltip("Baş kemiği bulunamadığında kullanılacak göz yüksekliği")]
	public float fallbackEyeHeight = 1.6f;
	[Tooltip("Ek kamera offset değeri (kafa referansına eklenir)")]
	public Vector3 additionalOffset = Vector3.zero; 

	[Header("FPS Mouse Look Settings")]
	[Tooltip("Mouse hassasiyeti (ne kadar hızlı dönecek)")]
	[Range(0.1f, 10f)]
	public float mouseSensitivity = 2f;
	[Tooltip("Aşağı bakış minimum açısı")]
	public float minPitch = -90f;
	[Tooltip("Yukarı bakış maximum açısı")]
	public float maxPitch = 90f;
	[Tooltip("Y eksenini ters çevir (invert mouse Y)")]
	public bool invertY = false;
	[Tooltip("Smooth rotation kullan (daha yumuşak hareket)")]
	public bool useSmoothRotation = false;
	[Tooltip("Smooth rotation hızı (sadece useSmoothRotation true ise)")]
	[Range(1f, 20f)]
	public float smoothRotationSpeed = 10f;

	[Header("FPS Options")]
	[Tooltip("Oyun başladığında cursor'ı kilitle")]
	public bool lockCursorOnStart = true;
	[Tooltip("ESC tuşu ile cursor kilidini aç/kapat")]
	public bool unlockCursorOnEscape = true;

	// Private variables
	private float yaw = 0f;   // Yatay dönüş açısı (karakter rotasyonu)
	private float pitch = 0f; // Dikey dönüş açısı (sadece kamera)
	private bool isCursorLocked = false;
	private Quaternion targetRotation;
	private Transform headBone;

	void Start()
	{
		// Başlangıç rotasyonunu karakterden veya kameradan al
		if (target != null)
		{
			yaw = target.eulerAngles.y;
		}
		else
		{
			Vector3 e = transform.rotation.eulerAngles;
			yaw = e.y;
			pitch = e.x;
			if (pitch > 180f) pitch -= 360f; // -180 ile 180 arasına normalize et
		}

		// Başlangıç rotasyonunu ayarla
		targetRotation = Quaternion.Euler(pitch, yaw, 0f);

		// FPS cursor kilitleme
		if (lockCursorOnStart)
		{
			LockCursor();
		}

		// Baş kemiğini bul
		ResolveHeadBone();

		// Kamera başlangıç pozisyonunu ayarla
		if (target != null)
		{
			UpdateCameraPosition();
		}
	}

	void Update()
	{
		// ESC ile cursor kilidini aç/kapat
		if (unlockCursorOnEscape && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			if (isCursorLocked)
			{
				UnlockCursor();
			}
			else
			{
				LockCursor();
			}
		}

		// Mouse sol tık ile cursor kilitleme
		if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isCursorLocked)
		{
			LockCursor();
		}
	}

	private void ResolveHeadBone()
	{
		headBone = null;

		if (!followHeadBone)
		{
			return;
		}

		if (headBoneOverride != null)
		{
			headBone = headBoneOverride;
			return;
		}

		if (target == null) return;

		// Karakter hiyerarşisinde kafa kemiğini ara
		headBone = FindChildRecursive(target, headBoneName);

		if (headBone == null)
		{
			Debug.LogWarning($"RightMouseOrbit: '{headBoneName}' adıyla bir kafa kemiği bulunamadı. fallbackEyeHeight kullanılacak.");
		}
	}

	private Transform FindChildRecursive(Transform parent, string name)
	{
		foreach (Transform child in parent)
		{
			if (child.name == name)
			{
				return child;
			}

			Transform result = FindChildRecursive(child, name);
			if (result != null)
			{
				return result;
			}
		}

		return null;
	}

	void LateUpdate()
	{
		if (target == null) return;

		// Cursor kilitli değilse kamera kontrolü yapma
		if (!isCursorLocked) return;

		var mouse = Mouse.current;
		if (mouse == null) return;

		// Mouse hareketi ile FPS kamera kontrolü
		Vector2 delta = mouse.delta.ReadValue();
		
		// Yatay dönüş (yaw) - hem karakter hem kamera döner
		yaw += delta.x * mouseSensitivity;
		
		// Dikey dönüş (pitch) - sadece kamera yukarı/aşağı bakar
		float pitchDelta = delta.y * mouseSensitivity;
		if (invertY) pitchDelta = -pitchDelta;
		pitch -= pitchDelta;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

		// Karakteri yatay eksende döndür (karakterin bakış açısı)
		target.rotation = Quaternion.Euler(0f, yaw, 0f);

		// Kameranın hedef rotasyonu (karakterin yaw'ı + kameranın pitch'i)
		targetRotation = Quaternion.Euler(pitch, yaw, 0f);

		// Kamerayı karakterin göz seviyesine yerleştir
		UpdateCameraPosition();

		// Kameranın rotasyonunu uygula (smooth veya direkt)
		if (useSmoothRotation)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
		}
		else
		{
			transform.rotation = targetRotation;
		}
	}

	/// <summary>
	/// Kamerayı karakterin göz seviyesine yerleştir
	/// </summary>
	private void UpdateCameraPosition()
	{
		if (target == null) return;

		Vector3 basePosition;

		if (followHeadBone && headBone != null)
		{
			basePosition = headBone.position + headBone.TransformVector(headOffset);
		}
		else
		{
			// Kamerayı karakter gövdesine göre göz seviyesine yerleştir (fallback)
			basePosition = target.position + Vector3.up * fallbackEyeHeight;
		}

		// Ek offset uygula (karakterin dönüşüne göre)
		Vector3 worldOffset = target.TransformVector(additionalOffset);
		Vector3 cameraPosition = basePosition + worldOffset;

		transform.position = cameraPosition;
	}

	/// <summary>
	/// Cursor'ı kilitle (FPS modu)
	/// </summary>
	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		isCursorLocked = true;
	}

	/// <summary>
	/// Cursor kilidini aç (menü modu)
	/// </summary>
	private void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		isCursorLocked = false;
	}
}
