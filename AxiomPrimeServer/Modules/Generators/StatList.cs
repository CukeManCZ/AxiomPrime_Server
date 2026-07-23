using System;
using System.Collections.Generic;

[Serializable]
public enum StatType
{
    Global,
    Offensive,
    Defensive,
    Propulsion,
    Miscellaneous
}

public enum FightStats
{
    Offensive,
    Defensive,
    Propulsion
}



public class StatList
{
    private static StatList instance;

    public static StatList Instance
    {
        get
        {
            if (instance == null)
                instance = new StatList();
            return instance;
        }
    }
    private List<StatDefinition> allStats;

    private StatList()
    {
        allStats = new List<StatDefinition>()
        {
        // Global
        new StatDefinition("Energy cost"                 , false, 1f    , new List<string>{"Global"}                                ,StatType.Global,           0f,     0f, ""   ),
        new StatDefinition("weight"                             , false, 1f    , new List<string>{"Global"}                                ,StatType.Global,           0f,     0f, ""   ),
        new StatDefinition("Energy grid"              , false, 1f    , new List<string>{"Global"}                                ,StatType.Global,           0f,     0f, ""   ),
        new StatDefinition("Size"                               , false, 1f    , new List<string>{"Global"}                                ,StatType.Global,           0f,     0f, ""   ),

        // Weapons
        new StatDefinition("Damage"                             , false, 1f    , new List<string>{"Weapon"}                                ,StatType.Offensive,        0f,     0f      , "The base amount of damage dealt by the ship's weapons in each attack."),
        new StatDefinition("Damage bonus"                     , true, 5f    , new List<string>{"Weapon", "Miscellaneous:Amplifier"}     ,StatType.Offensive,        0f,     0f      , "A percentage increase applied to the total damage output, amplifying overall offensive power."),
        new StatDefinition("Critical chance"                      , true, 5f    , new List<string>{"Weapon"}                                ,StatType.Offensive,        100f,   5f    , "The probability of landing a critical hit, which typically deals 2x bonus damage."),
        new StatDefinition("Armor penetration"              , true, 3f    , new List<string>{"Weapon:Kinetic", "Weapon:Hybrid"}       ,StatType.Offensive,        100f,     5f      , "Reduces the effectiveness of enemy armor, allowing more damage to bypass armor layers."),
        new StatDefinition("Shield penetration"             , true, 3f    , new List<string>{"Weapon:Energy", "Weapon:Hybrid"}        ,StatType.Offensive,        100f,     5f      , "Reduces the effectiveness of enemy shields, allowing more damage to bypass shield layers."),

        // Defense
        new StatDefinition("Hit points"                         , false, 0.5f  , new List<string>{"Defense"}                               ,StatType.Defensive,        0f,     0f      , "The total health pool of the ship; when depleted, the ship is destroyed."),
        new StatDefinition("Damage reduction"                 , true, 5f    , new List<string>{"Defense", "Miscellaneous"}              ,StatType.Defensive,        75f,    5f     , "A percentage reduction applied to all incoming damage, enhancing overall survivability."),
        new StatDefinition("Armor"                              , false, 1f    , new List<string>{"Defense:Armor"}                         ,StatType.Defensive,        0f,     0f      , "Provides resistance to physical damage, reducing the amount of damage that reaches hit points."),
        new StatDefinition("Shield"                             , false, 1f    , new List<string>{"Defense:Shield"}                        ,StatType.Defensive,        0f,     0f      , "An energy barrier that absorbs incoming damage before it affects hit points."),
        new StatDefinition("Reflect chance"                   , true, 5f    , new List<string>{"Defense"}                               ,StatType.Defensive,        50f,    5f     , "The probability of reflecting incoming damage back to the attacker."),

        // Propulsion
        new StatDefinition("Speed"                              , false, 2f    , new List<string>{"Propulsion"}                            ,StatType.Propulsion,       0f,     0f     , "Determines how quickly the ship can move through space, affecting initiative in fight and travel time."),
        new StatDefinition("Dodge chance"                     , true, 5f    , new List<string>{"Propulsion"}                            ,StatType.Propulsion,       50f,    5f    , "The probability of evading incoming attacks, allowing the ship to avoid damage entirely."),

        // Misc
        new StatDefinition("Energy generation"                  , false, 5f    , new List<string>{"Miscellaneous:Generator"}               ,StatType.Miscellaneous,    0f,     0f      , "The rate at which the ship produces energy, used for missions."),
        new StatDefinition("Energy mitigation"      , false, 5f    , new List<string>{"Miscellaneous"}                         ,StatType.Miscellaneous,    0f,     0f      , "Reduces the impact of energy cost of modules fitted into the ship.")
        };
    }


    [Serializable]
    public class StatDefinition
    {
        public string Name;
        public bool IsPercentage;
        public float Weight;
        public List<string> AppliesTo;
        public StatType Type;
        public float MaxValue;
        public float MaxValuePerItem;
        public string Description;
        public StatDefinition(string name, bool isPercentage, float weight, List<string> appliesTo, StatType type, float maxValue, float maxValuePerItem, string description)
        {
            Name = name;
            IsPercentage = isPercentage;
            Weight = weight;
            AppliesTo = appliesTo;
            Type = type;
            MaxValue = maxValue;
            MaxValuePerItem = maxValuePerItem;
            Description = description;
        }
    }
    public List<StatDefinition> GetAllStats()
    {
        return allStats;
    }

    public string GetStatDescription(string statName)
    {
        foreach (var stat in allStats)
        {
            if (stat.Name.Equals(statName, System.StringComparison.OrdinalIgnoreCase))
                return stat.Description;
        }
        return "Description not found.";
    }
};
