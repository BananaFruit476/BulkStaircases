using BulkStaircases.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace BulkStaircases
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ModConfig Config;

        private static readonly string STAIRCASENAME = "Staircase";

        private static readonly int SPECIALSKULLCAVERNFLOOR = 220;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ToggleKey.JustPressed())
                return;
            if (!Context.CanPlayerMove)
                return;
            GameLocation location = Game1.currentLocation;
            if (location is not MineShaft shaft)
                return;
            Farmer player = Game1.player;
            if (player == null)
                return;
            Item heldItem = player.CurrentItem;
            if (heldItem == null)
                return;
            if (heldItem.Name != ModEntry.STAIRCASENAME)
                return;
            if(shaft.mineLevel == MineShaft.bottomOfMineLevel)
            {
                Game1.addHUDMessage(new HUDMessage($"You're already at the bottom of the mines", 3));
                return;
            }
            if(shaft.mineLevel == MineShaft.quarryMineShaft)
            {
                Game1.addHUDMessage(new HUDMessage($"Can't use staircases here", 3));
                return;
            }
            var numStairsCanBeUsed = heldItem.Stack - Config.NumberOfStaircasesToLeaveInStack;
            if (numStairsCanBeUsed <= 0)
            {
                Game1.addHUDMessage(new HUDMessage($"Only {heldItem.Stack} staircases left", 3));
                return;
            }
            int levelsToDescend;
            // normal mine
            if (shaft.mineLevel >= 0 && shaft.mineLevel < MineShaft.bottomOfMineLevel)
            {
                if(shaft.mineLevel + numStairsCanBeUsed > MineShaft.bottomOfMineLevel)
                {
                    levelsToDescend = MineShaft.bottomOfMineLevel - shaft.mineLevel;
                }
                else
                {
                    levelsToDescend = numStairsCanBeUsed;
                }
            }
            // skull cavern
            else if (!Config.SkipLevel100SkullCavernLevel && shaft.mineLevel < SPECIALSKULLCAVERNFLOOR && shaft.mineLevel + numStairsCanBeUsed > SPECIALSKULLCAVERNFLOOR)
            {
                levelsToDescend = SPECIALSKULLCAVERNFLOOR - shaft.mineLevel;
            }
            else
            {
                levelsToDescend = numStairsCanBeUsed;
            }
            Game1.enterMine(shaft.mineLevel + levelsToDescend);
            MineShaft.numberOfCraftedStairsUsedThisRun += levelsToDescend;
            if(heldItem.Stack > levelsToDescend)
            {
                heldItem.Stack = heldItem.Stack - levelsToDescend;
            }
            else
            {
                player.removeItemFromInventory(heldItem);
            }
        }
    }
}
