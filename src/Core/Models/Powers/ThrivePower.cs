using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Powers;

public sealed class ThrivePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonStatic)
	};


	public override decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel source)
	{
		if (amount <= 0m)
		{
			return amount;
		}

		if (summoner.Creature != base.Owner)
		{
			return amount;
		}

		return amount + base.Amount;
	}
}
