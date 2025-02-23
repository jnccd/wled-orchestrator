using System.Text.Json.Serialization;

namespace Server.Services.LedTheme;

[JsonDerivedType(typeof(RotateColorsModifier), typeDiscriminator: "rotateColorsModifier")]
[JsonDerivedType(typeof(WakeupModifier), typeDiscriminator: "wakeupModifier")]
public class LedThemeModifier
{
    public LedThemeModifier() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeName => GetType().Name.Replace("Modifier", "");
    public bool Enabled { get; set; } = true;

    public LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input) => Enabled ? Modify(state, input) : state;
    internal virtual LedGroupState? Modify(LedGroupState? state, LedThemeInput input) => null;
}