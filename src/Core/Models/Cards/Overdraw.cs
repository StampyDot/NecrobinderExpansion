using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Cards;

public sealed class Overdraw : CardModel
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
        new CardsVar(3),
        new EnergyVar(2)
    };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        base.EnergyHoverTip
    };

	public Overdraw()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		List<CardModel> list = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 999999999), context: choiceContext, player: base.Owner, filter: null, source: this)).ToList();
		foreach (CardModel item in list)
		{
			await CardPileCmd.Add(item, PileType.Draw, CardPilePosition.Random);
		}
		if (list.Count >= base.DynamicVars.Cards.BaseValue)
		{
        	await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
		}
	}

    protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(-1);
	}
}
