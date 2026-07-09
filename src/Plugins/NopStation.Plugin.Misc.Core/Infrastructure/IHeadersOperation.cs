using Microsoft.OpenApi.Models;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public interface IHeadersOperation
{
    string GroupName { get; }

    IList<OpenApiParameter> GetApiParameters();
}
