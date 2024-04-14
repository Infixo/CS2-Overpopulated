using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Debug;
using UnityEngine.Rendering;
using Unity.Entities;
using HarmonyLib;

namespace Overpopulated;

[HarmonyPatch]
public class Mod : IMod
{
    public static readonly string harmonyID = "Infixo." + nameof(Overpopulated);

    // mod's instance and asset
    public static Mod instance { get; private set; }
    public static ExecutableAsset modAsset { get; private set; }
    // logging
    public static ILog log = LogManager.GetLogger($"{nameof(Overpopulated)}").SetShowsErrorsInUI(false);

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
        {
            log.Info($"{asset.name} v{asset.version} mod asset at {asset.path}");
            modAsset = asset;
        }

        // Harmony
        var harmony = new Harmony(harmonyID);
        harmony.PatchAll(typeof(Mod).Assembly);
        var patchedMethods = harmony.GetPatchedMethods().ToArray();
        log.Info($"Plugin {harmonyID} made patches! Patched methods: " + patchedMethods.Length);
        foreach (var patchedMethod in patchedMethods)
        {
            log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.DeclaringType.Name}.{patchedMethod.Name}");
        }

        // Systems
        updateSystem.UpdateAt<OverpopulatedBuildingsSystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<OverpopulatedDebugSystem>(SystemUpdatePhase.DebugGizmos);

        // 240414 Restore old systems
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Citizens.HouseholdAndCitizenRemoveSystem>().Enabled = false;
        log.Info("HouseholdAndCitizenRemoveSystem is disabled.");
        updateSystem.UpdateAt<Overpopulated.HouseholdRemoveSystem>(SystemUpdatePhase.Modification2);
        updateSystem.UpdateAt<Overpopulated.CitizenRemoveSystem>(SystemUpdatePhase.Modification4);
    }

    public void OnDispose()
    {
        log.Info(nameof(OnDispose));
        // Harmony
        var harmony = new Harmony(harmonyID);
        harmony.UnpatchAll(harmonyID);
    }

    // This is going to be hacky because virtually all fields and methods needed for that are private :(
    // DebugSystem => RefreshGizmosDebug => AddSystemGizmoField<WaterCullingDebugSystem>(container, base.World, "Water Culling Debug System");
    // DebugUI.Container container = new DebugUI.Container();
    // container is added to the list
    // List<DebugUI.Widget> list = new List<DebugUI.Widget>();
    // Then list is used in AddPanel => 
    [HarmonyPatch(typeof(Game.Debug.DebugSystem), "RefreshGizmosDebug", new Type[] { }, new ArgumentType[] { })]
    [HarmonyPostfix]
    public static void DebugSystem_RegisterGizmoPanel(
        DebugSystem __instance,
        Dictionary<string, List<DebugUI.Widget>> ___m_Panels
        )
    {
        //Mod.log.Info($"DebugSystem_RegisterGizmoPanel");

        // Get panel variable - apparently this is not needed to register a new gizmo, leaving for future reference
        /*
        DebugUI.Panel panel = DebugManager.instance.GetPanel("Gizmos"); // Get existing panel, createIfNull = false
        if (panel == null)
        {
            Mod.log.Warn($"Failed to retrieve the Gizmos panel.");
        }
        else
        {
            Mod.log.Info($"Gizmos panel: {panel.displayName} flags {panel.flags} children {panel.children.Count}");
            foreach (DebugUI.Container item in panel.children)
            {
                Mod.log.Info($"{item.children}");
            }
        }
        */

        /* Debug
        Mod.log.Info($"m_Panels: {___m_Panels.Count}");
        foreach (var item in ___m_Panels)
        {
            Mod.log.Info($"{item.Key}: {item.Value} {item.Value.Count}");
        }
        */

        // Retrieve container that holds all gizmos
        List<DebugUI.Widget> widgets = ___m_Panels["Gizmos"];
        //Mod.log.Info($"Gizmos: {widgets.Count}");
        DebugUI.Container container = widgets.First() as DebugUI.Container;

        /* Debug
        foreach (DebugUI.Container item in widgets)
        {
            Mod.log.Info($"{item}");
            //foreach (var item2 in item.children) Mod.log.Info($"{item2}");
            container = item;
            break;
        }
        */

        // Register a new system as Gizmo; need to perform this call of a private method:
        // __instance.AddSystemGizmoField<OverpopulatedDebugSystem>(container, __instance.World, "Overpopulated");

        // Get the MethodInfo for the method you want to invoke
        MethodInfo methodInfo = typeof(Game.Debug.DebugSystem).GetMethod("AddSystemGizmoField", BindingFlags.Instance | BindingFlags.NonPublic);
        //Mod.log.Info($"{methodInfo.Name}: {methodInfo}");

        // The method is generic, so make it specific
        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(typeof(OverpopulatedDebugSystem));
        //Mod.log.Info($"{genericMethodInfo.Name}: {genericMethodInfo}");

        // Invoke the method with arguments
        object result = genericMethodInfo.Invoke(__instance, new object[] { container, __instance.World, "Overpopulated" });
        //Mod.log.Info($"Invoked: {result}");
    }
}
