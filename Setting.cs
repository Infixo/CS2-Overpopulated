using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace Overpopulated;

[FileLocation(nameof(Overpopulated))]
[SettingsUIGroupOrder(kOptionsGroup)]
[SettingsUIShowGroupName(kOptionsGroup)]
public class Setting : ModSetting
{
    public const string kSection = "Main";

    public const string kOptionsGroup = "Options";

    public Setting(IMod mod) : base(mod)
    {
        SetDefaults();
    }

    /// <summary>
    /// Gets or sets a value indicating whether: Used to force saving of Modsettings if settings would result in empty Json.
    /// </summary>
    [SettingsUIHidden]
    public bool _Hidden { get; set; }

    [SettingsUISection(kSection, kOptionsGroup)]
    public bool FeatureGizmo { get; set; }

    [SettingsUISection(kSection, kOptionsGroup)]
    public bool FeatureDumpToLog { get; set; }

    [SettingsUISection(kSection, kOptionsGroup)]
    public bool FeatureLoadingFix { get; set; }

    [SettingsUISection(kSection, kOptionsGroup)]
    public bool FeatureRenterFix { get; set; }

    [SettingsUISection(kSection, kOptionsGroup)]
    public bool FeatureSeparatedSystems { get; set; }

    public override void SetDefaults()
    {
        _Hidden = true;
        FeatureGizmo = true;
        FeatureDumpToLog = true;
        FeatureLoadingFix = true;
        FeatureRenterFix = true;
        FeatureSeparatedSystems = true;
    }

    public override void Apply()
    {
        base.Apply();
    }
}

public class LocaleEN : IDictionarySource
{
    private readonly Setting m_Setting;
    public LocaleEN(Setting setting)
    {
        m_Setting = setting;
    }

    public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
    {
        return new Dictionary<string, string>
        {
            { m_Setting.GetSettingsLocaleID(), $"Overpopulated Gizmo & Fix {Mod.modAsset.version}" },
            { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

            { m_Setting.GetOptionGroupLocaleID(Setting.kOptionsGroup), "Options" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FeatureGizmo)), "Enable overpopulated gizmo" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.FeatureGizmo)), "Enables gizmo that shows buildings with more households than their capacity." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FeatureDumpToLog)), "Enable dump to log" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.FeatureDumpToLog)), "Upon savefile loading, dumps to the log a list of buildings that have more households than their capacity." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FeatureLoadingFix)), "Enable loading fix" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.FeatureLoadingFix)), "Patches the savefile loading process so it will not create buildings with more households or companies than their capacity." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FeatureRenterFix)), "Enable renter fix" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.FeatureRenterFix)), "Patches the household removal system so it will not create property renters detached from properties." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FeatureSeparatedSystems)), "Enable separated removal systems" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.FeatureSeparatedSystems)), "Separates household and citizen removal processes. This prevents a rare race conditon leading to more frequent renter issues. Valid only with the Renter fix enabled." },
        };
    }

    public void Unload()
    {
    }
}
