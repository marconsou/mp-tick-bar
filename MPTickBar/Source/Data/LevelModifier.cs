using MPTickBar.Properties;
using System.Collections.Generic;
using System.IO;

namespace MPTickBar
{
    public static class LevelModifier
    {
        private static List<Data> LevelModifierData { get; } = new();

        private readonly struct Data
        {
            public int Sub { get; init; }

            public int Div { get; init; }
        }

        static LevelModifier()
        {
            using var memoryStream = new MemoryStream(Resources.levelmodifier);
            using var streamReader = new StreamReader(memoryStream);
            string line;
            byte level = 1;
            const byte levelMax = 90;
            while ((line = streamReader.ReadLine()) != null)
            {
                line = line.Trim();
                var sub = 0;
                var div = 0;

                if (level <= 50)
                {
                    var data = int.Parse(line);
                    sub = data;
                    div = data;
                }
                else if (level <= levelMax)
                {
                    var data = line.Split('|');

                    if (data.Length != 2)
                        throw new($"The level {level} data is invalid!");

                    sub = int.Parse(data[0]);
                    div = int.Parse(data[1]);
                }
                else
                {
                    var index = levelMax - 1;
                    sub = LevelModifier.LevelModifierData[index].Sub;
                    div = LevelModifier.LevelModifierData[index].Div;
                }
                LevelModifier.LevelModifierData.Add(new() { Sub = sub, Div = div });
                level++;
            }
        }

        public static int GetLevelModifierSub(int level) => (level > 0) ? LevelModifier.LevelModifierData[level - 1].Sub : 0;

        public static int GetLevelModifierDiv(int level) => (level > 0) ? LevelModifier.LevelModifierData[level - 1].Div : 0;
    }
}