using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Systems
{
	public class TimerSystem : AbstractSystem, ITickable
	{
		private GameStateModel _gameState;

		protected override void OnInit()
		{
			_gameState = this.GetModel<GameStateModel>();
		}

		public void Tick(double delta)
		{
			if (_gameState == null || _gameState.State.Value != RoundResult.Running) return;

			var nextCountdown = Mathf.Max(0.0f, _gameState.Countdown.Value - (float)delta);
			_gameState.Countdown.Value = nextCountdown;

			if (nextCountdown <= 0.0f)
			{
				this.SendCommand(EndRoundCommand.Lose(LossReason.Timeout));
			}
		}
	}
}
