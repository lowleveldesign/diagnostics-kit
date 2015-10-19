using Dapper;
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using MySql.Data.MySqlClient;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public sealed class MySqlAppUserManager : DefaultAppUserManager
    {
        public MySqlAppUserManager()
            : base(MySqlLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("create table if not exists Users (Id varchar(32) not null primary key," +
                        "UserName varchar(100) not null unique, PasswordHash varchar(1000), " +
                        "RegistrationDateUtc datetime not null)");

                conn.Execute("create table if not exists UserClaims (UserId varchar(32) not null references Users(Id), ClaimType varchar(250) not null, " +
                    "ClaimValue varchar(1000) not null, primary key(UserId, ClaimType))");
            }
        }

        protected override System.Data.Common.DbConnection CreateConnection()
        {
            return new MySqlConnection(dbConnString);
        }
    }
}
