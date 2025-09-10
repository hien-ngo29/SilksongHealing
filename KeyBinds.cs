using InControl;

namespace SilksongHealing
{   
    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction quickHealKey;
        
        public KeyBinds()
        {
            quickHealKey = CreatePlayerAction("Quick Heal");
            quickHealKey.AddDefaultBinding(Key.V);
        }
    }

    public class ButtonBinds: PlayerActionSet
    {
        public PlayerAction quickHealButton;

        public ButtonBinds()
        {
            quickHealButton = CreatePlayerAction("Quick Heal Controller");
            quickHealButton.AddDefaultBinding(InputControlType.Action2);
        }
    }
}
