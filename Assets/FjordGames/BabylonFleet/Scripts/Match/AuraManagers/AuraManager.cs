using System.Collections.Generic;
using System.Linq;
using FjordGames.BabylonFleet.Scripts.Common.Components;
using FjordGames.BabylonFleet.Scripts.Match.Components.Auras;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers
{
    public abstract class AuraManager : ToolkitMonoBehaviour
    {
        #region Fields

        protected List<Aura> _auras = new List<Aura>();

        #endregion

        #region Public Methods

        public virtual void ApplyAura(Aura aura)
        {
            if (!_auras.Contains(aura))
                _auras.Add(aura);

            ApplyAuras();
        }

        public virtual void EndAllAuras()
        {
            _auras.ForEach(a => a.EndEffect());
        }

        #endregion

        #region Public Methods

        public virtual void ApplyAuras()
        {

        }

        /// <summary> Adds a new health aura to those managed by this script. </summary>
        public virtual void AddAura(Aura aura, bool allowDuplicate = false)
        {
            if (_auras.Contains(aura) && !allowDuplicate)
                return;

            _auras.Add(aura);
            aura.transform.SetParent(transform);
            aura.gameObject.SetActive(true);
            SubscribeToAuraDelegates(aura);
            ApplyAuras();
        }

        /// <summary> Removes all auras managed by this script. </summary>
        public virtual void ClearAuras()
        {
            _auras.Clear();
            ApplyAuras();
        }

        /// <summary> Removes a health aura from those managed by this script. </summary>
        public virtual void RemoveAura(Aura aura)
        {
            if (!_auras.Contains(aura))
                return;

            _auras.Remove(aura);
            UnsubscribeFromAuraDelegates(aura);
            ApplyAuras();
        }

        /// <summary> Returns a new list of all health auras currently managed by this script. </summary>
        public virtual List<Aura> GetAuras()
        {
            return new List<Aura>(_auras);
        }

        #endregion

        #region Protected Methods

        /// <summary> Applies modifiers from a given list to a value and returns the result. </summary>
        protected virtual float ApplyModifiers(float baseValue, IEnumerable<Modifier> modifiers)
        {
            if (modifiers == null)
                return baseValue;

            var modifierList = modifiers.ToList();

            var result = baseValue + modifierList.Sum(m => m.value);
            result = result * (1 + modifierList.Sum(m => m.additivePercentage));
            result = modifierList.Aggregate(result, (current, modifier) => current * modifier.multiplicativePercentage);

            var minimumValue = (modifierList.Any(m => m.forceMinimum))
                ? modifierList.Where(m => m.forceMinimum).Min(m => m.minimumValue)
                : result;

            var maximumValue = (modifierList.Any(m => m.forceMaximum))
                ? modifierList.Where(m => m.forceMaximum).Max(m => m.maximumValue)
                : result;

            result = Mathf.Clamp(result, minimumValue, maximumValue);

            return result;
        }

        /// <summary> Applies a single modifier to a value and returns the result. </summary>
        protected virtual float ApplyModifier(float baseValue, Modifier modifier)
        {
            if (modifier == null)
                return baseValue;

            var result = baseValue + modifier.value;
            result *= (1 + modifier.additivePercentage);
            result *= modifier.multiplicativePercentage;

            var minimum = (modifier.forceMinimum) ? modifier.minimumValue : result;
            var maximum = (modifier.forceMaximum) ? modifier.maximumValue : result;

            result = Mathf.Clamp(result, minimum, maximum);

            return result;
        }

        protected void OnAuraEnded(object sender, AuraEventArgs e)
        {
            var aura = (Aura) sender;
            UnsubscribeFromAuraDelegates(aura);
        }

        protected virtual void SubscribeToAuraDelegates(Aura aura)
        {
            aura.auraEnded += OnAuraEnded;
        }

        protected virtual void UnsubscribeFromAuraDelegates(Aura aura)
        {
            aura.auraEnded -= OnAuraEnded;
        }

        #endregion
    }
}