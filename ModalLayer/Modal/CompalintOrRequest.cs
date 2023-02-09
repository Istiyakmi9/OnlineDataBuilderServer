﻿using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class CompalintOrRequest
    {
        public int ComplaintOrRequestId { set; get; }
        public int RequestTypeId { set; get; }
        public int RequestedId { set; get; }
        public long EmployeeId { set; get; }
        public string EmployeeName { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public long ManagerId { set; get; }
        public string ManagerName { set; get; }
        public string ManagerEmail { set; get; }
        public string ManagerMobile { set; get; }
        public string EmployeeMessage { set; get; }
        public string ManagerComments { set; get; }
        public int CurrentStatus { set; get; }
        public List<Notify> NotifyList { set; get; } = new List<Notify>();
        public string Notify { set; get; }
        public DateTime RequestedOn { set; get; }
        public DateTime AttendanceDate { set; get; }
        public DateTime? LeaveFromDate { set; get; }
        public DateTime? LeaveToDate { set; get; }
        public DateTime UpdatedOn { set; get; }
    }

    public class Notify
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
