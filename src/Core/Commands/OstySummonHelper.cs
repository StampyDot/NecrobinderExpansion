using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Commands;

internal static class OstySummonHelper
{
	/// <summary>
	/// Summons Osty normally, then restores the HP that the base game's summon command cannot heal
	/// once killing the final enemy has made combat enter its ending state.
	/// </summary>
	public static async Task<SummonResult> Summon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, AbstractModel source)
	{
		bool wasOstyAlive = summoner.IsOstyAlive;
		decimal currentHpBeforeSummon = summoner.Osty?.CurrentHp ?? 0m;
		SummonResult result = await OstyCmd.Summon(choiceContext, summoner, amount, source);

		if (!CombatManager.Instance.IsEnding || result.Amount <= 0m || result.Creature == null)
		{
			return result;
		}

		decimal intendedCurrentHp = wasOstyAlive
			? currentHpBeforeSummon + result.Amount
			: result.Amount;
		await CreatureCmd.SetCurrentHp(result.Creature, intendedCurrentHp);
		return result;
	}
}
