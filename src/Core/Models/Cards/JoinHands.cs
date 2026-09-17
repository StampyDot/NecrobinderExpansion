using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using NecrobinderExpansion.Powers;

namespace NecrobinderExpansion.Cards;

public sealed class JoinHands : CardModel
{
	private const string PendingSummonsFilePath = "user://NecrobinderExpansion/join_hands_pending.txt";
	private static readonly Dictionary<string, decimal> PendingSummons = new();
	private static readonly HashSet<string> TriggeredSummons = new();
	private static readonly AbstractModel HookListener = new JoinHandsHookListener();
	private static bool _subscribed;
	private static RunState? _activeRunStateForHooks;
	private static bool _pendingSummonsLoaded;

	private sealed class JoinHandsHookListener : AbstractModel
	{
		public override bool ShouldReceiveCombatHooks => false;

		public override async Task BeforeCombatStartLate()
		{
			RunState? runState = GetActiveRunState();
			if (runState == null)
			{
				return;
			}

			foreach (Player player in runState.Players)
			{
				LoadPendingSummons();
				string key = GetPendingSummonKey(runState, player.NetId);
				if (!PendingSummons.TryGetValue(key, out decimal summonAmount) || summonAmount <= 0m)
				{
					continue;
				}

				TriggeredSummons.Add(key);
				await JoinHandsPower.FlashForCombatStart(player);
				await OstyCmd.Summon(new ThrowingPlayerChoiceContext(), player, summonAmount, this);
			}
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			RunState? runState = GetActiveRunState();
			if (runState == null)
			{
				return Task.CompletedTask;
			}

			LoadPendingSummons();
			foreach (string key in new List<string>(TriggeredSummons))
			{
				if (!key.StartsWith(GetRunKeyPrefix(runState), System.StringComparison.Ordinal))
				{
					continue;
				}

				PendingSummons.Remove(key);
			}

			TriggeredSummons.RemoveWhere(key => key.StartsWith(GetRunKeyPrefix(runState), System.StringComparison.Ordinal));
			SavePendingSummons();
			return Task.CompletedTask;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonStatic)
	};

	public JoinHands()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	public static void EnsureSubscribed()
	{
		if (_subscribed)
		{
			return;
		}

		ModHelper.SubscribeForRunStateHooks("NecrobinderExpansion_JoinHands", GetRunHookListeners);
		_subscribed = true;
	}

	public static void QueueSummon(Player player, decimal amount)
	{
		if (player.RunState is not RunState runState)
		{
			return;
		}

		LoadPendingSummons();
		string key = GetPendingSummonKey(runState, player.NetId);
		PendingSummons[key] = PendingSummons.TryGetValue(key, out decimal current)
			? decimal.Max(current, amount)
			: amount;
		TriggeredSummons.Remove(key);
		SavePendingSummons();
	}

	private static IEnumerable<AbstractModel> GetRunHookListeners(RunState runState)
	{
		_activeRunStateForHooks = runState;
		yield return HookListener;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<JoinHandsPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
	}

	private static string GetPendingSummonKey(RunState runState, ulong playerNetId)
	{
		return $"{GetRunKeyPrefix(runState)}{playerNetId}";
	}

	private static RunState? GetActiveRunState()
	{
		return _activeRunStateForHooks;
	}

	private static string GetRunKeyPrefix(RunState runState)
	{
		return $"{runState.Rng.StringSeed}|";
	}

	private static void LoadPendingSummons()
	{
		if (_pendingSummonsLoaded)
		{
			return;
		}

		_pendingSummonsLoaded = true;
		string path = ProjectSettings.GlobalizePath(PendingSummonsFilePath);
		if (!File.Exists(path))
		{
			return;
		}

		string[] lines;
		try
		{
			lines = File.ReadAllLines(path);
		}
		catch (IOException)
		{
			return;
		}
		catch (System.UnauthorizedAccessException)
		{
			return;
		}

		foreach (string line in lines)
		{
			string[] parts = line.Split('=', 2);
			if (parts.Length != 2 || !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
			{
				continue;
			}

			PendingSummons[parts[0]] = amount;
		}
	}

	private static void SavePendingSummons()
	{
		string path = ProjectSettings.GlobalizePath(PendingSummonsFilePath);
		List<string> lines = new();
		foreach (KeyValuePair<string, decimal> entry in PendingSummons)
		{
			if (entry.Value > 0m)
			{
				lines.Add($"{entry.Key}={entry.Value.ToString(CultureInfo.InvariantCulture)}");
			}
		}

		try
		{
			string? directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllLines(path, lines);
		}
		catch (IOException)
		{
		}
		catch (System.UnauthorizedAccessException)
		{
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
    }
}
