using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace NecrobinderExpansion.Powers;

public sealed class DeathmatchPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == CombatSide.Player)
		{
			if (participants.Contains(base.Owner))
			{
				if (base.Owner.Player.IsOstyAlive)
				{
					await CreatureCmd.Kill(base.Owner.Player.Osty);
				}
			}
			await PowerCmd.Decrement(this);
		}
		
    }
}
