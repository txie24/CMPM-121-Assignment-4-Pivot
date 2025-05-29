// File: Assets/Scripts/Spells/ArcaneBurst.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneBurst : Spell
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
    private string radiusExpr;
    private float radius;

    public ArcaneBurst(SpellCaster owner) : base(owner) { }

    public override string DisplayName => displayName;
    public override int IconIndex => iconIndex;

    // RPN‐scaled stats
    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(damageExpr, GetVars(), 30f);
    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(speedExpr, GetVars(), 10f);
    protected override float BaseMana => baseMana;
    protected override float BaseCooldown => baseCooldown;

    private Dictionary<string, float> GetVars() => new()
    {
        { "power", owner.spellPower },
        { "wave",  GetCurrentWave()      }
    };

    private float GetCurrentWave()
    {
        var spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        return spawner != null ? spawner.currentWave : 1;
    }

    // Load everything from JSON exactly like your other spells
    public override void LoadAttributes(JObject j, Dictionary<string, float> vars)
    {
        displayName = j["name"].Value<string>();
        description = j["description"]?.Value<string>() ?? "";
        iconIndex = j["icon"].Value<int>();

        damageExpr = j["damage"]["amount"].Value<string>();
        speedExpr = j["projectile"]["speed"].Value<string>();

        baseMana = RPNEvaluator.SafeEvaluateFloat(j["mana_cost"].Value<string>(), vars, 25f);
        baseCooldown = RPNEvaluator.SafeEvaluateFloat(j["cooldown"].Value<string>(), vars, 4f);

        trajectory = j["projectile"]["trajectory"].Value<string>();
        projectileSprite = j["projectile"]["sprite"]?.Value<int>() ?? 0;

        radiusExpr = j["radius"]?.Value<string>() ?? "2.5";
        radius = RPNEvaluator.SafeEvaluateFloat(radiusExpr, vars, 2.5f);
    }

    // Cast exactly like ArcaneBlast but with an AOE on impact
    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        float dmg = Damage;
        float spd = Speed;
        float rad = radius;
        Vector3 dir = (to - from).normalized;

        GameManager.Instance.projectileManager.CreateProjectile(
            projectileSprite,
            trajectory,
            from,
            dir,
            spd,
            (hit, impactPos) =>
            {
                // direct hit
                if (hit.team != owner.team)
                {
                    int amt = Mathf.RoundToInt(dmg);
                    hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                }

                // explosion AOE
                Collider2D[] hits = Physics2D.OverlapCircleAll(impactPos, rad);
                foreach (var col in hits)
                {
                    Hittable hp = null;
                    var ec = col.GetComponent<EnemyController>();
                    if (ec != null) hp = ec.hp;
                    else
                    {
                        var pc = col.GetComponent<PlayerController>();
                        if (pc != null) hp = pc.hp;
                    }

                    if (hp != null && hp.team != owner.team)
                    {
                        int amt = Mathf.RoundToInt(dmg);
                        hp.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                    }
                }

                // optional debug VFX
                Debug.Log($"[ArcaneBurst] AOE at {impactPos}");
                GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fx.transform.position = impactPos;
                fx.transform.localScale = Vector3.one * rad * 2f;
                var rdr = fx.GetComponent<Renderer>();
                rdr.material.color = new Color(0.7f, 0f, 1f, 0.1f);
                rdr.sortingOrder = 10;
                UnityEngine.Object.Destroy(fx.GetComponent<Collider>());
                UnityEngine.Object.Destroy(fx, 0.3f);
            });

        yield return null;
    }
}
