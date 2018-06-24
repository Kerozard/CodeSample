using System.Collections;
using FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.Auras
{
    /// <summary> Damage auras hold information about buffs and debuffs (e.g. from Powerups) that is applied to damage (either dealing out or receiving). </summary>
    public class DamageAura : Aura
    {
        #region Fields
        [Header("Damage Aura Settings")]
        [SerializeField] protected Modifier _modifier = new Modifier();

        #endregion

        #region Properties

        public Modifier modifier { get { return _modifier; } }

        #endregion

        #region Public Methods

        public virtual void SetModifier(Modifier newModifier)
        {
            _modifier = newModifier;
        }

        #endregion

        #region Protected Methods

        /// <summary> Applies the effect to the engine auras component.</summary>
        protected override IEnumerator ApplyEffect()
        {
            if (!GetAurasComponent<DamageAuraManager>())
                yield break;

            if (_ticks == 1)
                _auraManager.AddAura(this);
            else
                _auraManager.ApplyAuras();
        }

        #endregion
    }
}