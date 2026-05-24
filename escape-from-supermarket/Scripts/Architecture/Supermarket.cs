using System.Collections.Generic;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using EscapeFromSupermarket.Systems;
using EscapeFromSupermarket.Utilities;
using QFramework;

namespace EscapeFromSupermarket.Architecture
{
    public class Supermarket : Architecture<Supermarket>
    {
        public static Supermarket Instance => (Supermarket)Interface;

        private readonly List<ITickable> _tickables = new();
        private readonly List<ITickable> _pendingAdd = new();
        private readonly List<ITickable> _pendingRemove = new();
        private bool _isTicking;

        protected override void Init()
        {
            this.RegisterUtility(new ProductCatalog());
            this.RegisterModel(new CartModel());
            this.RegisterModel(new ShelfModel());
            this.RegisterModel(new GameStateModel());
            this.RegisterModel(new GuardModel());
            this.RegisterSystem(new MovementSystem());

            var timerSystem = new TimerSystem();
            this.RegisterSystem(timerSystem);
            RegisterTickable(timerSystem);

            var extractionSystem = new ExtractionSystem();
            this.RegisterSystem(extractionSystem);
            RegisterTickable(extractionSystem);
        }

        public void RegisterTickable(ITickable tickable)
        {
            if (tickable == null) return;
            if (_isTicking)
            {
                _pendingRemove.Remove(tickable);
                if (!_tickables.Contains(tickable) && !_pendingAdd.Contains(tickable))
                {
                    _pendingAdd.Add(tickable);
                }
            }
            else if (!_tickables.Contains(tickable))
            {
                _tickables.Add(tickable);
            }
        }

        public void UnregisterTickable(ITickable tickable)
        {
            if (tickable == null) return;
            if (_isTicking)
            {
                _pendingAdd.Remove(tickable);
                if (!_pendingRemove.Contains(tickable)) _pendingRemove.Add(tickable);
            }
            else
            {
                _tickables.Remove(tickable);
            }
        }

        public void Tick(double delta)
        {
            _isTicking = true;
            try
            {
                for (int i = 0; i < _tickables.Count; i++)
                {
                    _tickables[i].Tick(delta);
                }
            }
            finally
            {
                _isTicking = false;
                FlushPending();
            }
        }

        private void FlushPending()
        {
            if (_pendingRemove.Count > 0)
            {
                for (int i = 0; i < _pendingRemove.Count; i++) _tickables.Remove(_pendingRemove[i]);
                _pendingRemove.Clear();
            }
            if (_pendingAdd.Count > 0)
            {
                for (int i = 0; i < _pendingAdd.Count; i++)
                {
                    var t = _pendingAdd[i];
                    if (!_tickables.Contains(t)) _tickables.Add(t);
                }
                _pendingAdd.Clear();
            }
        }
    }
}
