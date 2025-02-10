using System.Text.Json.Serialization;

namespace Server.Services.LedTheme;

[JsonDerivedType(typeof(NothingModifier), typeDiscriminator: "nothingModifier")]
[JsonDerivedType(typeof(WakeupModifier), typeDiscriminator: "wakeupModifier")]
public class LedThemeModifier
{
    public LedThemeModifier() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeName => GetType().Name.Replace("Modifier", " Modifier");

    public virtual LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input) => null;
}