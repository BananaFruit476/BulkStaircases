using StardewModdingAPI.Utilities;

namespace BulkStaircases.Framework
{
    internal class ModConfig
    {
        private int _numberOfStaircasesToLeaveInStack = 0;
        public int NumberOfStaircasesToLeaveInStack
        {
            get
            {
                return this._numberOfStaircasesToLeaveInStack;
            }
            set
            {
                if (value < 0)
                    this._numberOfStaircasesToLeaveInStack = 0;
                else
                    this._numberOfStaircasesToLeaveInStack = value;
            }
        }

        public bool SkipLevel100SkullCavernLevel { get; set; } = false;
        
        public KeybindList ToggleKey { get; set; } = KeybindList.Parse("LeftShift + F2");
    }
}
