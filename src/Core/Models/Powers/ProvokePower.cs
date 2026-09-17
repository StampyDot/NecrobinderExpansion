using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NecrobinderExpansion.Cards;

namespace NecrobinderExpansion.Powers;

public sealed class ProvokePower : PowerModel
{
	private bool _shouldIgnoreNextInstance;

	public override PowerType Type
	{
		get
		{
			if (!IsPositive)
			{
				return PowerType.Debuff;
			}
			return PowerType.Buff;
		}
	}

	public override PowerStackType StackType => PowerStackType.Counter;

	public PowerModel InternallyAppliedPower => ModelDb.Power<ThrivePower>();
    public PowerModel InternallyAppliedPower2 => ModelDb.Power<CalcifyPower>();

	private bool IsPositive => true;

	private int Sign
	{
		get
		{
			if (!IsPositive)
			{
				return -1;
			}
			return 1;
		}
	}

	public override LocString Description => new LocString("powers", IsPositive ? "PROVOKE_POWER.description" : "PROVOKE_DOWN.description");

	protected override string SmartDescriptionLocKey
	{
		get
		{
			if (!IsPositive)
			{
				return "PROVOKE_DOWN.smartDescription";
			}
			return "PROVOKE_POWER.smartDescription";
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonStatic),
		HoverTipFactory.FromCard<Provoke>()
	};

	public void IgnoreNextInstance()
	{
		_shouldIgnoreNextInstance = true;
	}

	public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_shouldIgnoreNextInstance)
		{
			_shouldIgnoreNextInstance = false;
		}
		else
		{
			await PowerCmd.Apply<ThrivePower>(new ThrowingPlayerChoiceContext(), target, (decimal)Sign * amount, applier, cardSource, silent: true);
            await PowerCmd.Apply<CalcifyPower>(new ThrowingPlayerChoiceContext(), target, (decimal)Sign * amount, applier, cardSource, silent: true);
		}
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext,PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!(amount == (decimal)base.Amount) && power == this)
		{
			if (_shouldIgnoreNextInstance)
			{
				_shouldIgnoreNextInstance = false;
			}
			else
			{
				await PowerCmd.Apply<ThrivePower>(choiceContext, base.Owner, (decimal)Sign * amount, applier, cardSource, silent: true);
                await PowerCmd.Apply<CalcifyPower>(choiceContext, base.Owner, (decimal)Sign * amount, applier, cardSource, silent: true);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == base.Owner.Side)
		{
			Flash();
			await PowerCmd.Remove(this);
			await PowerCmd.Apply<ThrivePower>(choiceContext, base.Owner, -Sign * base.Amount, base.Owner, null);
            await PowerCmd.Apply<CalcifyPower>(choiceContext, base.Owner, -Sign * base.Amount, base.Owner, null);
		}
	}
}
