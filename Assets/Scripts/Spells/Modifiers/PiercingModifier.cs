// File: Assets/Scripts/Spells/PiercingModifier.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;                  // for Except()
using Newtonsoft.Json.Linq;

public sealed class PiercingModifier : ModifierSpell
{
    private string modifierName = "pierce";
    private string modifierDescription = "Projectiles pierce the first enemy they hit.";

    public PiercingModifier(Spell inner) : base(inner) { }

    protected override string Suffix => modifierName;

    public override void LoadAttributes(JObject j, Dictionary<string, float> vars)
    {
        modifierName = j["name"]?.Value<string>() ?? modifierName;
        modifierDescription = j["description"]?.Value<string>() ?? modifierDescription;
        // no numeric RPN stats to inject here
        base.LoadAttributes(j, vars);
    }

    protected override void InjectMods(StatBlock mods) { }

    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        // 1) snapshot existing projectiles
        var before = Object
            .FindObjectsByType<ProjectileController>(FindObjectsSortMode.None)
            .ToList();

        // 2) fire the wrapped spell
        yield return inner.TryCast(from, to);

        // 3) capture new ones
        var after = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        var spawned = after.Except(before);

        // 4) mark them piercing (they'll pass through the first unit, 
        //     but hit walls/stuff will still destroy them per our controller change)
        foreach (var proj in spawned)
            proj.piercing = true;

        yield return null;
    }
}
