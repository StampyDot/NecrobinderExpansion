using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace NecrobinderExpansion.Cards;

public sealed class MuscleMemory : CardModel
{
	private decimal _extraDamage;
	private Dictionary<CardPlay, decimal> _pendingDamage = new();

	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.OstyAttack };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new OstyDamageVar(18m, ValueProp.Move)
	};

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
	{
		CardKeyword.Exhaust
	};

	private decimal ExtraDamage
	{
		get => _extraDamage;
		set
		{
			AssertMutable();
			_extraDamage = value;
		}
	}

	public MuscleMemory()
		: base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_pendingDamage = new Dictionary<CardPlay, decimal>();
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue)
				.FromOsty(base.Owner.Osty, this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.Owner != base.Owner || card.Type != CardType.Attack || !card.Tags.Contains(CardTag.OstyAttack))
		{
			return Task.CompletedTask;
		}

		if (CombatManager.Instance.History.CardPlaysStarted.Any((CardPlayStartedEntry entry) => entry.HappenedThisTurn(base.CombatState) && entry.CardPlay.Card.Owner == base.Owner && entry.CardPlay.Card.Type == CardType.Attack && entry.CardPlay.Card.Tags.Contains(CardTag.OstyAttack)))
		{
			return Task.CompletedTask;
		}

		decimal damage = GetCurrentFaceDamage(card);
		_pendingDamage[cardPlay] = damage;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_pendingDamage.Remove(cardPlay, out decimal damage))
		{
			base.DynamicVars.OstyDamage.BaseValue += damage;
			ExtraDamage += damage;
		}

		return Task.CompletedTask;
	}

	private decimal GetCurrentFaceDamage(CardModel card)
	{
		decimal damage;
		if (card.DynamicVars.ContainsKey("CalculatedDamage"))
		{
			damage = card.DynamicVars.CalculatedDamage.Calculate(null);
		}
		else if (card.DynamicVars.ContainsKey("OstyDamage"))
		{
			damage = card.DynamicVars.OstyDamage.BaseValue;
		}
		else if (card.DynamicVars.ContainsKey("Damage"))
		{
			damage = card.DynamicVars.Damage.BaseValue;
		}
		else
		{
			Log.Warn($"{base.Id.Entry} observed Osty Attack card {card.Id.Entry}, but it did not have an appropriate damage var!");
			return 0m;
		}

		return Hook.ModifyDamage(
			base.Owner.RunState,
			base.Owner.Creature.CombatState,
			null,
			base.Owner.Osty,
			damage,
			ValueProp.Move,
			card,
			null,
			ModifyDamageHookType.All,
			CardPreviewMode.None,
			out IEnumerable<AbstractModel> _);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.OstyDamage.BaseValue += ExtraDamage;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(6m);
	}
}
