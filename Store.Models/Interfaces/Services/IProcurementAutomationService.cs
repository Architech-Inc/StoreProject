using System.Threading;
using System.Threading.Tasks;

namespace Store.Models.Interfaces.Services;

public interface IProcurementAutomationService
{
    /// <summary>
    /// Scans branch item stocks against reorder thresholds and automatically drafts Purchase Orders.
    /// Returns the number of purchase orders created.
    /// </summary>
    Task<int> EvaluateInventoryThresholdsAsync(CancellationToken cancellationToken = default);
}
