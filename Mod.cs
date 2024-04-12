using System.Linq;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Debug;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Rendering;

namespace Overpopulated;

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
            log.Info($"Current mod asset at {asset.path}");
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

        //RegisterGizmoPanel();
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
    public void RegisterGizmoPanel()
    {
        /*
        Game.Debug.DebugSystem.AddSystemGizmoField<LandValueDebugSystem>(container, base.World, "Land Value Debug System"); // this is private

        Game.Debug.DebugSystem
        private Dictionary<string, List<DebugUI.Widget>> m_Panels = new Dictionary<string, List<DebugUI.Widget>>();

    UnityEngine.Rendering.DebugManager

         DebugUI.Panel panel = DebugManager.instance.GetPanel("Gizmos", createIfNull: true, groupIndex, overrideIfExists);


    m_Panels[name] = widgets;
        i need to retrieve m_Panels from Game.Debug.DebugSystem
        */
        //Dictionary<string, List<DebugUI.Widget>> __m_Panels = AccessTools.FieldRef<Dictionary<string, List<DebugUI.Widget>>>(

        // Step 1 - get m_Panels from DebugSystem

        // get container ?
        // Step 2 - create widget?

        // get panel
        DebugUI.Panel panel = DebugManager.instance.GetPanel("Gizmos"); // Get existing panel, createIfNull = false
        if (panel == null)
        {
            Mod.log.Warn($"Failed to retrieve the Gizmos panel.");
        }
        else
        {
            Mod.log.Info($"Gizmos panel: {panel.displayName} flags {panel.flags} children {panel.children.Count}");
            //foreach (ObservableList<Widget> list in panel.children)
            //{
            //}
        }

        // Step 
    }

}
