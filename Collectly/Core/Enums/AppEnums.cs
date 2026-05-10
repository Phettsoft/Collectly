namespace Collectly.Core.Enums;

public enum ItemPriority
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum SortOrder
{
    DateCreatedDesc,
    DateCreatedAsc,
    NameAsc,
    NameDesc,
    PriceAsc,
    PriceDesc,
    PriorityDesc,
    PriorityAsc
}

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public enum EventType
{
    None,
    Birthday,
    Christmas,
    Anniversary,
    Wedding,
    BabyShower,
    Graduation,
    Housewarming,
    Valentines,
    MothersDay,
    FathersDay,
    Easter,
    Halloween,
    Thanksgiving,
    Other
}
