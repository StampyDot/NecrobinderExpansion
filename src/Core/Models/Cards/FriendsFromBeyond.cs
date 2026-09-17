using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using NecrobinderExpansion.Powers;

namespace NecrobinderExpansion.Cards;

public sealed class FriendsFromBeyond : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromCard<Soul>(),
		HoverTipFactory.Static(StaticHoverTip.Block)
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FriendsFromBeyondPower>(3m)
    };

	public FriendsFromBeyond()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(base.Owner, 1, base.CombatState), PileType.Hand, base.Owner);
		await PowerCmd.Apply<FriendsFromBeyondPower>(choiceContext, base.Owner.Creature, base.DynamicVars["FriendsFromBeyondPower"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}
