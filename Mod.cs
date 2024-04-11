using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace Overpopulated;

public class Mod : IMod
{
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

        // Systems
        updateSystem.UpdateAt<OverpopulatedBuildingsSystem>(SystemUpdatePhase.GameSimulation);
    }

    public void OnDispose()
    {
        log.Info(nameof(OnDispose));
    }
}
