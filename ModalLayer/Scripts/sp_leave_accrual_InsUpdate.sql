DELIMITER $$

drop procedure if exists sp_leave_accrual_InsUpdate $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_leave_accrual_InsUpdate`(	
	_LeaveAccrualId int,
    _LeavePlanTypeId int,
    _CanApplyEntireLeave bit,
    _IsLeaveAccruedPatternAvail bit,
	_JoiningMonthLeaveDistribution json,
    _ExitMonthLeaveDistribution json,
    _LeaveDistributionSequence varchar(45),
    _LeaveDistributionAppliedFrom decimal,
    _IsLeavesProratedForJoinigMonth bit,
    _IsLeavesProratedOnNotice bit,
    _IsNotAllowProratedOnNotice bit,
    _IsNoLeaveOnNoticePeriod bit,
    _IsVaryOnProbationOrExprience bit,
	_IsAccrualStartsAfterJoining bit,
	_IsAccrualStartsAfterProbationEnds bit,
	_AccrualDaysAfterJoining decimal,
	_AccrualDaysAfterProbationEnds decimal,
    _AccrualProrateDetail json,
    _IsImpactedOnWorkDaysEveryMonth bit,
    _WeekOffAsAbsentIfAttendaceLessThen decimal,
    _HolidayAsAbsentIfAttendaceLessThen decimal,    
    _CanApplyForFutureDate bit,
	_IsExtraLeaveBeyondAccruedBalance bit,
    _IsNoExtraLeaveBeyondAccruedBalance bit,
    _NoOfDaysForExtraLeave decimal,
	_IsAccrueIfHavingLeaveBalance bit,
    _AllowOnlyIfAccrueBalanceIsAlleast decimal,
    _IsAccrueIfOnOtherLeave bit,
    _NotAllowIfAlreadyOnLeaveMoreThan decimal,    
    _RoundOffLeaveBalance bit,
    _ToNearestHalfDay bit,
    _ToNearestFullDay bit,
    _ToNextAvailableHalfDay bit,
    _ToNextAvailableFullDay bit,
    _ToPreviousHalfDay bit,    
    _DoesLeaveExpireAfterSomeTime bit,
    _AfterHowManyDays decimal,
    out _ProcessingResult varchar(100)
)
Begin
	Begin
		Declare Exit handler for sqlexception
        Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
										@errorno = MYSQL_ERRNO,
										@errortext = MESSAGE_TEXT;
                                        
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
            Call sp_LogException (@Message, '', 'sp_leave_accrual_InsUpdate', 1, 0, @Result);
		end;


		if not exists (select 1 from leave_accrual where LeaveAccrualId = _LeaveAccrualId ) then
		Begin
			set @leaveTypeId = 0;
			select LeaveAccrualId from leave_accrual order by LeaveAccrualId Desc limit 1 into @leaveTypeId;
			set @leaveTypeId = @leaveTypeId + 1;
			
			Insert into leave_accrual values(
					@leaveTypeId,
					_LeavePlanTypeId,
					_CanApplyEntireLeave,
					_IsLeaveAccruedPatternAvail,
					_JoiningMonthLeaveDistribution,
					_ExitMonthLeaveDistribution,
					_LeaveDistributionSequence,
					_LeaveDistributionAppliedFrom,
					_IsLeavesProratedForJoinigMonth,
					_IsLeavesProratedOnNotice,
					_IsNotAllowProratedOnNotice,
					_IsNoLeaveOnNoticePeriod,
					_IsVaryOnProbationOrExprience,
                    _IsAccrualStartsAfterJoining,
					_IsAccrualStartsAfterProbationEnds,
					_AccrualDaysAfterJoining,
					_AccrualDaysAfterProbationEnds,
					_AccrualProrateDetail,
					_IsImpactedOnWorkDaysEveryMonth,
					_WeekOffAsAbsentIfAttendaceLessThen,
					_HolidayAsAbsentIfAttendaceLessThen,    
					_CanApplyForFutureDate,
					_IsExtraLeaveBeyondAccruedBalance,
					_IsNoExtraLeaveBeyondAccruedBalance,
					_NoOfDaysForExtraLeave,
					_IsAccrueIfHavingLeaveBalance,
					_AllowOnlyIfAccrueBalanceIsAlleast,
					_IsAccrueIfOnOtherLeave,
					_NotAllowIfAlreadyOnLeaveMoreThan,    
					_RoundOffLeaveBalance,
					_ToNearestHalfDay,
					_ToNearestFullDay,
					_ToNextAvailableHalfDay,
					_ToNextAvailableFullDay,
					_ToPreviousHalfDay,    
					_DoesLeaveExpireAfterSomeTime,
					_AfterHowManyDays
				);
				
				set _ProcessingResult = @leaveTypeId;
		end;
		else
		Begin
			update leave_accrual set
							LeavePlanTypeId						=		_LeavePlanTypeId,
							CanApplyEntireLeave					=		_CanApplyEntireLeave,
							IsLeaveAccruedPatternAvail			=		_IsLeaveAccruedPatternAvail,
							JoiningMonthLeaveDistribution		=		_JoiningMonthLeaveDistribution,
							ExitMonthLeaveDistribution			=		_ExitMonthLeaveDistribution,
							LeaveDistributionSequence			=		_LeaveDistributionSequence,
							LeaveDistributionAppliedFrom		=		_LeaveDistributionAppliedFrom,
							IsLeavesProratedForJoinigMonth		=		_IsLeavesProratedForJoinigMonth,
							IsLeavesProratedOnNotice			=		_IsLeavesProratedOnNotice,
							IsNotAllowProratedOnNotice			=		_IsNotAllowProratedOnNotice,
							IsNoLeaveOnNoticePeriod				=		_IsNoLeaveOnNoticePeriod,
							IsVaryOnProbationOrExprience		=		_IsVaryOnProbationOrExprience,
							IsAccrualStartsAfterJoining			=		_IsAccrualStartsAfterJoining,
							IsAccrualStartsAfterProbationEnds	=		_IsAccrualStartsAfterProbationEnds,
							AccrualDaysAfterJoining				=		_AccrualDaysAfterJoining,
							AccrualDaysAfterProbationEnds		=		_AccrualDaysAfterProbationEnds,
							AccrualProrateDetail				=		_AccrualProrateDetail,
							IsImpactedOnWorkDaysEveryMonth		=		_IsImpactedOnWorkDaysEveryMonth,
							WeekOffAsAbsentIfAttendaceLessThen	=		_WeekOffAsAbsentIfAttendaceLessThen,
							HolidayAsAbsentIfAttendaceLessThen	=		_HolidayAsAbsentIfAttendaceLessThen,    
							CanApplyForFutureDate				=		_CanApplyForFutureDate,
							IsExtraLeaveBeyondAccruedBalance	=		_IsExtraLeaveBeyondAccruedBalance,
							IsNoExtraLeaveBeyondAccruedBalance	=		_IsNoExtraLeaveBeyondAccruedBalance,
							NoOfDaysForExtraLeave				=		_NoOfDaysForExtraLeave,
							IsAccrueIfHavingLeaveBalance		=		_IsAccrueIfHavingLeaveBalance,
							AllowOnlyIfAccrueBalanceIsAlleast	=		_AllowOnlyIfAccrueBalanceIsAlleast,
							IsAccrueIfOnOtherLeave				=		_IsAccrueIfOnOtherLeave,
							NotAllowIfAlreadyOnLeaveMoreThan	=		_NotAllowIfAlreadyOnLeaveMoreThan,    
							RoundOffLeaveBalance				=		_RoundOffLeaveBalance,
							ToNearestHalfDay					=		_ToNearestHalfDay,
							ToNearestFullDay					=		_ToNearestFullDay,
							ToNextAvailableHalfDay				=		_ToNextAvailableHalfDay,
							ToNextAvailableFullDay				=		_ToNextAvailableFullDay,
							ToPreviousHalfDay					=		_ToPreviousHalfDay,    
							DoesLeaveExpireAfterSomeTime		=		_DoesLeaveExpireAfterSomeTime,
							AfterHowManyDays					=		_AfterHowManyDays
					where 	LeaveAccrualId						=		_LeaveAccrualId;
								
				set _ProcessingResult = _LeaveAccrualId;	
		end;
		end if;
    end;
end$$
DELIMITER ;