using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public partial class BusyIndicatorDialog : Dialog
    {
        public BusyIndicatorDialog()
            : base(null, null)
        {
            InitializeWidget();
        }
    }
}
