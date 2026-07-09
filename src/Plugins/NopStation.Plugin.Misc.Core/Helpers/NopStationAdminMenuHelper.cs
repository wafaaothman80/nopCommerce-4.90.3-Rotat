using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.Core.Helpers;
public static class NopStationAdminMenuHelper
{
    private static bool Insert(AdminMenuItem currentMenuItem, string itemSystemName, AdminMenuItem newMenuItem)
    {
        var position = 0;
        var inserted = false;

        foreach (var adminMenuItem in currentMenuItem.ChildNodes.ToList())
            if (!adminMenuItem.SystemName.Equals(itemSystemName))
                position += 1;
            else
            {
                adminMenuItem.ChildNodes.Add(newMenuItem);
                inserted = true;
                break;
            }

        if (inserted)
            return true;

        foreach (var adminMenuItem in currentMenuItem.ChildNodes)
        {
            inserted = Insert(adminMenuItem, itemSystemName, newMenuItem);

            if (inserted)
                break;
        }

        return inserted;
    }

    public static bool InsertInside(this AdminMenuItem adminMenuItem, string itemSystemName, AdminMenuItem newMenuItem)
    {
        return Insert(adminMenuItem, itemSystemName, newMenuItem);
    }
}
