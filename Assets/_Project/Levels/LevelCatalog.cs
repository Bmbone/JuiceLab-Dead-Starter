using System;

namespace JuiceStudy.Levels
{
    public enum LevelId { Courtyard = 1, TwinTowers = 2, Crown = 3 }

    public sealed class LevelDefinition
    {
        private readonly string[] pattern;
        public LevelId Id { get; }
        public int Number => (int)Id;
        public string Name { get; }
        public string Description { get; }
        public int Rows => pattern.Length;
        public int Columns => pattern[0].Length;
        public int BrickCount { get; }
        public int MaximumScore => BrickCount * 10;

        internal LevelDefinition(LevelId id, string name, string description, params string[] rows)
        {
            Id = id;
            Name = name;
            Description = description;
            if (rows == null || rows.Length == 0 || string.IsNullOrEmpty(rows[0]))
                throw new ArgumentException("A level must contain a non-empty brick pattern.");
            pattern = (string[])rows.Clone();
            int count = 0;
            foreach (string row in pattern)
            {
                if (row == null || row.Length != Columns) throw new ArgumentException("Level rows must have matching widths.");
                foreach (char cell in row)
                {
                    if (cell == '#') count++;
                    else if (cell != '.') throw new ArgumentException("Use # for bricks and . for gaps.");
                }
            }
            if (count == 0) throw new ArgumentException("A level needs at least one brick.");
            BrickCount = count;
        }

        public bool HasBrick(int row, int column)
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException();
            return pattern[row][column] == '#';
        }
    }

    public static class LevelCatalog
    {
        private static readonly LevelDefinition Courtyard = new LevelDefinition(LevelId.Courtyard, "Courtyard",
            "A classic wall of bricks. Learn your spells and find your rhythm.",
            "#######", "#######", "#######", "#######");
        private static readonly LevelDefinition TwinTowers = new LevelDefinition(LevelId.TwinTowers, "Twin Towers",
            "Two tall towers with a clear center lane. Aim for either flank.",
            "##...##", "##...##", "##...##", "##...##", "##...##", "###.###");
        private static readonly LevelDefinition Crown = new LevelDefinition(LevelId.Crown, "Crown",
            "A crown-shaped formation with narrow tips and a dense center.",
            "#..#..#", "##.#.##", "#######", ".#####.", "..###..", "...#...");

        public static LevelDefinition Get(LevelId id)
        {
            switch (id)
            {
                case LevelId.Courtyard: return Courtyard;
                case LevelId.TwinTowers: return TwinTowers;
                case LevelId.Crown: return Crown;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}
