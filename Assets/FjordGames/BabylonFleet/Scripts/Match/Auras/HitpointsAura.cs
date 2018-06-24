using System.Collections;
using FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.Auras
{
    /// <summary> Defines an aura that modifies health values (start/maximum) of a health controller. </summary>
    public class HitpointsAura : Aura
    {
        #region Fields

        [Header("Health Modifications")]
        [SerializeField]
        private Modifier _health = new Modifier();

        [SerializeField]
        private Modifier _maximum = new Modifier();

        #endregion

        #region Properties

        public Modifier health { get { return _health; } }

        public Modifier maximum { get { return _maximum; } }

        #endregion

        #region Public Methods

        protected override IEnumerator ApplyEffect()
        {
            if (!GetAurasComponent<HitpointsAuraManager>())
                yield break;

            if (_ticks == 1)
                _auraManager.AddAura(this);
            else
                _auraManager.ApplyAuras();

        }

        #endregion
    }
}