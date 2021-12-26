namespace AzerothMemories.Blizzard.Models.GameDataApi;

/// <summary>
/// A recipe category.
/// </summary>
public record RecipeCategory
{
    /// <summary>
    /// Gets the name of the recipe category.
    /// </summary>
    [JsonPropertyName("name")]
    public Name Name { get; init; }

    /// <summary>
    /// Gets the recipes in the recipe category.
    /// </summary>
    [JsonPropertyName("recipes")]
    public RecipeReference[] Recipes { get; init; }
}