using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using NecrobinderExpansion.Cards;

namespace NecrobinderExpansion.Powers;

public sealed class JoinHandsPower : PowerModel
{
	private const string MaxSummonAmountKey = "MaxSummonAmount";
	public const decimal MaxSummonPerStack = 25m;
	private bool _suppressVfx;
	private bool _visualOnly;

	private sealed class MaxSummonAmountVar : DynamicVar
	{
		public MaxSummonAmountVar()
			: base(MaxSummonAmountKey, MaxSummonPerStack)
		{
		}

		protected override decimal GetBaseValueForIConvertible()
		{
			return _owner is PowerModel power ? MaxSummonPerStack * power.Amount : BaseValue;
		}

		public override string ToString()
		{
			return GetBaseValueForIConvertible().ToString();
		}
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldPlayVfx => !_suppressVfx && base.ShouldPlayVfx;

	public override bool ShouldReceiveCombatHooks => !_visualOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
		new SummonVar(MaxSummonPerStack),
		new MaxSummonAmountVar()
    };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonStatic)
	};

	public static async Task FlashForCombatStart(Player player)
	{
		JoinHandsPower? existingPower = player.Creature.GetPower<JoinHandsPower>();
		if (existingPower != null)
		{
			existingPower.Flash();
			return;
		}

		JoinHandsPower flashPower = (JoinHandsPower)ModelDb.Power<JoinHandsPower>().ToMutable();
		flashPower._visualOnly = true;

		await PowerCmd.Apply(
			new ThrowingPlayerChoiceContext(),
			flashPower,
			player.Creature,
			1m,
			player.Creature,
			null,
			silent: false);

		if (player.Creature.GetPower<JoinHandsPower>() != flashPower)
		{
			return;
		}

		flashPower.Flash();
		await PowerCmd.Remove(flashPower);
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		if (_visualOnly)
		{
			return Task.CompletedTask;
		}

		if (base.Owner?.Player?.Osty == null)
		{
			return Task.CompletedTask;
		}

		decimal savedSummonAmount = Math.Clamp(
			base.Owner.Player.Osty.CurrentHp * base.Amount,
			0m,
			MaxSummonPerStack * base.Amount);

		if (savedSummonAmount > 0m)
		{
			JoinHands.QueueSummon(base.Owner.Player, savedSummonAmount);
			Flash();
		}

		return Task.CompletedTask;
	}

}
