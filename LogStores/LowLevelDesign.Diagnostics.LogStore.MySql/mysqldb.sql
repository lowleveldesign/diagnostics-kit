
create table applog_LogRecords (
     Id bigint auto_increment
    ,LoggerName varchar(100) not null
    ,LogLevel smallint not null
    ,TimeUTC datetime not null
    ,Message varchar(5000) null
    ,ExceptionType varchar(100) null
    ,ExceptionMessage varchar(2000) null
    ,ExceptionAdditionalInfo varchar(2000) null
    ,CorrelationId varchar(100) null
    ,Server varchar(200) null
    ,ApplicationPath varchar(2000) null
    ,ProcessId int null
    ,ThreadId int null
    ,Identity varchar(200) null
    ,Host varchar(100) null
    ,LoggedUser varchar(200) null
    ,HttpStatusCode varchar(15) character set ascii null
    ,Url varchar(2000) null
    ,Referer varchar(2000) null
    ,ClientIP varchar(50) character set ascii null -- FIXME max IPv6 length
    ,RequestData varchar(2000) null
    ,ResponseData varchar(2000) null
    ,ServiceName varchar(100) null
    ,ServiceDisplayName varchar(200) null
    ,PRIMARY KEY (Id, TimeUTC)
)
COLLATE='utf8_general_ci'
PARTITION BY RANGE  COLUMNS(TimeUTC)
(PARTITION p20150413 VALUES LESS THAN ('2015-04-14 00:00') ENGINE = InnoDB,
 PARTITION p20150414 VALUES LESS THAN ('2015-04-15 00:00') ENGINE = InnoDB,
 PARTITION p20150415 VALUES LESS THAN ('2015-04-16 00:00') ENGINE = InnoDB);

-- FIXME partitions

create table perflog_all (
    TimeUTC
    ,ApplicationPath varchar(2000) not null
    ,CounterName varchar(100) not null
    ,LogRecordId bigint null
    ,Value float not null
    ,constraint PK_LogRecordPerformanceData primary key (TimeUTC, ApplicationPath, CounterName)
);
go

-- FIXME partitions
