namespace SemanticVersioning
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using SemanticVersioning.Views;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("f4199493-3695-4a7e-9410-1cb07e49f120")]
    public class VersionManagerToolWindow : ToolWindowPane, IVsRunningDocTableEvents
    {
        private uint rdtCookie;
        private IVsRunningDocumentTable rdt;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionManagerToolWindow"/> class.
        /// </summary>
        public VersionManagerToolWindow() : base(null)
        {
            this.Caption = "Version Manager";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new VersionManagerToolWindowControl(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            rdt = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));
            rdt.AdviseRunningDocTableEvents(this, out rdtCookie);
        }

        protected override void Dispose(bool disposing)
        {
            // Release the RDT cookie.
            rdt.UnadviseRunningDocTableEvents(rdtCookie);

            base.Dispose(disposing);
        }

        #region IVsRunningDocTableEvents

        // See https://docs.microsoft.com/en-us/visualstudio/extensibility/subscribing-to-an-event

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            var doc = rdt.GetDocumentInfo(
                docCookie,
                out uint pgrfRDTFlags,
                out uint pdwReadLocks,
                out uint pdwEditLocks,
                out string pbstrMkDocument,
                out IVsHierarchy ppHier,
                out uint pitemid,
                out IntPtr ppunkDocData
            );

            DocumentSavedEventArgs documentSavedEventArgs = new DocumentSavedEventArgs
            {
                FullName = pbstrMkDocument
            };

            DocumentSaved?.Invoke(this, documentSavedEventArgs);

            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        #endregion IVsRunningDocTableEvents

        public event EventHandler<DocumentSavedEventArgs> DocumentSaved;
    }
}
