using System.Linq;
using FjordGames.BabylonFleet.Scripts.Match.Components.Auras;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers
{
    /// <summary> Manages auras that modify incoming heals on a health controller. Has to be added on or below a game object with a health controller. </summary>
    public class HealAuraManager : AuraManager
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

            SubscribeToHealthDelegates();
            if (_auras.Any())
                _auras.ForEach(SubscribeToAuraDelegates);
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromHealthDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromHealthDelegates();
            _auras.ForEach(UnsubscribeFromAuraDelegates);
        }

        #endregion

        #region Protected Methods

        protected virtual void OnHitpointsDestroying(object sender, ToolkitEventArgs e)
        {
            _hitpoints = null;
            UnsubscribeFromHealthDelegates();
        }

        protected virtual void OnHitpointsHealReceiving(object sender, HealthEventArgs e)
        {
            var auras = _auras.Cast<HealAura>().ToList();
            var modifiers = auras.Select(aura => aura.modifier);

            e.SetValue(ApplyModifiers(e.value, modifiers));
        }

        protected virtual void OnAuraExpired(object sender, AuraEventArgs e)
        {
            var aura = (HealAura) sender;
            RemoveAura(aura);
        }

        protected virtual void SetReferences()
        {
            if (_hitpoints == null)
                _hitpoints = GetComponentInParent<Hitpoints>();
        }

        protected virtual void SubscribeToAuraDelegates(HealAura aura)
        {
            aura.auraEnded += OnAuraExpired;
        }

        protected virtual void SubscribeToHealthDelegates()
        {
            if (_hitpoints == null)
                return;

            _hitpoints.destroying += OnHitpointsDestroying;
            _hitpoints.healReceiving += OnHitpointsHealReceiving;
        }

        protected virtual void UnsubscribeFromAuraDelegates(HealAura aura)
        {
            aura.auraEnded -= OnAuraExpired;
        }

        protected virtual void UnsubscribeFromHealthDelegates()
        {
            if (_hitpoints == null)
                return;

            _hitpoints.destroying -= OnHitpointsDestroying;
            _hitpoints.healReceiving -= OnHitpointsHealReceiving;
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