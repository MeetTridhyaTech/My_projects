using System.ComponentModel;

public class PaginationParams
{
    public string SearchQuery { get; set; } = "";
    public string SortBy { get; set; } = "";
    public bool IsDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<FilterCriteria>? Filters { get; set; }

}

public class FilterCriteria
{
    public string ColumnName { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

