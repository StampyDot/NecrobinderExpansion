using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using NecrobinderExpansion.Cards;// ← 这里如果报红就删掉，其他爆红的别动。
using NecrobinderExpansion.Powers;// ← 这里如果报红就删掉，其他爆红的别动。

namespace NecrobinderExpansion   // ← 这里改成你的模组ID（必须和你的.json文件中的"id"一致！）
{
    // 通用 ModInitializer
    [ModInitializer(nameof(Initialize))]
    public static class ModInitializer
    {
        public static void Initialize()
        {
            {
                // 1. 初始化 Harmony（强烈推荐这样写，避免与其他模组冲突。）
                var harmony = new Harmony("NecrobinderExpansion.StampyDot");   // 格式：模组ID.作者名
                harmony.PatchAll();

                // 2. 在这里添加你的注册逻辑（卡牌、遗物、药水等）
                //ModHelper.AddModelToPool(typeof( 卡牌池 ), typeof( 卡牌名字 ));
                //ModHelper.AddModelToPool(typeof( 遗物池 ), typeof( 遗物名字 ));
                //ModHelper.AddModelToPool(typeof( 药水池 ), typeof( 药水名字 ));
                
                // ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(BorrowedTimeOld));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Splice));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Thrive));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(AbsorbLife));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Backlash));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Funeral));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Provoke));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Remembrance));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(GrowWild));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(DesperateGuard));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(JoinHands));
            
                JoinHands.EnsureSubscribed();

                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Deathmatch));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Gnaw));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Tremor));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(GetReady));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Necrophagy));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Overdraw));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(FlameOfSouls));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(FriendsFromBeyond));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Condense));
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(MuscleMemory));
                
                ModHelper.AddModelToPool(typeof(NecrobinderCardPool), typeof(Hovering));
                
            }
        }
    }
}
