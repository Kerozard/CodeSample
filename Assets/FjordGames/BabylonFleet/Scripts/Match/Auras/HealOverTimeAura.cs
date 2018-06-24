using System.Collections;
using FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.Auras
{
    public class HealOverTimeAura : Aura
    {
        #region Fields

        [SerializeField]
        private Modifier _regenerationIntervalSeconds;

        [SerializeField]
        private Modifier _regenerationPerInterval;

        #endregion

        #region Properties

        /// <summary> Modifies the interval at which health is regained. </summary>
        public Modifier regenerationIntervalSeconds { get { return _regenerationIntervalSeconds; } }

        /// <summary> Modifies how much health is regained each interval. </summary>
        public Modifier regenerationPerInterval { get { return _regenerationPerInterval; } }

        #endregion

        #region Protected Methods

        /// <summary> Applies the effect to the engine auras component. </summary>
        protected override IEnumerator ApplyEffect()
        {
            if (!GetAurasComponent<HealOverTimeAuraManager>())
                yield break;

            if (_ticks == 1)
                _auraManager.AddAura(this);
            else
                _auraManager.ApplyAuras();
        }

        #endregion
    }
}