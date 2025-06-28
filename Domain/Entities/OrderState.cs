using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    public enum OrderState
    {
        Initial,
        Pending,
        Paid,
        Refunded,
        Completed,
        Cancelled,
    }


    record OrderTransitionRule(OrderState State, List<OrderState> NextStates);

    public class OrderTransitionValidator
    {
        private static readonly OrderTransitionRule[] Rules = [
            new OrderTransitionRule(OrderState.Initial, new List<OrderState> { OrderState.Pending }),
            new OrderTransitionRule(OrderState.Pending, new List<OrderState> { OrderState.Paid, OrderState.Cancelled }),
            new OrderTransitionRule(OrderState.Paid, new List<OrderState> { OrderState.Completed, OrderState.Refunded }),
            new OrderTransitionRule(OrderState.Refunded, new List<OrderState> { OrderState.Cancelled }),
            new OrderTransitionRule(OrderState.Completed, new List<OrderState> { OrderState.Cancelled }),
            new OrderTransitionRule(OrderState.Cancelled, new List<OrderState>())
        ];
        public static bool IsValidTransition(OrderState from, OrderState to)
            => Rules.Any(rule => rule.State == from && rule.NextStates.Contains(to));
    }
}