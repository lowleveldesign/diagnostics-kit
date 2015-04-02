
create table LogRecords (
     Id bigint auto_increment primary key
    ,LoggerName varchar(100) not null
    ,LogLevel smallint not null
    ,TimeUTC datetime not null
    ,Message varchar(5000) null
    ,ExceptionType varchar(100) null
    ,ExceptionMessage varchar(5000) null
    ,ExceptionAdditionalInfo varchar(4000) null
    ,CorrelationId varchar(100) null
    ,Server varchar(200) null
    ,ApplicationPath varchar(2000) null
    ,ProcessId int null
    ,ThreadId int null
    ,[Identity] varchar(200) null
    ,Host varchar(100) null
    ,LoggedUser varchar(200) null
    ,HttpStatusCode varchar(15) null
    ,Url varchar(2000) null
    ,Referer varchar(2000) null
    ,ClientIP varchar(100) null -- FIXME max IPv6 length
    ,RequestData varchar(2000) null
    ,ResponseData varchar(2000) null
    ,ServiceName varchar(100) null
    ,ServiceDisplayName varchar(200) null
);
go

-- FIXME partitions

create table LogRecordPerformanceData (
     LogRecordId bigint not null
    ,CounterName varchar(100) not null
    ,Value float not null
    ,constraint PK_LogRecordPerformanceData primary key (LogRecordId, CounterName)
);
go

-- FIXME partitions
