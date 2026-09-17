using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Powers;

public sealed class FuneralPower : PowerModel
{
	private class Data
	{
		public int etherealCount;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.Static(StaticHoverTip.Block)
	};
	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner.Creature == base.Owner)
		{
			if (causedByEthereal)
			{
				GetInternalData<Data>().etherealCount++;
			}
        }
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			Data data = GetInternalData<Data>();
            for (int i = 0; i < data.etherealCount; i++)
            {
                await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
            }
			data.etherealCount = 0;
		}
	}
}
