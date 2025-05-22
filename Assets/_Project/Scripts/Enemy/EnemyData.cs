using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3.5f; // Velocidad del NavMeshAgent
    public float angularSpeed = 120f; // Velocidad de giro del NavMeshAgent
    public int attackDamage = 10;
    public float attackRange = 1.5f;
    public float attackRate = 1f;
    // Podrías añadir más: puntuación por muerte, tipo de loot, etc.

    [Header("Visuals (Opcional)")]
    public Material enemyMaterial; // Para cambiar el color/material fácilmente
    // public Mesh enemyMesh; // Si quieres diferentes mallas por tipo
    [Header("Rewards")]
    public int currencyAwardedOnDeath = 10;

}