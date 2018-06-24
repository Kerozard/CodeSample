using System.Linq;
using FjordGames.BabylonFleet.Scripts.Match.Components.Auras;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers
{
    /// <summary> Manages auras that modify the starting and maximum health of a health controller. 
    /// Has to be added on or below a game object with a health controller. </summary>
    public class HitpointsAuraManager : AuraManager
    {
        #region Fields

        private Hitpoints _hitpoints;

        #endregion

        #region MonoBehaviour API

        protected virtual void OnEnable()
        {
            SetReferences();
            Validate();

            if (!_isValid)
                return;

            SubscribeToHitpointDelegates();
            if (_auras.Any())
            {
                _auras.ForEach(SubscribeToAuraDelegates);
                ApplyAuras();
            }
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromHitpointDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromHitpointDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        #endregion

        #region Public Methods

        public override void ApplyAuras()
        {
            if (_hitpoints == null)
            {
                Debug.LogWarning(string.Format("Health auras component on {0} could not find a health component to modify.", name));
                return;
            }

            ValidateHealthStartingValues();           
            ApplyMaximumHealthModification();
            ApplyHealthModification();
        }

        #endregion

        #region Protected Methods

        protected virtual void ApplyMaximumHealthModification()
        {
            _hitpoints.SetHealthMaximum(GetModifiedMaximumHealth());
        }

        protected virtual void ApplyHealthModification()
        {
            var modifiedHealth = GetModifiedHealth();

            if (modifiedHealth < _hitpoints.health)
            {
                var damageAmount = _hitpoints.health - modifiedHealth;
                _hitpoints.Damage(new Damage(damageAmount));
            }
            else if (modifiedHealth > _hitpoints.health)
            {
                var healAmount = modifiedHealth - _hitpoints.health;
                _hitpoints.Heal(new Heal(healAmount));
            }
        }

        protected virtual float GetModifiedHealth()
        {
            var health = _hitpoints.health;
            var auras = _auras.Cast<HitpointsAura>().ToList();
            health = ApplyModifiers(health, auras.Select(a => a.health));
            return health;
        }

        protected virtual float GetModifiedMaximumHealth()
        {
            var maximumHealth = _hitpoints.setup.healthMaximum;
            var auras = _auras.Cast<HitpointsAura>().ToList();
            maximumHealth = ApplyModifiers(maximumHealth, auras.Select(a => a.maximum));
            return maximumHealth;
        }

        protected virtual void OnHitpointsDestroying(object sender, ToolkitEventArgs e)
        {
            _hitpoints = null;
            UnsubscribeFromHitpointDelegates();
        }

        protected virtual void SetReferences()
        {
            if(_hitpoints == null)
                _hitpoints = GetComponentInParent<Hitpoints>();
        }

        protected virtual void SubscribeToHitpointDelegates()
        {
            if (_hitpoints == null)
                return;

            _hitpoints.destroying += OnHitpointsDestroying;
        }
        
        protected virtual void UnsubscribeFromHitpointDelegates()
        {
            if (_hitpoints == null)
                return;

            _hitpoints.destroying -= OnHitpointsDestroying;
        }

        protected virtual void ValidateHealthStartingValues()
        {
            if (_hitpoints.health <= 0 && _hitpoints.healthMaximum <= 0)
                _hitpoints.SetStartValues(_hitpoints.setup.health, _hitpoints.setup.healthMaximum);
        }

        protected virtual void Validate()
        {
            _isValid = true;

            if (_hitpoints == null)
                _isValid = false;
        }

        #endregion
    }
}