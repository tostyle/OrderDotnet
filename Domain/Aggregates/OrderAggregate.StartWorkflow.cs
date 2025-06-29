using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing loyalty burning logic
/// </summary>
public partial class OrderAggregate
{
    public Order StartWorkflow(string workflowId)
    {
        _order.SetWorkflowId(workflowId);
        return _order;
    }
}