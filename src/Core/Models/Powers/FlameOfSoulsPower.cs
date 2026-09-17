using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace NecrobinderExpansion.Powers;

public sealed class FlameOfSoulsPower : PowerModel
{
	private class Data
	{
		public int soulsPlayed;

		public int triggerCount;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => base.Amount - GetInternalData<Data>().soulsPlayed % base.Amount;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<Soul>(),
        HoverTipFactory.ForEnergy(this)
    };

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
        new EnergyVar(1)
	};

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card is Soul && cardPlay.Card.Owner.Creature == base.Owner)
		{
			Data data = GetInternalData<Data>();
			data.soulsPlayed += 1;
			int triggers = data.soulsPlayed / base.Amount - data.triggerCount;
			if (triggers > 0)
			{
				Flash();
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue * triggers, base.Owner.Player);
				data.triggerCount += triggers;
			}
			InvokeDisplayAmountChanged();
		}
	}
}
