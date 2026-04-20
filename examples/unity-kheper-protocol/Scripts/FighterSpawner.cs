using UnityEngine;

public class FighterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject voraPrefab;
    [SerializeField] private GameObject sabraPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameHUD hud;

    private void Start()
    {
        SpawnSelectedFighter();
    }

    private void SpawnSelectedFighter()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is not assigned.", this);
            return;
        }

        GameObject prefab = CharacterSelectController.SelectedCharacterId == "Sabra"
            ? sabraPrefab
            : voraPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("Selected fighter prefab is not assigned.", this);
            return;
        }

        GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        if (hud == null)
        {
            return;
        }

        PlayerCombatController combatController = instance.GetComponent<PlayerCombatController>();
        SpecialMoves specialMoves = instance.GetComponent<SpecialMoves>();
        if (combatController != null && specialMoves != null)
        {
            hud.Bind(combatController, specialMoves);
        }
    }
}
