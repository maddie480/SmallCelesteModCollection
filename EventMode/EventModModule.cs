using Celeste;
using Celeste.Mod;
using System;

public class EventModModule : EverestModule {
    public override Type SettingsType => null;

    public override void Load() {
        Celeste.Celeste.PlayMode = Celeste.Celeste.PlayModes.Event;
    }

    public override void Unload() {}
}
