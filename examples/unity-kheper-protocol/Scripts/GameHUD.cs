using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Meters")]
    [SerializeField] private Slider maatMeter;
    [SerializeField] private Slider energyMeter;

    [Header("Sources")]
    [SerializeField] private PlayerCombatController playerCombatController;
    [SerializeField] private SpecialMoves specialMoves;

    private void OnEnable()
    {
        if (playerCombatController != null)
        {
            playerCombatController.MaatChanged += UpdateMaat;
            UpdateMaat(playerCombatController.maat, playerCombatController.maxMaat);
        }

        if (specialMoves != null)
        {
            specialMoves.EnergyChanged += UpdateEnergy;
            UpdateEnergy(specialMoves.currentEnergy, specialMoves.maxEnergy);
        }
    }

    private void OnDisable()
    {
        if (playerCombatController != null)
        {
            playerCombatController.MaatChanged -= UpdateMaat;
        }

        if (specialMoves != null)
        {
            specialMoves.EnergyChanged -= UpdateEnergy;
        }
    }

    public void Bind(PlayerCombatController combatController, SpecialMoves moves)
    {
        playerCombatController = combatController;
        specialMoves = moves;
    }

    public void UpdateMaat(float value, float max)
    {
        if (maatMeter == null)
        {
            return;
        }

        maatMeter.value = max <= 0f ? 0f : value / max;
    }

    public void UpdateEnergy(float value, float max)
    {
        if (energyMeter == null)
        {
            return;
        }

        energyMeter.value = max <= 0f ? 0f : value / max;
    }
}
