using System.Collections;
using FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers;
using FjordGames.BabylonFleet.Scripts.Match.EventArguments;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.Auras
{
    /// <summary> The base class for auras (buffs/debuffs) that can be applied to ship  subsystems. </summary>
    public abstract class Aura : Entity
    {
        #region Fields

        [Header("Base Aura Settings")]
        [SerializeField]
        protected float _duration;

        [SerializeField]
        protected float _startDelay;

        [SerializeField]
        protected float _interval;

        protected float _remainingDuration;

        protected float _nextTick;

        protected bool _isActive;

        protected SceneEntity _entity;

        protected int _ticks;

        protected AuraManager _auraManager;

        #endregion

        #region Delegates

        public delegate void AuraEndedHandler(object sender, AuraEventArgs e);

        public delegate void AuraStartedHandler(object sender, AuraEventArgs e);

        public delegate void ApplyingEffectHandler(object sender, AuraEventArgs e);

        public delegate void EffectAppliedHandler(object sender, AuraEventArgs e);

        #endregion

        #region Events

        public event AuraEndedHandler auraEnded;

        public event AuraStartedHandler auraStarted;

        public event ApplyingEffectHandler applyingEffect;

        public event EffectAppliedHandler effectApplied;

        #endregion

        #region Properties

        /// <summary> The duration of the aura. If it is 0, then the aura has to be ended manually by calling EndEffect(). </summary>
        public float duration { get { return _duration; } }

        /// <summary> The startDelay before the aura applies the effect for the first time. </summary>
        public float startDelay { get { return _startDelay; } }

        /// <summary> The interval in which the effect is applied. </summary>
        public float interval { get { return _interval; } }

        /// <summary> The remaining duration before the aura ends. </summary>
        public float remainingDuration { get { return _remainingDuration; } }

        public SceneEntity entity { get { return _entity; } }

        #endregion

        #region MonoBehaviour API

        protected override void OnEnable()
        {
            base.OnEnable();
            _entity = GetComponentInParent<SceneEntity>();
            _remainingDuration = _duration;
            _isActive = true;

            if (_startDelay > 0)
                StartCoroutine(StartAfterDelay());
            else if (_interval > 0)
                StartCoroutine(ApplyEffectRepeating());
            else
                StartCoroutine(ExecuteApplyEffect());

            if (duration > 0)
                StartCoroutine(EndAfterDelay());
        }
        #endregion

        #region Coroutines

        protected virtual IEnumerator ExecuteApplyEffect()
        {
            var eventArgs = AuraEventArgs.Empty;
            OnApplyingEffect(eventArgs);

            if (eventArgs.Cancel)
            {
                EndEffect();
                yield break;
            }

            _ticks++;
            yield return ApplyEffect();
            OnEffectApplied(eventArgs);
        }

        protected virtual IEnumerator EndAfterDelay()
        {
            yield return new WaitForSeconds(_startDelay + _duration);
            EndEffect();
        }

        protected virtual IEnumerator StartAfterDelay()
        {
            yield return new WaitForSeconds(_startDelay);
            StartCoroutine(_interval > 0 ? ApplyEffectRepeating() : ExecuteApplyEffect());
        }

        protected virtual IEnumerator ApplyEffectRepeating()
        {
            while (_isActive)
            {
                if (_nextTick <= 0 && _remainingDuration > 0)
                {
                    StartCoroutine(ExecuteApplyEffect());
                    _nextTick += _interval;
                }

                _nextTick -= Time.deltaTime;
                _remainingDuration -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary> Should be replaced by the actual aura effect. </summary>

        protected virtual IEnumerator ApplyEffect()
        {
            yield break;
        }

        #endregion

        #region Public Methods

        /// <summary> Ends the aura by stopping all coroutines and destroying the game object. </summary>
        public virtual void EndEffect()
        {
            _isActive = false;

            StopCoroutine(ExecuteApplyEffect());
            StopCoroutine(StartAfterDelay());
            StopCoroutine(ApplyEffectRepeating());
            StopCoroutine(ApplyEffect());
            StopCoroutine(EndAfterDelay());

            Destroy(gameObject);
        }

        #endregion

        #region Protected Methods

        protected virtual void OnAuraEnded(AuraEventArgs e)
        {
            if (auraEnded != null)
                auraEnded(this, e);
        }

        protected virtual void OnAuraStarted(AuraEventArgs e)
        {
            if (auraStarted != null)
                auraStarted(this, e);
        }

        protected virtual void OnApplyingEffect(AuraEventArgs e)
        {
            if (applyingEffect != null)
                applyingEffect(this, e);
        }

        protected virtual void OnEffectApplied(AuraEventArgs e)
        {
            if (effectApplied != null)
                effectApplied(this, e);
        }

        /// <summary> Validates the existence of an engine auras component.</summary>
        protected virtual bool GetAurasComponent<T>() where T : AuraManager
        {
            if (_auraManager == null)
            {
                _auraManager = GetComponentInParent<T>();
                if (_auraManager == null)
                {
                    Debug.LogWarning(string.Format("{0} could not find an auras component of type {1}. Aura will not be applied.", name, typeof(T).Name));
                    EndEffect();
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}