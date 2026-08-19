#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Editor utility that authors the Draft Day ScriptableObjects from the design table.</summary>
public static class DraftDayDataGenerator
{
    private const string OutputFolder = "Assets/Scripts/MiniGames/DraftDay/SOs/DraftDaySO";
    private const string ConfigAssetName = "DDDraftDaySO.asset";

    private struct AssetSeed
    {
        public string Name;
        public DDAssetCategory Category;
        public float[] Returns;
    }

    private struct OpponentSeed
    {
        public string Name;
        public string[] Assets;
        public int[] Weights;
    }

    private static readonly AssetSeed[] AssetSeeds =
    {
        new() { Name = "Bitcoin",    Category = DDAssetCategory.Crypto,    Returns = new[] {  6.95f,  3.25f, -1.25f, -4.25f, -3.65f,  1.85f,  0.95f } },
        new() { Name = "Ethereum",   Category = DDAssetCategory.Crypto,    Returns = new[] {  6.40f,  2.70f,  0.70f, -6.80f, -0.60f, -0.50f,  2.30f } },
        new() { Name = "Gold",       Category = DDAssetCategory.Commodity, Returns = new[] { -0.82f, -0.88f,  0.36f,  2.42f,  1.22f, -0.86f,  0.10f } },
        new() { Name = "Copper",     Category = DDAssetCategory.Commodity, Returns = new[] { -1.10f,  2.86f, -0.96f,  0.46f, -1.30f,  2.24f, -0.24f } },
        new() { Name = "Nat Gas",    Category = DDAssetCategory.Commodity, Returns = new[] {  1.57f, -3.15f,  1.67f, -1.35f,  2.31f, -1.93f, -0.17f } },
        new() { Name = "Silver",     Category = DDAssetCategory.Commodity, Returns = new[] { -1.52f, -2.00f,  2.35f,  2.15f,  1.12f, -0.58f,  0.23f } },
        new() { Name = "World ETF",  Category = DDAssetCategory.Etf,       Returns = new[] {  1.20f,  1.55f, -0.95f, -0.70f, -0.55f,  0.40f,  0.45f } },
        new() { Name = "Tech ETF",   Category = DDAssetCategory.Etf,       Returns = new[] {  1.70f,  1.50f,  0.28f, -1.28f, -0.62f, -0.24f,  0.90f } },
        new() { Name = "Bank Index", Category = DDAssetCategory.Etf,       Returns = new[] { -0.60f, -0.82f,  1.26f,  0.88f,  1.28f, -0.08f, -0.66f } },
        new() { Name = "Apple",      Category = DDAssetCategory.Stock,     Returns = new[] {  1.95f, -0.38f,  0.52f, -0.85f,  1.83f, -1.41f,  0.44f } },
        new() { Name = "Tesla",      Category = DDAssetCategory.Stock,     Returns = new[] {  2.67f,  4.49f, -1.33f, -3.27f, -3.09f,  2.83f,  0.85f } }
    };

    private static readonly OpponentSeed[] OpponentSeeds =
    {
        new() { Name = "Zara H",   Assets = new[] { "Gold", "Bank Index", "World ETF", "Copper", "Bitcoin" },     Weights = new[] { 25, 25, 20, 20, 10 } },
        new() { Name = "Hina M",   Assets = new[] { "Bank Index", "World ETF", "Gold", "Tech ETF", "Copper" },    Weights = new[] { 30, 30, 25, 10,  5 } },
        new() { Name = "Bilal K",  Assets = new[] { "World ETF", "Copper", "Silver", "Apple", "Bitcoin" },        Weights = new[] { 25, 20, 20, 20, 15 } },
        new() { Name = "Noor E",   Assets = new[] { "Bitcoin", "Bank Index", "Gold", "World ETF", "Apple" },      Weights = new[] { 20, 30, 30, 15,  5 } },
        new() { Name = "Faraz A",  Assets = new[] { "Gold", "Silver", "Copper", "World ETF", "Bitcoin" },         Weights = new[] { 35, 30, 25,  5,  5 } },
        new() { Name = "Dev P",    Assets = new[] { "Tech ETF", "Apple", "Tesla", "Copper", "Gold" },             Weights = new[] { 35, 25, 20, 15,  5 } },
        new() { Name = "Sana J",   Assets = new[] { "Tesla", "Tech ETF", "Apple", "Gold", "Bitcoin" },            Weights = new[] { 45, 20, 15, 15,  5 } },
        new() { Name = "Ayesha R", Assets = new[] { "Ethereum", "Tesla", "Tech ETF", "Nat Gas", "Gold" },         Weights = new[] { 55, 20, 15,  5,  5 } },
        new() { Name = "Imran S",  Assets = new[] { "Nat Gas", "Silver", "Copper", "World ETF", "Bitcoin" },      Weights = new[] { 40, 20, 20, 10, 10 } }
    };

    [MenuItem("Tools/Draft Day/Generate Data")]
    public static void Generate()
    {
        Directory.CreateDirectory(OutputFolder);

        Dictionary<string, DDAssetSO> created = new();

        foreach (AssetSeed seed in AssetSeeds)
        {
            string path = $"{OutputFolder}/DDAsset_{seed.Name.Replace(" ", string.Empty)}.asset";
            DDAssetSO asset = AssetDatabase.LoadAssetAtPath<DDAssetSO>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DDAssetSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.displayName = seed.Name;
            asset.category = seed.Category;
            asset.dailyReturns = seed.Returns;
            EditorUtility.SetDirty(asset);
            created[seed.Name] = asset;
        }

        string configPath = $"{OutputFolder}/{ConfigAssetName}";
        DDDraftDaySO config = AssetDatabase.LoadAssetAtPath<DDDraftDaySO>(configPath);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<DDDraftDaySO>();
            AssetDatabase.CreateAsset(config, configPath);
        }

        config.pool = new List<DDAssetSO>();
        foreach (AssetSeed seed in AssetSeeds) config.pool.Add(created[seed.Name]);

        config.rules = new List<DDCategoryRule>
        {
            new() { category = DDAssetCategory.Commodity, ruleType = DDRuleType.Minimum, amount = 1 },
            new() { category = DDAssetCategory.Etf,       ruleType = DDRuleType.Minimum, amount = 1 },
            new() { category = DDAssetCategory.Crypto,    ruleType = DDRuleType.Maximum, amount = 1 }
        };

        config.opponents = new List<DDOpponent>();
        foreach (OpponentSeed seed in OpponentSeeds)
        {
            DDOpponent opponent = new() { displayName = seed.Name };
            for (int i = 0; i < seed.Assets.Length; i++)
            {
                opponent.roster.Add(new DDRosterEntry { asset = created[seed.Assets[i]], weight = seed.Weights[i] });
            }
            config.opponents.Add(opponent);
        }

        config.playerEntryName = "You";
        config.infoDrafting = "Scored on return per unit of risk, not raw return.";
        config.infoPlayingWeek = "Week in progress — steadier squads climb as the swings even out.";
        config.infoResolved = "You returned {0}% and scored {1}. The top squads returned less and scored more.";
        config.coachBuckets = BuildCoachBuckets();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = config;

        Debug.Log($"Draft Day data generated at {OutputFolder}");
    }

    private static List<DDCoachBucket> BuildCoachBuckets()
    {
        return new List<DDCoachBucket>
        {
            new()
            {
                situation = DDCoachSituation.ConstraintOpen, mood = DDCoachMood.Neutral,
                lines = new[]
                {
                    "You still owe me a commodity. Don't leave it to the last round.",
                    "No ETF yet. That's the boring pick that saves your week."
                }
            },
            new()
            {
                situation = DDCoachSituation.CryptoFilled, mood = DDCoachMood.Neutral,
                lines = new[]
                {
                    "That's your one crypto. Spend the rest on things that don't move with it.",
                    "Crypto's filled. Now build something around it."
                }
            },
            new()
            {
                situation = DDCoachSituation.RosterComplete, mood = DDCoachMood.Happy,
                lines = new[]
                {
                    "Five names. Now the real decision — how much of each.",
                    "Squad's in. Weights are where the week is won."
                }
            },
            new()
            {
                situation = DDCoachSituation.WeightsBalanced, mood = DDCoachMood.Happy,
                lines = new[]
                {
                    "Now that's a squad. Nothing here can sink you alone.",
                    "Spread like that, a bad day is just a bad day."
                }
            },
            new()
            {
                situation = DDCoachSituation.WeightsConcentrated, mood = DDCoachMood.Sad,
                lines = new[]
                {
                    "Forty percent on one name? That's not a squad, that's a bet.",
                    "You've got five players and you're only really playing one."
                }
            },
            new()
            {
                situation = DDCoachSituation.ResultTop, mood = DDCoachMood.Happy,
                lines = new[]
                {
                    "Told you. Steady wins it.",
                    "You didn't chase the big number and you still finished ahead of the ones who did."
                }
            },
            new()
            {
                situation = DDCoachSituation.ResultMid, mood = DDCoachMood.Neutral,
                lines = new[]
                {
                    "Middle of the pack. The shape was close — the weights weren't.",
                    "Not a bad week. Not a memorable one either."
                }
            },
            new()
            {
                situation = DDCoachSituation.ResultBottom, mood = DDCoachMood.Sad,
                lines = new[]
                {
                    "You made more money than half this table and still finished behind them. That's the whole lesson.",
                    "Big swings, small score. The market noticed."
                }
            }
        };
    }
}
#endif
