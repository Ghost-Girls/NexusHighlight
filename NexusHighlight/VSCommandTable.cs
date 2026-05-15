using System;

namespace NexusHighlight
{
    internal static class PackageGuids
    {
        public const string guidNexusHighlightPackageString = "35348A1B-994B-4378-B2C1-8B7261FE76D7";
        public const string guidNexusHighlightPackageCmdSetString = "09e59564-c21a-44f8-ae2b-c2bc17facd08";
        
        public static readonly Guid guidNexusHighlightPackage = new Guid(guidNexusHighlightPackageString);
        public static readonly Guid guidNexusHighlightPackageCmdSet = new Guid(guidNexusHighlightPackageCmdSetString);
    }

    internal static class PackageIds
    {
        public const int ContextMenuGroup = 0x0001;
        public const int CreateEditForegroundRule = 0x0100;
        public const int CreateEditBackgroundRule = 0x0101;
    }
}
