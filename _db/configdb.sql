
create table Applications (
    Path nvarchar(2000) primary key,
    Name nvarchar(500) null,
    IsExcluded bit not null default(0));

