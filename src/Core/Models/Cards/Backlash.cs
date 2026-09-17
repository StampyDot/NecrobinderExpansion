using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NecrobinderExpansion.Cards;

public sealed class Backlash : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
	{
		CardKeyword.Exhaust
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(9m, ValueProp.Move)
	};

	public Backlash()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);

		List<PowerModel> debuffs = base.Owner.Creature.Powers
			.Where((PowerModel power) => power.TypeForCurrentAmount == PowerType.Debuff)
			.ToList();
		if (debuffs.Count == 0)
		{
			return;
		}

		IEnumerable<PowerModel> powersToTransfer = base.IsUpgraded
			? debuffs
			: new PowerModel[] { base.Owner.RunState.Rng.CombatTargets.NextItem(debuffs) };

		foreach (PowerModel power in powersToTransfer.ToList())
		{
			PowerModel transferredPower = (PowerModel)power.ClonePreservingMutability();
			await PowerCmd.Apply(choiceContext, transferredPower, cardPlay.Target, power.Amount, base.Owner.Creature, this);
			await PowerCmd.Remove(power);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
