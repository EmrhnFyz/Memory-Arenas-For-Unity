using System.Collections.Generic;

public enum ItemType
{
	IronOre,
	IronIngot,
	IronSword,
	Wood,
	Stick
}

public struct Ingredient
{
	public ItemType IngredientType;
	public int Amount;
}

public class RecipeBook
{
	private readonly Dictionary<ItemType, Recipe> recipes = new();

	public void AddRecipe(Recipe recipe) => recipes[recipe.Result] = recipe;

	public bool TryGetRecipe(ItemType item, out Recipe recipe) => recipes.TryGetValue(item, out recipe);
}

public class Recipe
{
	public ItemType Result { get; }
	public Ingredient[] Ingredients { get; }

	public Recipe(ItemType result, Ingredient[] ingredients)
	{
		Result = result;
		Ingredients = ingredients;
	}
}

public class Inventory
{
	private readonly Dictionary<ItemType, int> stock = new();
	public int GetCount(ItemType item) => stock.GetValueOrDefault(item, 0);

	public void AddItem(ItemType item, int amount)
	{
		stock.TryAdd(item, 0);
		stock[item] += amount;
	}
}