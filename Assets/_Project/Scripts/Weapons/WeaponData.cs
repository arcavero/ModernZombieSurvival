using UnityEngine;

// Añade un atributo para poder crear instancias de este objeto desde el menú Assets->Create
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game Data/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject // ¡Hereda de ScriptableObject, no de MonoBehaviour!
{
    [Header("Info")]
    [Tooltip("Nombre del arma para mostrar en UI, etc.")]
    public string weaponName = "Default Weapon";

    [Header("Shooting")]
    [Tooltip("Daño infligido por cada disparo/bala.")]
    [Min(0)] // Asegura que el daño no sea negativo en el Inspector
    public float damage = 10f;

    [Tooltip("Alcance máximo del disparo (para Raycast).")]
    [Min(0.1f)]
    public float range = 100f;

    [Tooltip("Tiempo mínimo en segundos entre disparos (Cadencia). 0.1 = 10 disparos/segundo.")]
    [Min(0.01f)]
    public float fireRate = 0.5f; // 2 disparos por segundo por defecto

    // --- Podríamos añadir más datos aquí en el futuro ---
    // [Header("Ammo")]
    // public int magazineSize = 15;
    // public int totalAmmoCapacity = 90;
    // public float reloadTime = 1.5f;

    // [Header("Visuals & Audio")]
    // public GameObject muzzleFlashPrefab;
    // public GameObject bulletImpactEffectPrefab;
    // public AudioClip fireSound;
    // public AudioClip reloadSound;
}