public unsafe struct CraftNode
{
	public CraftNode** subIngredients; // Pointer to an array of pointers to CraftNode

	public ItemType OutputItem;

	public int AmountNeeded;
	public int AmountAvailable;
	public int SubCount;
}

public unsafe class CraftSimulator
{
	private readonly RecipeBook _recipeBook;
	private readonly Inventory _inventory;

	public CraftSimulator(RecipeBook recipeBook, Inventory inventory)
	{
		_recipeBook = recipeBook;
		_inventory = inventory;
	}

	public CraftNode* SimulateCraft(ArenaAllocator allocator, ItemType item, int amountNeeded)
	{
		var node = allocator.Alloc<CraftNode>();
		node->OutputItem = item;
		node->AmountNeeded = amountNeeded;

		if (_recipeBook.TryGetRecipe(item, out var recipe))
		{
			node->SubCount = recipe.Ingredients.Length;
			node->subIngredients = (CraftNode**)allocator.Alloc<byte>(sizeof(CraftNode*) * node->SubCount);

			var maxCraftable = int.MaxValue;

			for (var i = 0; i < recipe.Ingredients.Length; i++)
			{
				var ingredient = recipe.Ingredients[i];
				var requiredAmount = ingredient.Amount * amountNeeded;

				var sub = SimulateCraft(allocator, ingredient.IngredientType, requiredAmount);
				node->subIngredients[i] = sub;

				var possible = sub->AmountAvailable / ingredient.Amount;

				if (possible < maxCraftable)
				{
					maxCraftable = possible; // Find the minimum across all ingredients
				}
			}

			node->AmountAvailable = maxCraftable;
		}
		else
		{
			node->SubCount = 0;
			node->subIngredients = null;
			node->AmountAvailable = _inventory.GetCount(item);
		}

		return node;
	}
}