using System.Collections;
using FjordGames.BabylonFleet.Scripts.Match.Components.AuraManagers;
using FjordGames.BabylonFleet.Scripts.Match.Models;
using UnityEngine;

namespace FjordGames.BabylonFleet.Scripts.Match.Components.Auras
{
    public class EngineAura: Aura
    {
        #region Fields

        /// <summary> Defines the mass of the entity (applied in the rigidbody2d). </summary>
        [Header("Mass")]
        [SerializeField]
        protected Modifier _mass;

        /// <summary> The forward acceleration of the engine. </summary>
        [Header("Forward / Backward Movement")]
        [SerializeField]
        protected  Modifier _forwardAcceleration;

        /// <summary> The backward acceleration of the engine. </summary>
        [SerializeField]
        protected  Modifier _backwardAcceleration;

        /// <summary> The maximum forward speed of the engine. </summary>
        [SerializeField]
        protected  Modifier _maximumVelocity;

        /// <summary> The drag of the engine (applied to the rigidbody2d). </summary>
        [SerializeField]
        protected  Modifier _drag;

        /// <summary> The maximum lateral speed of the engine. </summary>
        [Header("Sideways / Lateral Movement")]
        [SerializeField]
        protected  Modifier _lateralMaximumVelocity;

        /// <summary> The lateral acceleration of the engine. </summary>
        [SerializeField]
        protected  Modifier _lateralAcceleration;

        /// <summary> The rotation acceleration of the engine. </summary>
        [Header("Rotation")]
        [SerializeField]
        protected  Modifier _angularAcceleration;

        /// <summary> The maximum rotation speed of the engine (in degrees). </summary>
        [SerializeField]
        protected  Modifier _angularMaximumVelocity;

        /// <summary> The angular drag of the engine. </summary>
        [SerializeField]
        protected  Modifier _angularDrag;

        #endregion

        #region Properties

        /// <summary> The mass of the entity (applied in the rigidbody2d). </summary>
        public Modifier mass { get { return _mass; } }

        /// <summary> The forward acceleration of the engine. </summary>
        public Modifier forwardAcceleration { get { return _forwardAcceleration; } }

        /// <summary> The backward acceleration of the engine. </summary>
        public Modifier backwardAcceleration { get { return _backwardAcceleration; } }

        /// <summary> The maximum forward speed of the engine. </summary>
        public Modifier maximumVelocity { get { return _maximumVelocity; } }

        /// <summary> The drag of the engine (applied to the rigidbody2d). </summary>
        public Modifier drag { get { return _drag; } }

        /// <summary> The lateral acceleration of the engine. </summary>
        public Modifier lateralAcceleration { get { return _lateralAcceleration; } }

        /// <summary> The maximum lateral speed of the engine. </summary>
        public Modifier lateralMaximumVelocity { get { return _lateralMaximumVelocity; } }

        /// <summary> The rotation acceleration of the engine. </summary>
        public Modifier angularAcceleration { get { return _angularAcceleration; } }

        /// <summary> The maximum rotation speed of the engine. </summary>
        public Modifier angularMaximumVelocity { get { return _angularMaximumVelocity; } }

        /// <summary> The angular drag of the engine. </summary>
        public Modifier angularDrag { get { return _angularDrag; } }

        #endregion

        #region Public Methods

        /// <summary> Configures the lateral (left/right) movement values. </summary>
        public virtual void ConfigureLateralMovement(Modifier acceleration, Modifier velocityMaximum)
        {
            _lateralAcceleration = acceleration;
            _lateralMaximumVelocity = velocityMaximum;
        }

        /// <summary> Configures the longitudinal (forward/backward) movement values. </summary>
        public virtual void ConfigureLongitudinalMovement(Modifier accelerationForward, Modifier accelerationBackward, Modifier velocityMaximum)
        {
            _forwardAcceleration = accelerationForward;
            _backwardAcceleration = accelerationBackward;
            _maximumVelocity = velocityMaximum;
        }

        /// <summary> Configures the angular (rotation) movement values. </summary>
        public virtual void ConfigureAngularMovement(Modifier acceleration, Modifier velocityMaximum)
        {
            _angularAcceleration = acceleration;
            _angularMaximumVelocity = velocityMaximum;
        }

        /// <summary> Configures the rigidbody values. </summary>
        public virtual void ConfigureRigidbody2D(Modifier rigidbodyMass, Modifier rigidbodyDrag, Modifier rigidBodyangularDrag)
        {
            _mass = rigidbodyMass;
            _drag = rigidbodyDrag;
            _angularDrag = rigidBodyangularDrag;
        }

        #endregion

        #region Protected Methods

        /// <summary> Applies the effect to the engine auras component. </summary>
        protected override IEnumerator ApplyEffect()
        {
            if (!GetAurasComponent<EngineAuraManager>())
                yield break;

            if (_ticks == 1)
                _auraManager.AddAura(this);
            else
                _auraManager.ApplyAuras();
        }

        #endregion
    }
}