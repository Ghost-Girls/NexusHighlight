using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using BetterCommentsPlus.Options;

namespace BetterCommentsPlus
{
   [ProvideOptionPage(typeof(OptionsGeneralPage), "Better Comments Plus", "General", 0, 0, true)]
   [ProvideOptionPage(typeof(OptionsRulesPage), "Better Comments Plus", "Rules", 0, 0, true)]
   [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading =true)]
   [InstalledProductRegistration("#110", "#112", Vsix.Id, IconResourceID = 400)]
   [ProvideMenuResource("Menus.ctmenu", 1)]
   [Guid(PACKAGE_GUID_STRING)]
   [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
   public sealed class VsPackage : AsyncPackage
   {
      public const string PACKAGE_GUID_STRING = "09e59564-c21a-44f8-ae2b-c2bc17facd07";

      protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<Microsoft.VisualStudio.Shell.ServiceProgressData> progress)
      {
         await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
         
         var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
         if (commandService != null)
         {
             Commands.CreateEditRule.Initialize(this, commandService);
         }
      }
   }
}
