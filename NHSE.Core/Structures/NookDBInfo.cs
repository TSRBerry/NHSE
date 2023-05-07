

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NHSE.Core.Structures
{
    internal class NookDBInfo
    {
        [JsonInclude]
        public PlayerItemCatalog catalog;
        [JsonInclude]
        public Dictionary<string, Dictionary<string, bool>> diy;
        [JsonInclude]
        public Dictionary<string, GSaveDate> museum;
        // TODO: figure out translation stuff, but use internal ids for now
        [JsonInclude]
        public Dictionary<string, int> achievements;
        [JsonInclude]
        public List<string> reactions;
        [JsonInclude]
        public List<string> villagers;

        
        private const string Unknown = "???";

        public NookDBInfo(int playerIndex, PlayerItemCatalog catalog, RecipeBook book, Museum museum, AchievementList achievements, IReactionStore reactionStore, IVillager[] villagers)
        {
            this.catalog = catalog;
            this.diy = CreateDIYDict(book);
            this.museum = CreateMuseumDict(playerIndex, museum);
            this.achievements = NookDBInfo.CreateAchievementDict(achievements);
            this.reactions = NookDBInfo.CreateReactionList(reactionStore);
            this.villagers = NookDBInfo.CreateVillagerList(villagers);
        }

        private static Dictionary<string, Dictionary<string, bool>> CreateDIYDict(RecipeBook book)
        {
            Dictionary<string, Dictionary<string, bool>> diy = new Dictionary<string, Dictionary<string, bool>>();
            for (ushort i = 0; i <= RecipeBook.RecipeCount; i++)
            {
                var name = GetItemName(i, RecipeList.Recipes, GameInfo.Strings.itemlist);
                var known = book.GetIsKnown(i);
                var crafted = book.GetIsMade(i);
                if (known)
                {
                    diy[name] = new Dictionary<string, bool>
                    {
                        { "known", known },
                        { "crafted", crafted }
                    };
                }
            }
            return diy;
        }

        private static string GetItemName(ushort index, IReadOnlyDictionary<ushort, ushort> recipes, string[] itemNames)
        {
            bool exists = recipes.TryGetValue(index, out var value);
            string item = exists ? itemNames[value] : Unknown;
            if (string.IsNullOrEmpty(item))
                item = value.ToString();
            return item;
        }

        public Dictionary<string, GSaveDate> CreateMuseumDict(int playerIndex, Museum museum)
        {
            var dict = new Dictionary<string, GSaveDate>();
            for (int i = 0; i < Museum.EntryCount; i++)
            {
                var item = museum.GetItems()[i];
                if (item.IsNone)
                    continue;
                var date = museum.GetDates()[i];
                var player = museum.GetPlayers()[i];
                var itemName = GameInfo.Strings.GetItemName(item);
                if (player == playerIndex)
                {
                    dict[itemName] = date;
                }
            }
            return dict;
        }

        public static Dictionary<string, int> CreateAchievementDict(AchievementList achievements)
        {
            var dict = new Dictionary<string, int>();
            var str = GameInfo.Strings.InternalNameTranslation;
            for (int i = 0; i < achievements.Counts.Length; i++)
            {
                if (LifeSupportAchievement.List.TryGetValue(i, out var val)) {
                    //var name = val.Name;
                    //if (str.TryGetValue(val.Name, out var translated))
                    //{
                    //    name = translated;
                    //}
                    var milestones = val.GetSatisfiedMilestones(achievements.Counts[i]);
                    if (milestones > 0)
                    {
                        dict[val.Name] = milestones;
                    }
                }
            }
            return dict;
        }

        public static List<string> CreateReactionList(IReactionStore reactionStore)
        {
            var list = new List<string>();
            foreach (Reaction react in reactionStore.ManpuBit)
            {
                if (react != Reaction.None)
                    list.Add(react.ToString());
            }
            return list;
        }

        public static List<string> CreateVillagerList(IVillager[] villagers)
        {
            var list = new List<string>();
            foreach (IVillager villager in villagers)
            {
                if ((VillagerSpecies) villager.Species != VillagerSpecies.non)
                {
                    list.Add(GameInfo.Strings.GetVillager(villager.InternalName));
                }
            }
            return list;
        }
    }
}
