using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
	public partial class CustomerController : CharacterBody3D, IController
	{
		[Export] public Vector3 PointA { get; set; }
		[Export] public Vector3 PointB { get; set; }

		private PrototypeBalance _balance = PrototypeBalance.Default;
		private GameStateModel _gameState;
		private Vector3 _target;
		private Vector3 _pushVelocity;

		public override void _Ready()
		{
			_balance = this.GetUtility<PrototypeBalance>();
			_gameState = this.GetModel<GameStateModel>();
			_target = PointB;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_gameState == null || _gameState.State.Value != RoundResult.Running)
			{
				Velocity = Vector3.Zero;
				MoveAndSlide();
				return;
			}

			var toTarget = _target - GlobalPosition;
			toTarget.Y = 0;
			if (toTarget.Length() < _balance.CustomerArriveDistance)
			{
				_target = _target.DistanceSquaredTo(PointA) < 0.01f ? PointB : PointA;
				toTarget = _target - GlobalPosition;
				toTarget.Y = 0;
			}

			var direction = toTarget.LengthSquared() > 0.0001f ? toTarget.Normalized() : Vector3.Zero;
			Velocity = direction * _balance.CustomerSpeed + _pushVelocity;
			MoveAndSlide();

			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				var collision = GetSlideCollision(i);
				if (collision.GetCollider() is PlayerController player)
				{
					player.ApplyCustomerSlow();
					var away = GlobalPosition - player.GlobalPosition;
					away.Y = 0;
					_pushVelocity = away.LengthSquared() > 0.0001f
						? away.Normalized() * _balance.CustomerPushStrength
						: Vector3.Zero;
				}
			}

			_pushVelocity = _pushVelocity.MoveToward(Vector3.Zero, _balance.CustomerPushDecay * (float)delta);
			if (direction.LengthSquared() > 0.0001f)
			{
				float targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
				Rotation = new Vector3(0, Mathf.LerpAngle(Rotation.Y, targetYaw, _balance.CustomerTurnSpeed * (float)delta), 0);
			}
		}

		public IArchitecture GetArchitecture() => Supermarket.Interface;
	}
}
