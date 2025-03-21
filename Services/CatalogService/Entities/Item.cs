﻿using BuildingBlocks.Common.Abstractions;

namespace CatalogService.Entities
{
    public class Item : IBaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
