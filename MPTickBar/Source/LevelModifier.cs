using MPTickBar.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace MPTickBar
{
    public static class LevelModifier
    {
        private struct Data
        {
            public int Sub { get; init; }

            public int Div { get; init; }

            public Data(int sub, int div)
            {
                this.Sub = sub;
                this.Div = div;
            }
        }

        private static List<Data> DataList { get; set; } = new List<Data>();

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
                        throw new Exception("LevelModifier invalid!");

                    sub = int.Parse(data[0]);
                    div = int.Parse(data[1]);
                }
                LevelModifier.DataList.Add(new Data(sub, div));
                level++;
            }
        }

        public static int GetLevelModifierSub(int level)
        {
            if (level <= 0) return 0;
            return LevelModifier.DataList[level - 1].Sub;
        }

        public static int GetLevelModifierDiv(int level)
        {
            if (level <= 0) return 0;
            return LevelModifier.DataList[level - 1].Div;
        }
    }
}