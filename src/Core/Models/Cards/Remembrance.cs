using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Cards;

public sealed class Remembrance : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
	{
		CardKeyword.Exhaust
	};

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
    };

	public Remembrance()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
		CardPile pile = PileType.Exhaust.GetPile(base.Owner);
        IReadOnlyList<CardModel> cards = pile.Cards.Where((CardModel c) => c.Keywords.Contains(CardKeyword.Ethereal)).ToList();
		CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, base.Owner, prefs)).FirstOrDefault();
		if (cardModel != null)
		{
            if (base.IsUpgraded)
		{
			cardModel.SetToFreeThisTurn();
		}
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}

}
