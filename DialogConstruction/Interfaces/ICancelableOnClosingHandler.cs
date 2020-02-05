using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogConstruction.Interfaces
{
    /// <summary>
    /// This interface can be implemented by view models, which want to be notified when
    /// the corresponding window is about to be closed.
    /// </summary>
    public interface ICancelableOnClosingHandler
    {
        /// <summary>
        /// This method is called when the corresponding view's <see cref="Window.Closing"/> event was raised.
        /// </summary>
        /// <returns><see landword="true"/> the the window can be closed; otherwise, <see langword="false"/>.</returns>
        bool OnClosing();
    }
}
