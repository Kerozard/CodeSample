using System.Collections;
using System.Linq;
using FjordGames.BabylonFleet.Scripts.Common.Models.EditorScripts;
using FjordGames.BabylonFleet.Scripts.Match.Components.Auras;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using FjordGames.BabylonFleet.Scripts.Match.ScriptableObjects.Definitions;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers
{
    /// <summary> Provides heal-over-time functionality for the associated health controller. Heals can be augmented by heal-over-time auras. 
    /// Has to be added on or below a game object with a health controller. </summary>
    public class HealOverTimeAuraManager : AuraManager
    {
        #region Fields

        [Header("Manual References & Settings (Required)")]
        [SerializeField]
        private float _intervalSeconds;

        [SerializeField]
        private float _health;

        [Header("Manual References & Settings (Optional)")]
        [SerializeField]
        private GameEventType _regenerationEvent;

        [SerializeField]
        private bool _regenerateDuringDepletion;

        [Header("Automatic References & Settings (Required)")]
        [SerializeField]
        private Hitpoints _hitpointsController;

        [Header("Display Values")]
        [FgReadOnly][SerializeField]
        private bool _isRegenerating;

        [FgReadOnly][SerializeField]
        private float _interval;

        [FgReadOnly][SerializeField]
        private float _regeneration;


        private float _nextRegeneration;
        #endregion

        #region MonoBehaviour API

        protected virtual void OnEnable()
        {
            SetReferences();
            Validate();

            if (!_isValid)
                return;

            SubscribeToHealthControllerDelegates();
            _auras.ForEach(SubscribeToAuraDelegates);
            UpdateRegenerationValues();
            StartRegeneration();
        }

        protected virtual void OnDisable()
        {
            StopRegeneration();
            UnsubscribeFromHealthControllerDelegates();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopRegeneration();
            UnsubscribeFromHealthControllerDelegates();
        }

        #endregion

        #region Coroutines

        /// <summary> Performs the automatic regeneration. </summary>
        protected virtual IEnumerator Regenerate()
        {
            _isRegenerating = true;
            while (_isRegenerating && _interval > 0)
            {
                _nextRegeneration += _interval;
                UpdateRegenerationValues();
                while (_nextRegeneration > 0)
                {
                    yield return new WaitForEndOfFrame();
                    _nextRegeneration -= Time.deltaTime;
                }               
                if (_isRegenerating)
                {
                    UpdateRegenerationValues();
                    var heal = new Heal(_regeneration, null, _regenerationEvent);
                    _hitpointsController.Heal(heal, _hitpointsController.entity);
                }
            }
            _isRegenerating = false;
            _nextRegeneration = 0;
        }

        #endregion

        #region Public Methods
        
        /// <summary> Starts the regeneration process. </summary>
        public virtual void StartRegeneration()
        {
            StartCoroutine(Regenerate());
        }

        /// <summary> Stops the regeneration process. </summary>
        public virtual void StopRegeneration()
        {
            if (!_isRegenerating)
                return;
            
            _isRegenerating = false;
            StopCoroutine(Regenerate());
        }

        #endregion

        #region Protected Methods

        protected virtual void EvaluateRegenerationStatus()
        {
            if (_hitpointsController == null)
            {
                _isRegenerating = false;
                return;
            }

            if (enabled && !_isRegenerating && (!_hitpointsController.isDepleted || _regenerateDuringDepletion))
                StartCoroutine(Regenerate());
        }

        protected virtual void EvaluateRegenerationStatus(object sender, HealthEventArgs e)
        {
            EvaluateRegenerationStatus();
        }

        protected virtual void HitpointsControllerDestroying(object sender, ToolkitEventArgs e)
        {
            UnsubscribeFromHealthControllerDelegates();
            StopRegeneration();
            _hitpointsController = null;
        }

        protected virtual void OnAuraExpired(object sender, AuraEventArgs e)
        {
            var aura = (HealOverTimeAura) sender;
            RemoveAura(aura);
        }

        protected virtual void SetReferences()
        {
            if (_hitpointsController == null)
                _hitpointsController = GetComponentInParent<Hitpoints>();
        }

        protected virtual void SubscribeToAuraDelegates(HealOverTimeAura aura)
        {
            aura.auraEnded += OnAuraExpired;
        }

        protected virtual void SubscribeToHealthControllerDelegates()
        {
            _hitpointsController.depleted += EvaluateRegenerationStatus;
            _hitpointsController.depletionEnded += EvaluateRegenerationStatus;
            _hitpointsController.destroying += HitpointsControllerDestroying;
        }

        protected virtual void UpdateRegenerationValues()
        {
            if (!_auras.Any())
            {
                _interval = _intervalSeconds;
                _regeneration = _health;
            }
            else
            {
                var auras = _auras.Cast<HealOverTimeAura>().ToList();
                _interval = ApplyModifiers(_intervalSeconds, auras.Select(m => m.regenerationIntervalSeconds));
                _regeneration = ApplyModifiers(_health, auras.Select(m => m.regenerationPerInterval));
            }

            _nextRegeneration = (_isRegenerating) ? Mathf.Min(_interval, _nextRegeneration) : 0;
        }

        protected virtual void UnsubscribeFromAuraDelegates(HealOverTimeAura aura)
        {
            aura.auraEnded -= OnAuraExpired;
        }

        protected virtual void UnsubscribeFromHealthControllerDelegates()
        {
            _hitpointsController.depleted -= EvaluateRegenerationStatus;
            _hitpointsController.depletionEnded -= EvaluateRegenerationStatus;
            _hitpointsController.destroying -= HitpointsControllerDestroying;
        }

        /// <summary> Validates the script setup. </summary>
        protected virtual void Validate()
        {
            _isValid = true;
            if (_hitpointsController == null)
            {
                _isValid = false;
            }
        }

        #endregion
    }
}