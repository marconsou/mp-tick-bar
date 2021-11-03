using MPTickBar.Properties;
using System;
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
            using var memoryStream = new MemoryStream(Resources.LevelModifier);
            using var streamReader = new StreamReader(memoryStream);
            string line;
            var level = 1;
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
                else
                {
                    var data = line.Split('|');

                    if (data.Length != 2)
                        throw new Exception($"The level {level} data is invalid!");

                    sub = int.Parse(data[0]);
                    div = int.Parse(data[1]);
                }
                LevelModifier.LevelModifierData.Add(new() { Sub = sub, Div = div });
                level++;
            }
        }

        public static int GetLevelModifierSub(int level)
        {
            return (level > 0) ? LevelModifier.LevelModifierData[level - 1].Sub : 0;
        }

        public static int GetLevelModifierDiv(int level)
        {
            return (level > 0) ? LevelModifier.LevelModifierData[level - 1].Div : 0;
        }
    }
}