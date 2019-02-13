# DevExpressSortByReflectionProblem

A sample application that [DevExpress Issue](https://www.devexpress.com/Support/Center/Question/Details/T715393/datagrid-virtual-sources-sortby-reflection-problem) when using DevExpress `DataGrid Virtual Sources` with sorting by an inherited property.

The problem is in the `Devexpress.Xpf.Data.ExpressionHelper.GetProperty<T>(string propertyName)` method.
```csharp
public static PropertyInfo GetProperty<T>(string propertyName)
{
  return typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
}
```
When calling that on a property from a base class, the return value is different than on an directly declared property.
The difference is the `ReflectedType` property.
That causes that the `DevExpress.Xpf.Data.GridQueryableExtensions.Orderby<T, TProperty>(IQueryable<T> query, SortDefinition sortDescription)` doesn´t add the Linq `OrderBy()` expression the right way.
At least not when using `Microsoft.EntityFrameworkCore 2.2.0` as the `IQueryable<T>` provider.

A fix would be to change the `GetProperty<T>(string propertyName)` method to something like that:
```csharp
public static PropertyInfo GetProperty<T>(string propertyName)
{
    var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
    if (propertyInfo.DeclaringType != propertyInfo.ReflectedType)
        return propertyInfo.DeclaringType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
    return propertyInfo;
}
```

More details can be found in [MainViewModel](https://github.com/rzlsoftware/DevExpressSortByReflectionProblem/blob/912953eafd52ba1a36183a00df1b88ab812eeb48/DevExpressSortByReflectionProblem/ViewModels/MainViewModel.cs#L40) constructor.
