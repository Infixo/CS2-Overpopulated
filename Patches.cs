using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using Game.Debug;
using HarmonyLib;
using Unity.Entities;
using UnityEngine.Rendering;
using Unity.Mathematics;

namespace Overpopulated;

[HarmonyPatch]
internal class Patches
{
    [HarmonyPatch(typeof(Game.Debug.DebugSystem), "RegisterDebug")]
    [HarmonyPostfix]
    public static void RegisterDebug()
    {
        Mod.log.Info("RegisterDebug");
    }

    /*
    [HarmonyPatch(typeof(Game.Economy.EconomyUtils), "AddResources",
        new Type[] { typeof(Resource), typeof(int), typeof(DynamicBuffer<Resources>) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
    */



private DebugUI.Container AddSystemGizmoField<T>(DebugUI.Container container, World world, string name) where T : ComponentSystemBase
{
    T system = world.GetOrCreateSystemManaged<T>();
    DebugUI.EnumField item = new DebugUI.EnumField
    {
        displayName = name,
        getter = () => system.Enabled ? 1 : 0,
        setter = delegate (int value)
        {
            system.Enabled = ((value != 0) ? true : false);
        },
        autoEnum = typeof(ToggleEnum),
        onValueChanged = RefreshGizmosDebug,
        getIndex = () => system.Enabled ? 1 : 0,
        setIndex = delegate
        {
        }
    };
    container.children.Add(item);
    BaseDebugSystem baseDebugSystem = system as BaseDebugSystem;
    if (system.Enabled)
    {
        if (baseDebugSystem != null)
        {
            List<BaseDebugSystem.Option> options = baseDebugSystem.options;
            if (options.Count != 0)
            {
                DebugUI.Container container2 = new DebugUI.Container();
                for (int i = 0; i < options.Count; i++)
                {
                    BaseDebugSystem.Option option = options[i];
                    DebugUI.EnumField item2 = new DebugUI.EnumField
                    {
                        displayName = option.name,
                        getter = () => option.enabled ? 1 : 0,
                        setter = delegate (int value)
                        {
                            option.enabled = ((value != 0) ? true : false);
                        },
                        autoEnum = typeof(ToggleEnum),
                        getIndex = () => option.enabled ? 1 : 0,
                        setIndex = delegate
                        {
                        }
                    };
                    container2.children.Add(item2);
                }
                container.children.Add(container2);
            }
            baseDebugSystem.OnEnabled(container);
        }
    }
    else
    {
        baseDebugSystem?.OnDisabled(container);
    }
    return container;
}



[HarmonyPatch(typeof(Game.Debug.DebugSystem), "RefreshGizmosDebug", new Type[] {}, new ArgumentType[] {})]
    [HarmonyPrefix]
    public static bool RefreshGizmosDebug()
    {
        Mod.log.Info("RefreshGizmosDebug");



            List<DebugUI.Widget> list = new List<DebugUI.Widget>();
            DebugUI.Container container = new DebugUI.Container();
            AddSystemGizmoField<ObjectDebugSystem>(container, base.World, "Object Debug System");
            AddSystemGizmoField<NetDebugSystem>(container, base.World, "Net Debug System");
            AddSystemGizmoField<LaneDebugSystem>(container, base.World, "Lane Debug System");
            AddSystemGizmoField<LightDebugSystem>(container, base.World, "Light Debug System");
            AddSystemGizmoField<WaterCullingDebugSystem>(container, base.World, "Water Culling Debug System");
            AddSystemGizmoField<ZoneDebugSystem>(container, base.World, "Zone Debug System");
            AddSystemGizmoField<AreaDebugSystem>(container, base.World, "Area Debug System");
            AddSystemGizmoField<RouteDebugSystem>(container, base.World, "Route Debug System");
            AddSystemGizmoField<NavigationDebugSystem>(container, base.World, "Navigation Debug System");
            AddSystemGizmoField<AudioGroupingDebugSystem>(container, base.World, "Audio Grouping Debug System");
            AddSystemGizmoField<AvailabilityDebugSystem>(container, base.World, "Availability Debug System");
            AddSystemGizmoField<DensityDebugSystem>(container, base.World, "Density Debug System");
            AddSystemGizmoField<CoverageDebugSystem>(container, base.World, "Coverage Debug System");
            AddSystemGizmoField<PathDebugSystem>(container, base.World, "Path Debug System");
            AddSystemGizmoField<PathfindDebugSystem>(container, base.World, "Pathfinding Debug System");
            AddSystemGizmoField<SearchTreeDebugSystem>(container, base.World, "Search Tree Debug System");
            AddSystemGizmoField<TerrainAttractivenessDebugSystem>(container, base.World, "Terrain Attractiveness Debug System");
            AddSystemGizmoField<LandValueDebugSystem>(container, base.World, "Land Value Debug System");
            AddSystemGizmoField<EconomyDebugSystem>(container, base.World, "Economy Debug System");
            AddSystemGizmoField<PollutionDebugSystem>(container, base.World, "Pollution Debug System");
            AddSystemGizmoField<GroundWaterDebugSystem>(container, base.World, "Ground Water Debug System");
            AddSystemGizmoField<SoilWaterDebugSystem>(container, base.World, "Soil Water Debug System");
            AddSystemGizmoField<NaturalResourceDebugSystem>(container, base.World, "Natural Resource Debug System");
            AddSystemGizmoField<GarbageDebugSystem>(container, base.World, "Garbage Debug System");
            AddSystemGizmoField<TerrainDebugSystem>(container, base.World, "Terrain Debug System");
            AddSystemGizmoField<WaterDebugSystem>(container, base.World, "Water Debug System");
            AddSystemGizmoField<WindDebugSystem>(container, base.World, "Wind Debug System");
            AddSystemGizmoField<EventDebugSystem>(container, base.World, "Event Debug System");
            AddSystemGizmoField<TradeCostDebugSystem>(container, base.World, "Tradecost Debug System");
            DebugUI.Container container2 = AddSystemGizmoField<BuildableAreaDebugSystem>(container, base.World, "Buildable Area Debug System");
            BuildableAreaDebugSystem buildableAreaSystem = base.World.GetOrCreateSystemManaged<BuildableAreaDebugSystem>();
            if (buildableAreaSystem.Enabled)
            {
                container2.children.Add(new DebugUI.Container
                {
                    children = { (DebugUI.Widget)new DebugUI.Value
            {
                displayName = "Buildable Area",
                getter = () => (int)math.round(100f * buildableAreaSystem.buildableArea)
            } }
                });
            }
            list.Add(container);
            AddPanel("Gizmos", list, -2);



        return false; // no original
    }
}
