using Monocle;

namespace Celeste.Mod.ProBananaSkin
{
    public class ProBananaSkinSettings : EverestModuleSettings
    {
        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;

                if (Engine.Scene is Level level)
                {
                    // we're in a map: reset sprite the same way the Other Self toggle does.
                    // the hook on PlayerSprite will decide if we should use the Pro Banana skin or not.
                    Player player = level.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        PlayerSpriteMode mode = (SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                        if (player.Active)
                        {
                            player.ResetSpriteNextFrame(mode);
                        }
                        else
                        {
                            player.ResetSprite(mode);
                        }
                    }
                }
            }
        }
    }
}
