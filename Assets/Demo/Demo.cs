using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe class Demo : MonoBehaviour
{
	private RecipeBook _recipeBook;
	private Inventory _inventory;
	private ArenaAllocator _allocator;
	private CraftSimulator _craftSimulator;

	protected void Start()
	{
		_recipeBook = new RecipeBook();
		_inventory = new Inventory();

		var _sizeOfNode = UnsafeUtility.SizeOf<CraftNode>();
		_allocator = new ArenaAllocator(_sizeOfNode * 10);
		_craftSimulator = new CraftSimulator(_recipeBook, _inventory);

		_recipeBook.AddRecipe(new Recipe(ItemType.IronIngot, new[]
		                                                     {
			                                                     new Ingredient { IngredientType = ItemType.IronOre, Amount = 2 }
		                                                     }));

		_recipeBook.AddRecipe(new Recipe(ItemType.IronSword, new[]
		                                                     {
			                                                     new Ingredient { IngredientType = ItemType.IronIngot, Amount = 4 },
			                                                     new Ingredient { IngredientType = ItemType.Stick, Amount = 1 }
		                                                     }));

		_inventory.AddItem(ItemType.IronOre, 20);
		_inventory.AddItem(ItemType.Stick, 10);

		var root = _craftSimulator.SimulateCraft(_allocator, ItemType.IronSword, 1);

		Debug.Log($"Can Craft {root->OutputItem}: {root->AmountAvailable}/{root->AmountNeeded}");

		for (var i = 0; i < root->SubCount; i++)
		{
			var sub = root->subIngredients[i];
			Debug.Log($"  Sub Ingredient {i}: {sub->OutputItem}, Available: {sub->AmountAvailable}, Needed: {sub->AmountNeeded}");
		}

		// Clean up
		_allocator.Reset();
		
		
	}

	private void OnDestroy()
	{
		_allocator.Dispose();
	}
}