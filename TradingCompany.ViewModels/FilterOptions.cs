using TradingCompany.Core.DTOs;

namespace TradingCompany.ViewModels;

public record StatusFilterItem(int? Id, string Name);

public record SortOption(OrderSortField Field, string Title);
