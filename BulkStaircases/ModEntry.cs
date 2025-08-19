using BulkStaircases.Framework;
using GenericModConfigMenu;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BulkStaircases
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /// <summary>
        /// Field name of treasure room field, see MineShaft.cs
        /// </summary>
        private static readonly string TREASUREFIELDNAME = "netIsTreasureRoom";

        /// <summary>
        /// Property name of quarry dungeon state, see MineShaft.cs
        /// </summary>
        private static readonly string QUARRYPROPERTYNAME = "isQuarryArea";

        /// <summary>
        /// Property name of monster area state, see MineShaft.cs
        /// </summary>
        private static readonly string MONSTERAREAPROPERTYNAME = "isMonsterArea";

        /// <summary>
        /// Property name of dinosaur area state, see MineShaft.cs
        /// </summary>
        private static readonly string DINOSAURAREAPROPERTYNAME = "isDinoArea";

        /// <summary>
        /// String for mines, see constructor of MineShaft.cs
        /// </summary>
        private static readonly string UNDERGROUNDMINESTRING = "UndergroundMine";

        private ModConfig Config;

        private static readonly string STAIRCASEID = "(BC)71";

        private static HashSet<string> MonsterFilterNames;

        private static Dictionary<string, int> MonsterCountDict;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            MonsterFilterNames = new HashSet<string>(this.Config.MonsterFilters.Keys);
            this.SetCountConfigDict();
            IModEvents events = helper.Events;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        /*********
        ** Private methods
        *********/
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Number of staircases to leave in stack",
                tooltip: () => "How many number of staircases are to be left in stack and not used up.",
                getValue: () => this.Config.NumberOfStaircasesToLeaveInStack,
                setValue: value => this.Config.NumberOfStaircasesToLeaveInStack = value,
                min: 0,
                formatValue: value => value > 0 ? value.ToString() : "None"
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Max levels to skip per use",
                tooltip: () => "The maximum number of levels that are skipped per use.",
                getValue: () => this.Config.MaxLevelsToSkipPerUse,
                setValue: value => this.Config.MaxLevelsToSkipPerUse = value,
                min: 0,
                formatValue: value => value > 0 ? value.ToString() : "None"
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Dinosaur Levels",
                tooltip: () => "Whether to skip dinosaur levels.",
                getValue: () => this.Config.SkipDinosaurLevels,
                setValue: value => this.Config.SkipDinosaurLevels = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Treasure Levels",
                tooltip: () => "Whether to skip treasure levels.",
                getValue: () => this.Config.SkipTreasureLevels,
                setValue: value => this.Config.SkipTreasureLevels = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Quarry Dungeon Levels",
                tooltip: () => "Whether to Quarry Dungeon levels.",
                getValue: () => this.Config.SkipQuarryDungeonLevels,
                setValue: value => this.Config.SkipQuarryDungeonLevels = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Slime Levels",
                tooltip: () => "Whether to skip slime infested levels.",
                getValue: () => this.Config.SkipSlimeLevels,
                setValue: value => this.Config.SkipSlimeLevels = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Monster Levels",
                tooltip: () => "Whether to skip monster infested levels.",
                getValue: () => this.Config.SkipMonsterLevels,
                setValue: value => this.Config.SkipMonsterLevels = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Skip Mushroom Levels",
                tooltip: () => "Whether to skip mushroom levels.",
                getValue: () => this.Config.SkipMushroomLevels,
                setValue: value => this.Config.SkipMushroomLevels = value
            );
            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Keybind",
                tooltip: () => "Keybind",
                getValue: () => this.Config.ToggleKey,
                setValue: value => this.Config.ToggleKey = value
            );
        }


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
            var staircases = player.Items.GetById(ModEntry.STAIRCASEID).ToList();
            if (staircases.Count == 0)
            {
                Game1.addHUDMessage(new HUDMessage($"No staircases in inventory", 3));
                return;
            }
            int totalStaircases = staircases.Sum(item => item.Stack);
            if (shaft.mineLevel == MineShaft.bottomOfMineLevel)
            {
                Game1.addHUDMessage(new HUDMessage($"You're already at the bottom of the mines", 3));
                return;
            }
            if(shaft.mineLevel == MineShaft.quarryMineShaft)
            {
                Game1.addHUDMessage(new HUDMessage($"Can't use staircases here", 3));
                return;
            }
            var maxLevelsToSkipPerUse = Config.MaxLevelsToSkipPerUse > 0 ? Config.MaxLevelsToSkipPerUse : int.MaxValue;
            var numStairsCanBeUsed = Math.Min(totalStaircases - Config.NumberOfStaircasesToLeaveInStack, maxLevelsToSkipPerUse);

            if (this.Config.DoNotSkipMineLevels.Count >= 0 && shaft.mineLevel <= MineShaft.bottomOfMineLevel && this.Config.DoNotSkipMineLevels.Any(n => n > shaft.mineLevel))
            {
                int? closestLevelDeeperThanCurrent = this.Config.DoNotSkipMineLevels.Where(n => n > shaft.mineLevel).Min();
                if (closestLevelDeeperThanCurrent.HasValue)
                {
                    int difference = closestLevelDeeperThanCurrent.Value - shaft.mineLevel;
                    numStairsCanBeUsed = Math.Min(numStairsCanBeUsed, difference);
                }
            }
            var realSkullCavernLevel = shaft.mineLevel - MineShaft.bottomOfMineLevel;
            if (this.Config.DoNotSkipSkullCavernLevels.Count > 0 && shaft.mineLevel > MineShaft.bottomOfMineLevel && this.Config.DoNotSkipSkullCavernLevels.Any(n => n > realSkullCavernLevel))
            {
                int? closestLevelDeeperThanCurrent = this.Config.DoNotSkipSkullCavernLevels.Where(n => n > realSkullCavernLevel).Min();
                if (closestLevelDeeperThanCurrent.HasValue)
                {
                    int difference = closestLevelDeeperThanCurrent.Value - realSkullCavernLevel;
                    numStairsCanBeUsed = Math.Min(numStairsCanBeUsed, difference);
                }
            }

            if (numStairsCanBeUsed <= 0)
            {
                Game1.addHUDMessage(new HUDMessage($"Only {totalStaircases} staircases left", 3));
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
            else
            {
                maxLevelsToDescend = numStairsCanBeUsed;
            }
            int actualLevelsToDescend = 0;
            LocationRequest levelToDescendTo;
            if (!this.NeedToCheckIndividualLevel())
            {
                actualLevelsToDescend = maxLevelsToDescend;
                levelToDescendTo = this.GetLocationRequestForMineLevel(shaft.mineLevel + actualLevelsToDescend);
            }
            // only actually calculate level if need be
            else
            {
                do
                {
                    actualLevelsToDescend++;
                    levelToDescendTo = this.GetLocationRequestForMineLevel(shaft.mineLevel + actualLevelsToDescend);
                }
                while (SkipLevel(levelToDescendTo) && actualLevelsToDescend < maxLevelsToDescend);
            }
            warpFarmer(levelToDescendTo);
            MineShaft.numberOfCraftedStairsUsedThisRun += actualLevelsToDescend;
            var stairs_to_remove = actualLevelsToDescend;
            foreach (var staircase in staircases)
            {
                if (staircase.Stack > stairs_to_remove)
                {
                    staircase.Stack -= stairs_to_remove;
                    break;
                }
                else
                {
                    stairs_to_remove -= staircase.Stack;
                    player.removeItemFromInventory(staircase);
                }
            }
        }

        /// <summary>
        /// Checks if individual levels need to be checked
        /// </summary>
        /// <returns></returns>
        private bool NeedToCheckIndividualLevel()
        {
            return !this.Config.SkipTreasureLevels
                || !this.Config.SkipSlimeLevels
                || !this.Config.SkipQuarryDungeonLevels
                || !this.Config.SkipMonsterLevels
                || !this.Config.SkipDinosaurLevels
                || !this.Config.SkipMushroomLevels
                || ModEntry.MonsterFilterNames.Count > 0;
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
                if(this.Config.DoNotSkipMineLevels.Count > 0)
                {
                    if (this.Config.DoNotSkipMineLevels.Contains(mine.mineLevel))
                        return false;
                }
                if(this.Config.DoNotSkipSkullCavernLevels.Count > 0)
                {
                    var realSkullCavernLevel = mine.mineLevel - MineShaft.bottomOfMineLevel;
                    if (this.Config.DoNotSkipSkullCavernLevels.Contains(realSkullCavernLevel))
                        return false;
                }
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
                if (!this.Config.SkipSlimeLevels)
                {
                    if (mine.isLevelSlimeArea())
                        return false;
                }
                if (!this.Config.SkipQuarryDungeonLevels)
                {
                    if (IsBoolPropertyTrue(QUARRYPROPERTYNAME, mine))
                        return false;
                }
                if (!this.Config.SkipMonsterLevels)
                {
                    if (IsBoolPropertyTrue(MONSTERAREAPROPERTYNAME, mine))
                        return false;
                }
                if (!this.Config.SkipDinosaurLevels)
                {
                    if (IsBoolPropertyTrue(DINOSAURAREAPROPERTYNAME, mine))
                        return false;
                }
                if (this.LevelContainsInterestedMonsters(mine))
                {
                    return false;
                }
                if (!this.Config.SkipMushroomLevels)
                {
                    if (MineShaft.mushroomLevelsGeneratedToday.Contains(mine.mineLevel))
                        return false;
                }
                if (this.ContainsInterestingNodes(mine))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ContainsInterestingNodes(MineShaft mine)
        {

            var obj_to_num_dict = new Dictionary<string, int>();
            foreach(var obj in mine.objects.Values)
            {
                if (!obj_to_num_dict.ContainsKey(obj.QualifiedItemId))
                {
                    obj_to_num_dict[obj.QualifiedItemId] = 0;
                }
                obj_to_num_dict[obj.QualifiedItemId] += 1;
            }
            if (this.Config.NodeFilters.Sum(kv => kv.Value) > 0)
            {
                foreach (var node_to_num in this.Config.NodeFilters)
                {
                    var node = node_to_num.Key;
                    var num = node_to_num.Value;
                    string node_id = string.Empty;
                    switch (node)
                    {
                        case "Iridium Node":
                            node_id = "(O)765";
                            break;
                        case "Radioactive Node":
                            node_id = "(O)95";
                            break;
                        default:
                            throw new NotImplementedException("Don't know that!");
                    }
                    int num_in_mine = 0;
                    if (obj_to_num_dict.ContainsKey(node_id))
                    {
                        num_in_mine = obj_to_num_dict[node_id];
                    }
                    if (num > 0 && num_in_mine >= num)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool LevelContainsInterestedMonsters(MineShaft mine)
        {
            bool containsMonster = false;
            if (ModEntry.MonsterFilterNames.Count > 0)
            {
                foreach (var npc in mine.characters)
                {
                    if (npc is Monster monster)
                    {
                        var monsterName = monster.Name;
                        if (ModEntry.MonsterFilterNames.Contains(monsterName))
                        {
                            ModEntry.MonsterCountDict[monsterName]++;
                            if (ModEntry.MonsterCountDict[monsterName] >= this.Config.MonsterFilters[monsterName])
                            {
                                containsMonster = true;
                                break;
                            }
                        }
                    }
                }
                ModEntry.ResetCountConfigDict();
            }
            return containsMonster;
        }

        private void SetCountConfigDict()
        {
            var emptyConfigDict = new Dictionary<string, int>();
            foreach(var pair in this.Config.MonsterFilters)
            {
                emptyConfigDict.Add(pair.Key, 0);
            }
            MonsterCountDict = emptyConfigDict;
        }

        private static void ResetCountConfigDict()
        {
            foreach (var pair in ModEntry.MonsterCountDict)
            {
                ModEntry.MonsterCountDict[pair.Key] = 0;
            }
        }

        private bool IsBoolPropertyTrue(string propertyName, object tobeCheckedForProperty)
        {
            IReflectedProperty<bool> dinosaurAreaProperty = Helper.Reflection.GetProperty<bool>(tobeCheckedForProperty, propertyName);
            if (dinosaurAreaProperty != null)
            {
                return dinosaurAreaProperty.GetValue();
            }
            return false;
        }
        private void warpFarmer(LocationRequest request)
        {
            // constants taken from Game1.enterMine
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
