using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveHolidaysAndWeekoff
    {
         public int LeaveHolidaysAndWeekOffId { get; set; }
         public int LeavePlanId { get; set; }
         public bool AdJoiningHolidayIsConsiderAsLeave { get; set; }
         public bool IfLeaveLieBetweenTwoHolidays { get; set; }
         public bool IfHolidayIsRightBeforLeave { get; set; }
         public bool IfHolidayIsRightAfterLeave { get; set; }
         public bool IfHolidayIsBetweenLeave { get; set; }
         public bool IfHolidayIsRightBeforeAfterOrInBetween { get; set; }
         public bool AdjoiningHolidayRulesIsValidForHalfDay { get; set; }
         public bool AdjoiningWeekOffIsConsiderAsLeave { get; set; }
         public int ConsiderLeaveIfIncludeDays { get; set; }
         public bool IfLeaveLieBetweenWeekOff { get; set; }
         public bool IfWeekOffIsRightBeforLeave { get; set; }
         public bool IfWeekOffIsRightAfterLeave { get; set; }
         public bool IfWeekOffIsBetweenLeave { get; set; }
         public bool IfWeekOffIsRightBeforeAfterOrInBetween { get; set; }
         public bool AdjoiningWeekOffRulesIsValidForHalfDay { get; set; }
         public bool ClubSandwichPolicy { get; set; }
    }
}
