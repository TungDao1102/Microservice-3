using InventoryService.Dtos;

namespace InventoryService.Clients
{
    public class CatalogClient(HttpClient httpClient)
    {
        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
        {
            var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");
            return items ?? [];
        }
    }
}
