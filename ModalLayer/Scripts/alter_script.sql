alter table employee_leave_request
add column TotalLeaveQuota decimal default 0;

alter table employee_leave_request
drop column AvailableLeaveQuota;

alter table employee_leave_request
add column LeaveQuotaStatus json;


alter table employee_leave_request
drop column LeaveQuotaStatus;

alter table employee_leave_request
add column LeaveQuotaDetail json;

drop table if exists email_setting_detail;

create table email_setting_detail(
	EmailSettingDetailId int primary key auto_increment,
	CompanyId int,
    EmailAddress varchar(200),
    EmailName varchar(100),
    EmailHost varchar(100),
    PortNo int,
    EnableSsl bit,
    DeliveryMothod varchar(50),
    UserDefaultCredentials bit,
    Credentials varchar(100),
    IsPrimary bit,
    UpdatedBy long,
    UpdatedOn Datetime not null
);

select * from company;
select * from email_setting_detail;
insert into email_setting_detail values(default, 1, 'info@bottomhalf.in', 'BOTTOMHALF', 'smtpout.asia.secureserver.net', 587, true, 0, false, 'bottomhalf@mi9', true, 22, now());