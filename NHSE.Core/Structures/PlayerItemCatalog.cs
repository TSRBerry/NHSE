using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NHSE.Core.Structures
{
    internal class PlayerItemCatalog
    {
        private readonly Player Player;
        
        private readonly Dictionary<string, Dictionary<int, string>> receivedItems = new Dictionary<string, Dictionary<int, string>>();

        private readonly Dictionary<string, Dictionary<int, string>> availableItems = new Dictionary<string, Dictionary<int, string>>();

        [JsonInclude]
        public IReadOnlyDictionary<string, Dictionary<int, string>> ReceivedItems
        {
            get => receivedItems;
            private set { }
        }

        // protected: Hack to not include in json
        protected IReadOnlyDictionary<string, Dictionary<int, string>> AvailableItems
        {
            get => availableItems;
            private set { }
        }

        public PlayerItemCatalog(Player player)
        {
            Player = player;
            
            var items = GameInfo.Strings.itemlistdisplay;
            FillCollected(items);
            FillVariants(items);
        }

        private void FillCollected(IReadOnlyList<string> items)
        {
            var ofs = Player.Personal.Offsets.ItemCollectBit;
            var data = Player.Personal.Data;
            for (int i = 0; i < items.Count; i++)
            {
                var flag = FlagUtil.GetFlag(data, ofs, i);
                if (flag)
                {
                    receivedItems.Add(items[i], new Dictionary<int, string>());
                }
                else
                {
                    availableItems.Add(items[i], new Dictionary<int, string>());
                }
            }
        }

        private void FillVariants(IReadOnlyList<string> items)
        {
            var str = GameInfo.Strings;
            var invert = ItemRemakeUtil.GetInvertedDictionary();
            var ofs = Player.Personal.Offsets.ItemRemakeCollectBit;
            var max = Player.Personal.Offsets.MaxRemakeBitFlag;
            var data = Player.Personal.Data;
            for (int i = 0; i < max; i++)
            {
                var remakeIndex = i >> 3; // ItemRemakeInfo.BodyColorCountMax
                var variant = i & 7;

                ushort itemId = invert.TryGetValue((short)remakeIndex, out var id) ? id : (ushort)0;
                var itemName = remakeIndex == 0652 ? "photo" : items[itemId];

                var flag = FlagUtil.GetFlag(data, ofs, i);
                string name =  $"{itemName}";

                if (ItemRemakeInfoData.List.TryGetValue((short)remakeIndex, out var info))
                    name = $"{name} ({info.GetBodyDescription(variant, str)})";

                if (flag)
                {
                    if (!receivedItems.ContainsKey(itemName))
                    {
                        receivedItems[itemName] = new Dictionary<int, string>();
                    }
                    receivedItems[itemName][variant] = name;
                }
                else
                {
                    if (!availableItems.ContainsKey(itemName))
                    {
                        availableItems[itemName] = new Dictionary<int, string>();
                    }
                    availableItems[itemName][variant] = name;
                }
            }
        }

    }
}
