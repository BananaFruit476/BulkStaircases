﻿using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace BulkStaircases.Framework
{
    internal class ModConfig
    {
        private int _numberOfStaircasesToLeaveInStack = 0;
        /// <summary>Number of staircases left in the held stack of staircases.</summary>
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
        /// <summary>How many levels to skip maximally for each use of this feature</summary>
        public int MaxLevelsToSkipPerUse { get; set; } = 0;

        /// <summary> which skull cavern levels not to skip. Default 100, 200 and 300</summary>
        public List<int> DoNotSkipSkullCavernLevels { get; set; } = new List<int> { 100, 200, 300 };

        /// <summary> which mine levels not to skip. Default empty</summary>
        public List<int> DoNotSkipMineLevels { get; set; } = new List<int> {};

        /// <summary>Whether to skip prehistoric floors.</summary>
        public bool SkipDinosaurLevels { get; set; } = false;

        /// <summary>Whether to skip levels with a treasure.</summary>
        public bool SkipTreasureLevels { get; set; } = false;

        /// <summary>Whether to skip quarry dungeon levels that may appear after having been to the quarry mine.</summary>
        public bool SkipQuarryDungeonLevels { get; set; } = false;

        /// <summary>Whether to skip slime infested levels.</summary>
        public bool SkipSlimeLevels { get; set; } = false;

        /// <summary>Whether to skip monster infested levels.</summary>
        public bool SkipMonsterLevels { get; set; } = false;

        /// <summary>Whether to skip mushroom levels.</summary>
        public bool SkipMushroomLevels { get; set; } = false;

        /// <summaryDon't skip level with the monsters given here if there are at least the given number of them.</summary>
        public Dictionary<string, int> MonsterFilters { get; set; } = new ();

        /// <summaryDon't skip level with the monsters given here if there are at least the given number of them.</summary>
        public Dictionary<string, int> NodeFilters { get; set; } = new Dictionary<string, int>
        {
            {"Iridium Node", 0},
            {"Radioactive Node", 0}
        };

        public KeybindList ToggleKey { get; set; } = KeybindList.Parse("LeftShift + C");
    }
}
