alter table employee_leave_request
add column TotalLeaveQuota decimal default 0;

alter table employee_leave_request
drop column AvailableLeaveQuota;

alter table employee_leave_request
add column  json;


alter table employee_leave_request
drop column LeaveQuotaStatus;

alter table employee_leave_request
add column LeaveQuotaDetail json;