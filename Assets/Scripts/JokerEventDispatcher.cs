public static class JokerEventDispatcher
{
    public static void DispatchHandEvent(JokerEventType eventType, HandContext context)
    {
        if (context?.ActiveJokers == null)
            return;

        foreach (JokerBase joker in context.ActiveJokers)
        {
            if (joker == null)
                continue;

            switch (eventType)
            {
                case JokerEventType.BeforeHandScored:
                    joker.OnBeforeHandScored(context);
                    break;
                case JokerEventType.AfterHandScored:
                    joker.OnAfterHandScored(context);
                    break;
            }
        }
    }

    public static void DispatchRoundEvent(JokerEventType eventType, JokerRoundContext context)
    {
        if (context?.ActiveJokers == null)
            return;

        foreach (JokerBase joker in context.ActiveJokers)
        {
            if (joker == null)
                continue;

            switch (eventType)
            {
                case JokerEventType.RoundStarted:
                    joker.OnRoundStarted(context);
                    break;
                case JokerEventType.RoundWon:
                    joker.OnRoundWon(context);
                    break;
                case JokerEventType.RoundLost:
                    joker.OnRoundLost(context);
                    break;
            }
        }
    }

    public static void DispatchShopEvent(JokerShopContext context)
    {
        if (context?.ActiveJokers == null)
            return;

        foreach (JokerBase joker in context.ActiveJokers)
        {
            if (joker == null)
                continue;

            joker.OnShopEntered(context);
        }
    }
}
