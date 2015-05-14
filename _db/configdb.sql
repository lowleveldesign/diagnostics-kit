create table Applications (
    PathHash binary(16) primary key,
    Path nvarchar(2000) not null,
    Name nvarchar(500) not null,
    IsExcluded bit not null
);
