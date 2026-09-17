using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace NecrobinderExpansion.Powers;

public sealed class DesperateGuardPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override Creature ModifyUnblockedDamageTarget(Creature target, decimal _, ValueProp props, Creature? __)
	{
		if (target != base.Owner)
		{
			return target;
		}

		Creature? osty = base.Owner.Player?.Osty;
		if (osty?.Monster is not Osty || osty.IsDead)
		{
			return target;
		}

		return osty;
	}
}
