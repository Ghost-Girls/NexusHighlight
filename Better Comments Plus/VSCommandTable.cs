using System;

namespace BetterCommentsPlus
{
    internal static class PackageGuids
    {
        public const string guidBetterCommentsPlusPackageString = "09e59564-c21a-44f8-ae2b-c2bc17facd07";
        public const string guidBetterCommentsPlusPackageCmdSetString = "09e59564-c21a-44f8-ae2b-c2bc17facd08";
        
        public static readonly Guid guidBetterCommentsPlusPackage = new Guid(guidBetterCommentsPlusPackageString);
        public static readonly Guid guidBetterCommentsPlusPackageCmdSet = new Guid(guidBetterCommentsPlusPackageCmdSetString);
    }

    internal static class PackageIds
    {
        public const int ContextMenuGroup = 0x0001;
        public const int CreateEditForegroundStyleRule = 0x0100;
        public const int CreateEditBackgroundStyleRule = 0x0101;
    }
}
