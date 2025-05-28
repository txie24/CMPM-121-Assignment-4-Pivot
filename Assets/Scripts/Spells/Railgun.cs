// File: Assets/Scripts/Spells/Railgun.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;                  // for Except() and ToList()
using Newtonsoft.Json.Linq;         // for JObject

public sealed class Railgun : Spell
{
    private string displayName;
    private string description;
    private int iconIndex;
    private float baseMana;
    private float baseCooldown;
    private string trajectory;
    private int projectileSprite;

    private string damageExpr;
    private string speedExpr;
    private string lifetimeExpr;

    public Railgun(SpellCaster owner) : base(owner) { }

    public override string DisplayName => displayName;
    public override int IconIndex => iconIndex;

    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(
        damageExpr,
        new Dictionary<string, float> {
            { "power", owner.spellPower },
            { "wave",  GetCurrentWave()    }
        },
        50f);

    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(
        speedExpr,
        new Dictionary<string, float> {
            { "power", owner.spellPower },
            { "wave",  GetCurrentWave()    }
        },
        25f);

    protected override float BaseMana => baseMana;
    protected override float BaseCooldown => baseCooldown;

    private float GetCurrentWave()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        return spawner != null ? spawner.currentWave : 1;
    }

    public override void LoadAttributes(JObject j, Dictionary<string, float> vars)
    {
        displayName = j["name"].Value<string>();
        description = j["description"]?.Value<string>() ?? "";
        iconIndex = j["icon"].Value<int>();

        damageExpr = j["damage"]["amount"].Value<string>();
        speedExpr = j["projectile"]["speed"].Value<string>();

        baseMana = RPNEvaluator.SafeEvaluateFloat(
            j["mana_cost"].Value<string>(), vars, 10f);
        baseCooldown = RPNEvaluator.SafeEvaluateFloat(
            j["cooldown"].Value<string>(), vars, 3f);

        trajectory = j["projectile"]["trajectory"].Value<string>();
        projectileSprite = j["projectile"]["sprite"]?.Value<int>() ?? 0;

        lifetimeExpr = j["projectile"]["lifetime"]?.Value<string>() ?? "1";
    }

    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        // 1) snapshot existing projectiles
        var before = Object
            .FindObjectsByType<ProjectileController>(FindObjectsSortMode.None)
            .ToList();

        // 2) fire the railgun with built-in piercing
        GameManager.Instance.projectileManager.CreatePiercingProjectile(
            projectileSprite,
            trajectory,
            from,
            (to - from).normalized,
            Speed,
            (hit, impactPos) =>
            {
                if (hit.team != owner.team)
                {
                    int amt = Mathf.RoundToInt(Damage);
                    hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                    Debug.Log($"[{displayName}] Hit {hit.owner.name} for {amt} dmg");
                }
            }
        );

        // 3) capture new projectiles and apply lifetime + ignoreEnvironment
        float lifetime = RPNEvaluator.SafeEvaluateFloat(
            lifetimeExpr,
            new Dictionary<string, float> {
                { "power", owner.spellPower },
                { "wave",  GetCurrentWave()    }
            },
            1f
        );

        var after = Object
            .FindObjectsByType<ProjectileController>(FindObjectsSortMode.None)
            .ToList();

        foreach (var ctrl in after.Except(before))
        {
            ctrl.SetLifetime(lifetime);
            ctrl.ignoreEnvironment = true;  // now walls won't stop it
        }

        yield return null;
    }
}
