using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace NecrobinderExpansion.Cards;

public sealed class GetReady : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(8m, ValueProp.Move),
		new PowerVar<VigorPower>(3m)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    { 
        HoverTipFactory.FromPower<VigorPower>() 
    };

	public GetReady()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        if (base.Owner.IsOstyAlive)
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Osty, base.DynamicVars["VigorPower"].IntValue, base.Owner.Creature, this);
        }
}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
		base.DynamicVars["VigorPower"].UpgradeValueBy(1m);
	}
}
