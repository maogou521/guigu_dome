using HarmonyLib;
using System;

namespace MOD_pRRmMh.Patch
{
    /// <summary>
    /// 世界初始化补丁
    /// </summary>
    [HarmonyPatch(typeof(WorldMgr), "Init")]
    internal class Patch_WorldMgr_Init
    {
        /// <summary>
        /// 世界初始化话完成后执行改方法
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix()
        {
            try
            {
                //添加世界运转结束监听
                g.world.run.On(WorldRunOrder.End, new Action(WorldRunEndCall));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static void WorldRunEndCall()
        {
            //获取所有人物，包括玩家
            var unitNpcs = g.world.unit.GetUnits();
            //循环所有人物
            foreach (var unit in unitNpcs)
            {
                //如果是玩家就跳过
                if (unit.data.unitData.unitID == g.world.playerUnit.data.unitData.unitID)
                {
                    continue;
                }

                //获取NPC是否是天骄
                var isHeroes = unit.data.unitData.heart.IsHeroes();
                //获取NPC的后天气运（包括逆天改命气运）
                var addLuck = unit.data.unitData.propertyData.addLuck;

                //如果是天骄
                if (isHeroes)
                {
                    //之前是普通人变成天骄的需要删除《普通气运》
                    //普通气运ID
                    var deleteFeatureId = -109902913;
                    //判断该NPC是否有普通气运，如果有就删除
                    bool existDeleteStrengthenLuckFunc(DataUnit.LuckData ld) => ld.id == deleteFeatureId;
                    if (addLuck.FindIndex((Il2CppSystem.Predicate<DataUnit.LuckData>)existDeleteStrengthenLuckFunc) > -1)
                        unit.CreateAction(new UnitActionLuckDel(deleteFeatureId));

                    //给天骄添加天骄气运
                    //天骄气运ID
                    var featureId = 1414175882;
                    //判断该NPC是否有《天骄气运》，如果没有就添加
                    bool existStrengthenLuckFunc(DataUnit.LuckData ld) => ld.id == featureId;
                    if (addLuck.FindIndex((Il2CppSystem.Predicate<DataUnit.LuckData>)existStrengthenLuckFunc) <= -1)
                        unit.CreateAction(new UnitActionLuckAdd(featureId));
                }
                else
                {
                    //天骄变成普通人需要删除《天骄气运》
                    //天骄气运ID
                    var deleteFeatureId = 1414175882;
                    //判断该NPC是否有天骄气运，如果有就删除
                    bool existDeleteStrengthenLuckFunc(DataUnit.LuckData ld) => ld.id == deleteFeatureId;
                    if (addLuck.FindIndex((Il2CppSystem.Predicate<DataUnit.LuckData>)existDeleteStrengthenLuckFunc) > -1)
                        unit.CreateAction(new UnitActionLuckDel(deleteFeatureId));

                    //添加上普通气运
                    //普通气运ID
                    var featureId = -109902913;
                    //判断该NPC是否有《普通气运》，如果没有就添加
                    bool existStrengthenLuckFunc(DataUnit.LuckData ld) => ld.id == featureId;
                    if (addLuck.FindIndex((Il2CppSystem.Predicate<DataUnit.LuckData>)existStrengthenLuckFunc) <= -1)
                        unit.CreateAction(new UnitActionLuckAdd(featureId));
                }
            }
        }
    }
}
