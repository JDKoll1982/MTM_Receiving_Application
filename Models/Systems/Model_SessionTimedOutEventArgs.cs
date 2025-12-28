using System;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Models.Systems
{
    public class Model_SessionTimedOutEventArgs : EventArgs
    {
        public Model_UserSession Session { get; set; } = null!;
        public TimeSpan IdleDuration { get; set; }
    }
}
