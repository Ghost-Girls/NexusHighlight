using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using NexusHighlight.Options;

namespace NexusHighlight
{
    [ProvideOptionPage(typeof(OptionsGeneralPage), "NexusHighlight", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(OptionsRulesPage), "NexusHighlight", "Rules", 0, 0, true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PACKAGE_GUID_STRING)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
                      Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VsPackage : AsyncPackage, IVsSolutionEvents
    {
        public const string PACKAGE_GUID_STRING = "35348A1B-994B-4378-B2C1-8B7261FE76D7";
        private uint _solutionEventsCookie;

        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<Microsoft.VisualStudio.Shell.ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            if (commandService != null)
            {
                Commands.CreateEditRule.Initialize(this, commandService);
            }

            // 订阅解决方案事件
            var solutionService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (solutionService != null)
            {
                solutionService.AdviseSolutionEvents(this, out _solutionEventsCookie);

                // 检查是否已经有打开的解决方案
                TryLoadExistingSolution(solutionService);
            }
        }

        private void TryLoadExistingSolution(IVsSolution solutionService)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                solutionService.GetSolutionInfo(out string solutionDirectory, out string solutionFile, out string userOptsFile);
                if (!string.IsNullOrEmpty(solutionFile))
                {
                    Settings.Instance.SetCurrentSolutionPath(solutionFile);
                }
            }
            catch
            {
                // 静默处理错误
            }
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposing && _solutionEventsCookie != 0)
            {
                var solutionService = GetService(typeof(SVsSolution)) as IVsSolution;
                if (solutionService != null)
                {
                    solutionService.UnadviseSolutionEvents(_solutionEventsCookie);
                }
            }
            base.Dispose(disposing);
        }

        #region IVsSolutionEvents Implementation

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var solutionService = GetService(typeof(SVsSolution)) as IVsSolution;
                if (solutionService != null)
                {
                    solutionService.GetSolutionInfo(out string solutionDirectory, out string solutionFile, out string userOptsFile);
                    if (!string.IsNullOrEmpty(solutionFile))
                    {
                        Settings.Instance.SetCurrentSolutionPath(solutionFile);
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            try
            {
                // 保存当前解决方案的规则
                Settings.Instance.SetCurrentSolutionPath(null);
            }
            catch
            {
                // 忽略错误
            }
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
