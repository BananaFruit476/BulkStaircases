using BulkStaircases.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace BulkStaircases
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private static readonly string TREASUREFIELDNAME = "netIsTreasureRoom";

        private static readonly string UNDERGROUNDMINESTRING = "UndergroundMine";

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
            int maxLevelsToDescend;
            // normal mine
            if (shaft.mineLevel >= 0 && shaft.mineLevel < MineShaft.bottomOfMineLevel)
            {
                if(shaft.mineLevel + numStairsCanBeUsed > MineShaft.bottomOfMineLevel)
                {
                    maxLevelsToDescend = MineShaft.bottomOfMineLevel - shaft.mineLevel;
                }
                else
                {
                    maxLevelsToDescend = numStairsCanBeUsed;
                }
            }
            // skull cavern
            else if (!Config.SkipLevel100SkullCavern && shaft.mineLevel < SPECIALSKULLCAVERNFLOOR && shaft.mineLevel + numStairsCanBeUsed > SPECIALSKULLCAVERNFLOOR)
            {
                maxLevelsToDescend = SPECIALSKULLCAVERNFLOOR - shaft.mineLevel;
            }
            else
            {
                maxLevelsToDescend = numStairsCanBeUsed;
            }
            int actualLevelsToDescend = 0;
            LocationRequest levelToDescendTo;
            do
            {
                actualLevelsToDescend++;
                levelToDescendTo = this.GetLocationRequestForMineLevel(shaft.mineLevel + actualLevelsToDescend);
            }
            while (SkipLevel(levelToDescendTo) && actualLevelsToDescend < maxLevelsToDescend);
            warpFarmer(levelToDescendTo);
            if (heldItem.Stack > actualLevelsToDescend)
            {
                heldItem.Stack -= actualLevelsToDescend;
            }
            else
            {
                player.removeItemFromInventory(heldItem);
            }
        }

        /// <summary>
        /// True if the level is to be skipped.
        /// False otherwise.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool SkipLevel(LocationRequest request)
        {
            var location = request.Location;
            if (location is MineShaft mine)
            {
                //this.Helper.Reflection.GetField<bool>()
                if (!this.Config.SkipTreasureLevels)
                {
                    IReflectedField<NetBool> treasureField = Helper.Reflection.GetField<NetBool>(mine, TREASUREFIELDNAME);
                    if(treasureField != null)
                    {
                        NetBool val = treasureField.GetValue();
                        bool isTreasure = val.Value;
                        if (isTreasure)
                            return false;
                    }
                }
            }
            return true;
        }
        private void warpFarmer(LocationRequest request)
        {
            Game1.warpFarmer(request, 6, 6, 2);
        }
        
        /// <summary>
        /// Gets the location request for the given mine level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private LocationRequest GetLocationRequestForMineLevel(int level)
        {
            return Game1.getLocationRequest(UNDERGROUNDMINESTRING + level);
        }
    }
}
