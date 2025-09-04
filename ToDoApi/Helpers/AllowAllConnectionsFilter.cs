using Hangfire.Dashboard;

public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Bu metodun true dönmesi, dashboard'a erişime izin verildiği anlamına gelir.
        return true;
    }
}
