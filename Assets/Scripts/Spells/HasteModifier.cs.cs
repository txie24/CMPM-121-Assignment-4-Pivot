// File: Assets/Scripts/Spells/HasteModifier.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class HasteModifier : ModifierSpell
{
    private float speedBonus = 2f;
    private float duration = 2f;
    private string modifierName = "hasted";
    private string modifierDescription = "Temporarily boosts your movement speed after casting.";

    // track buff state
    private int originalSpeed;
    private Coroutine buffCoroutine;
    private bool buffActive = false;

    public HasteModifier(Spell inner) : base(inner) { }

    protected override string Suffix => modifierName;

    public override void LoadAttributes(JObject j, Dictionary<string, float> vars)
    {
        modifierName = j["name"]?.Value<string>() ?? modifierName;
        modifierDescription = j["description"]?.Value<string>() ?? modifierDescription;

        if (j["speed_bonus"] != null)
            speedBonus = RPNEvaluator.SafeEvaluateFloat(
                j["speed_bonus"].Value<string>(),
                vars,
                speedBonus);

        if (j["duration"] != null)
            duration = RPNEvaluator.SafeEvaluateFloat(
                j["duration"].Value<string>(),
                vars,
                duration);

        base.LoadAttributes(j, vars);
    }

    protected override void InjectMods(StatBlock mods) { }

    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        var player = GameManager.Instance.player
                            .GetComponent<PlayerController>();
        if (player != null)
        {
            // cancel any existing removal
            if (buffActive && buffCoroutine != null)
                player.StopCoroutine(buffCoroutine);

            // record original speed once
            if (!buffActive)
            {
                originalSpeed = player.speed;
                buffActive = true;
            }

            // apply buff
            player.speed = originalSpeed + Mathf.RoundToInt(speedBonus);

            // immediately reapply whatever direction you’re moving
            var dir = player.unit.movement.normalized;
            if (dir != Vector2.zero)
                player.unit.movement = dir * player.speed;

            // schedule revert
            buffCoroutine = player.StartCoroutine(RemoveBuffAfterDelay(player));
        }

        // then fire the wrapped spell as usual
        yield return inner.TryCast(from, to);
    }

    private IEnumerator RemoveBuffAfterDelay(PlayerController player)
    {
        yield return new WaitForSeconds(duration);

        // revert speed
        player.speed = originalSpeed;
        buffActive = false;
        buffCoroutine = null;

        // reapply movement at original speed
        var dir = player.unit.movement.normalized;
        if (dir != Vector2.zero)
            player.unit.movement = dir * player.speed;
    }
}
