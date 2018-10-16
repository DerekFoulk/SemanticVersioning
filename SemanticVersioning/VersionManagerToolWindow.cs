using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SemanticVersioning.Views;

namespace SemanticVersioning
{
    /// <summary>
    ///     This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    ///     In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    ///     usually implemented by the package implementer.
    ///     <para>
    ///         This class derives from the ToolWindowPane class provided from the MPF in order to use its
    ///         implementation of the IVsUIElementPane interface.
    ///     </para>
    /// </remarks>
    [Guid("f4199493-3695-4a7e-9410-1cb07e49f120")]
    public class VersionManagerToolWindow : ToolWindowPane, IVsRunningDocTableEvents
    {
        private IVsRunningDocumentTable rdt;
        private uint rdtCookie;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VersionManagerToolWindow" /> class.
        /// </summary>
        public VersionManagerToolWindow() : base(null)
        {
            Caption = "Version Manager";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new VersionManagerToolWindowControl(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            rdt = (IVsRunningDocumentTable) GetService(typeof(SVsRunningDocumentTable));
            rdt.AdviseRunningDocTableEvents(this, out rdtCookie);
        }

        protected override void Dispose(bool disposing)
        {
            // Release the RDT cookie.
            rdt.UnadviseRunningDocTableEvents(rdtCookie);

            base.Dispose(disposing);
        }

        public event EventHandler<DocumentSavedEventArgs> DocumentSaved;

        #region IVsRunningDocTableEvents

        // See https://docs.microsoft.com/en-us/visualstudio/extensibility/subscribing-to-an-event

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
            uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
            uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            var doc = rdt.GetDocumentInfo(
                docCookie,
                out var pgrfRDTFlags,
                out var pdwReadLocks,
                out var pdwEditLocks,
                out var pbstrMkDocument,
                out var ppHier,
                out var pitemid,
                out var ppunkDocData
            );

            var documentSavedEventArgs = new DocumentSavedEventArgs
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
    }
}
