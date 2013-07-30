using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public partial class ErrorDialog : Dialog
    {
        public ErrorDialog()
            : base(null, null)
        {
            InitializeWidget();
        }
    }
}
