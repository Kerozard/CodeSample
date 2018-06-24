using System.Collections.Generic;
using System.Linq;
using FjordGames.BabylonFleet.Scripts.Match.Components.Auras;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;
using FjordGames.BabylonFleet.Scripts.Match.ScriptableObjects.Configuration;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers
{
    public class EngineAuraManager : AuraManager
    {
        #region Fields

        /// <summary> Defines the engine that is being modified. </summary>
        protected Engine _engine;

        #endregion

        #region MonoBehaviour API

        protected virtual void OnEnable()
        {
            SetReferences();
            Validate();

            if (!_isValid)
                return;

            SubscribeToEngineDelegates();
            if (_auras.Any())
            {
                _auras.ForEach(SubscribeToAuraDelegates);
                ApplyAuras();
            }
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEngineDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEngineDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        #endregion

        #region Public Methods

        /// <summary> Applies the managed auras to the engine. </summary>
        public override void ApplyAuras()
        {
            if (_engine == null)
            {
                Debug.LogWarning(string.Format("Engine auras component on {0} could not find an engine to modify.", name));
                return;
            }
            ApplyAuras((EngineSetup)_engine.configuration.Clone());
        }

        #endregion

        #region Protected Methods

        protected virtual void ApplyAuras(EngineSetup configuration)
        {
            var auras = _auras.Cast<EngineAura>().ToList();

            ApplyLongitudinalAuraModifiers(configuration, auras);
            ApplyLateralAuraModifiers(configuration, auras);
            ApplyAngularAuraModifiers(configuration, auras);
            ApplyRigidbody2DAuraModifiers(configuration, auras);

            _engine.ApplyConfigurationToEngine(configuration);
        }

        protected virtual void ApplyRigidbody2DAuraModifiers(EngineSetup configuration, List<EngineAura> auras)
        {
            var mass = ApplyModifiers(configuration.mass, auras.Select(a => a.mass));
            var drag = ApplyModifiers(configuration.drag, auras.Select(a => a.drag));
            var angularDrag = ApplyModifiers(configuration.angularDrag, auras.Select(a => a.angularDrag));
            configuration.SetRigidbody2DValues(mass, drag, angularDrag);
        }

        protected virtual void ApplyAngularAuraModifiers(EngineSetup configuration, List<EngineAura> auras)
        {
            var angularAcceleration = ApplyModifiers(configuration.angularAcceleration, auras.Select(a => a.angularAcceleration));
            var angularMaximumVelocity = ApplyModifiers(configuration.angularMaximumVelocity, auras.Select(a => a.angularMaximumVelocity));
            configuration.SetAngularMovement(angularAcceleration, angularMaximumVelocity);
        }

        protected virtual void ApplyLateralAuraModifiers(EngineSetup configuration, List<EngineAura> auras)
        {
            var lateralAcceleration = ApplyModifiers(configuration.lateralAcceleration, auras.Select(a => a.lateralAcceleration));
            var lateralMaximumVelocity = ApplyModifiers(configuration.lateralMaximumVelocity, auras.Select(a => a.lateralMaximumVelocity));
            configuration.SetLateralMovement(lateralAcceleration, lateralMaximumVelocity);
        }

        protected virtual void ApplyLongitudinalAuraModifiers(EngineSetup configuration, List<EngineAura> auras)
        {
            var forwardAcceleration = ApplyModifiers(configuration.forwardAcceleration, auras.Select(a => a.forwardAcceleration));
            var backwardAcceleration = ApplyModifiers(configuration.backwardAcceleration, auras.Select(a => a.backwardAcceleration));
            var forwardMaximumVelocity = ApplyModifiers(configuration.forwardMaximumVelocity, auras.Select(a => a.maximumVelocity));
            var backwardMaximumVelocity = ApplyModifiers(configuration.backwardMaximumVelocity, auras.Select(a => a.maximumVelocity));
            configuration.SetLongitudinalMovement(forwardAcceleration, forwardMaximumVelocity, backwardAcceleration, backwardMaximumVelocity);
        }

        protected virtual void OnEngineDestroying(object sender, ToolkitEventArgs e)
        {
            _engine = null;
            UnsubscribeFromEngineDelegates();
        }

        protected virtual void SetReferences()
        {
            if (_engine == null)
                _engine = GetComponentInParent<Engine>();
        }

        protected virtual void SubscribeToEngineDelegates()
        {
            if (_engine == null)
                return;

            _engine.destroying += OnEngineDestroying;
        }

        protected virtual void UnsubscribeFromEngineDelegates()
        {
            if (_engine == null)
                return;

            _engine.destroying -= OnEngineDestroying;
        }

        protected virtual void Validate()
        {
            _isValid = true;

            if (_engine == null)
                _isValid = false;
        }

        #endregion
    }
}