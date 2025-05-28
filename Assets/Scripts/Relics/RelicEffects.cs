using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelicEffect
{
    void Activate();
    void Deactivate();
}

public static class RelicEffects
{
    public static IRelicEffect Create(EffectData d, Relic r)
    {
        switch (d.type)
        {
            case "gain-mana":
                return new GainMana(int.Parse(d.amount), r.Name);

            case "gain-health":
                // support fraction (<=1) or flat amount (>1)
                return new GainHealth(float.Parse(d.amount), r.Name);

            case "gain-spellpower":
                if (d.until == "cast-spell")
                    return new GainSpellPowerOnce(int.Parse(d.amount), r.Name);
                if (d.until == "move")
                    return new GainSpellPowerUntilMove(d.amount, r.Name);
                if (d.until == "damage")
                    return new GainSpellPowerUntilDamage(int.Parse(d.amount), r.Name);
                return new GainSpellPower(d.amount, r.Name);

            case "gain-maxhp":
                return new GainMaxHP(int.Parse(d.amount), r.Name);

            case "speed-boost":
                return new SpeedBoost(float.Parse(d.amount), float.Parse(d.duration), r.Name);

            default:
                throw new Exception($"Unknown effect type: {d.type}");
        }
    }

    class GainMana : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public GainMana(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: +{amt} mana");
            pc.GainMana(amt);
        }
        public void Deactivate() { }
    }

    class GainHealth : IRelicEffect
    {
        readonly float amtFraction;
        readonly string relicName;
        public GainHealth(float amt, string name) { amtFraction = amt; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            int heal = (amtFraction <= 1f)
                ? Mathf.RoundToInt(pc.hp.max_hp * amtFraction)
                : Mathf.RoundToInt(amtFraction);
            Debug.Log($"[RelicEffect] “{relicName}”: Healing {heal} HP");
            pc.hp.hp = Mathf.Min(pc.hp.max_hp, pc.hp.hp + heal);
            pc.healthui.SetHealth(pc.hp);
        }
        public void Deactivate() { }
    }

    class SpeedBoost : IRelicEffect
    {
        readonly float multiplier;
        readonly float duration;
        readonly string relicName;
        int originalSpeed;
        Coroutine timer;

        public SpeedBoost(float mult, float dur, string name)
        {
            multiplier = mult;
            duration = dur;
            relicName = name;
        }

        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            if (timer != null) CoroutineManager.Instance.StopCoroutine(timer);

            originalSpeed = pc.speed;
            int boosted = Mathf.RoundToInt(originalSpeed * multiplier);
            Debug.Log($"[RelicEffect] “{relicName}”: Speed x{multiplier} for {duration}s (from {originalSpeed} to {boosted})");
            pc.speed = boosted;
            timer = CoroutineManager.Instance.StartCoroutine(EndBoost());
        }

        IEnumerator EndBoost()
        {
            yield return new WaitForSeconds(duration);
            Deactivate();
        }

        public void Deactivate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: Speed back to {originalSpeed}");
            pc.speed = originalSpeed;
            timer = null;
        }
    }

    // ─── existing spellpower effects ──────────────────────────────
    class GainSpellPower : IRelicEffect
    {
        readonly string formula;
        readonly string relicName;
        public GainSpellPower(string f, string name) { formula = f; relicName = name; }
        public void Activate()
        {
            var vars = new Dictionary<string, int> { { "wave", GameManager.Instance.wavesCompleted } };
            int v = RPNEvaluator.Evaluate(formula, vars);
            Debug.Log($"[RelicEffect] “{relicName}”: +{v} SP (formula: {formula})");
            GameManager.Instance.player.GetComponent<PlayerController>().AddSpellPower(v);
        }
        public void Deactivate() { }
    }

    class GainSpellPowerOnce : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        bool pending;

        public GainSpellPowerOnce(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            if (pending) return;
            pending = true;
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: +{amt} SP (one-shot)");
            pc.AddSpellPower(amt);
            SpellCaster.OnSpellCast += Handle;
        }
        void Handle()
        {
            if (!pending) return;
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: –{amt} SP (removed)");
            pc.AddSpellPower(-amt);
            pending = false;
            SpellCaster.OnSpellCast -= Handle;
        }
        public void Deactivate()
        {
            if (pending)
            {
                SpellCaster.OnSpellCast -= Handle;
                pending = false;
            }
        }
    }

    class GainSpellPowerUntilMove : IRelicEffect
    {
        readonly string formula;
        readonly string relicName;
        int buffAmt;
        bool active;
        public GainSpellPowerUntilMove(string f, string name) { formula = f; relicName = name; }
        public void Activate()
        {
            if (active) return;
            var vars = new Dictionary<string, int> { { "wave", GameManager.Instance.wavesCompleted } };
            buffAmt = RPNEvaluator.Evaluate(formula, vars);
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: +{buffAmt} SP until move");
            pc.AddSpellPower(buffAmt);
            active = true;
        }
        public void Deactivate()
        {
            if (!active) return;
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: –{buffAmt} SP (removed)");
            pc.AddSpellPower(-buffAmt);
            active = false;
        }
    }

    class GainSpellPowerUntilDamage : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public GainSpellPowerUntilDamage(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            Debug.Log($"[RelicEffect] “{relicName}”: +{amt} SP until damaged");
            pc.AddSpellPower(amt);
            EventBus.Instance.OnDamage += OnDamaged;
        }
        void OnDamaged(Vector3 _, Damage __, Hittable t)
        {
            if (t.team == Hittable.Team.PLAYER)
            {
                var pc = GameManager.Instance.player.GetComponent<PlayerController>();
                Debug.Log($"[RelicEffect] “{relicName}”: –{amt} SP (removed)");
                pc.AddSpellPower(-amt);
                EventBus.Instance.OnDamage -= OnDamaged;
            }
        }
        public void Deactivate() { EventBus.Instance.OnDamage -= OnDamaged; }
    }

    class GainMaxHP : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public GainMaxHP(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            pc.relicMaxHPBonus += amt;
            Debug.Log($"[RelicEffect] “{relicName}”: +{amt} max HP bonus (total={pc.relicMaxHPBonus})");
            pc.hp.SetMaxHP(pc.hp.max_hp + amt, true);
            pc.healthui.SetHealth(pc.hp);
        }
        public void Deactivate() { }
    }
}
