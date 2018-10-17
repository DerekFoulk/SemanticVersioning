using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SemanticVersioning.Views;

namespace SemanticVersioning
{
    /// <inheritdoc cref="ToolWindowPane" />
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
    public sealed class VersionManagerToolWindow : ToolWindowPane, IVsRunningDocTableEvents
    {
        private IVsRunningDocumentTable _rdt;
        private uint _rdtCookie;

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
            ThreadHelper.ThrowIfNotOnUIThread();

            base.Initialize();

            _rdt = (IVsRunningDocumentTable) GetService(typeof(SVsRunningDocumentTable));
            _rdt.AdviseRunningDocTableEvents(this, out _rdtCookie);
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // Release the RDT cookie.
            _rdt.UnadviseRunningDocTableEvents(_rdtCookie);

            base.Dispose(disposing);
        }

        public event EventHandler<DocumentSavedEventArgs> DocumentSaved;

        #region IVsRunningDocTableEvents

        // See https://docs.microsoft.com/en-us/visualstudio/extensibility/subscribing-to-an-event

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRdtLockType, uint dwReadLocksRemaining,
            uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRdtLockType, uint dwReadLocksRemaining,
            uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _rdt.GetDocumentInfo(
                docCookie,
                out _,
                out _,
                out _,
                out var pbstrMkDocument,
                out _,
                out _,
                out _
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
