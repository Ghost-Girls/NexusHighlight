using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace BetterCommentsPlus.CommentsViewCustomization
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [ContentType("html")]
    [ContentType("XML")]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class BackgroundAdornerTextViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable 649, 169

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("BackgroundAdorner")]
        [Order(After = PredefinedAdornmentLayers.Caret, Before = PredefinedAdornmentLayers.Selection)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        private AdornmentLayerDefinition editorAdornmentLayer;

#pragma warning restore 649, 169

        #region IWpfTextViewCreationListener

        public void TextViewCreated(IWpfTextView textView)
        {
            new BackgroundAdorner(textView);
        }

        #endregion
    }
}
