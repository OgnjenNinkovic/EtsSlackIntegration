using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using EtsClientDB;

namespace EtsClientData
{
    public class EtsDataContext : DbContext
    {

        public delegate void EntityDataChanged(object e);

        private static AesCryptoUtil AesCryptoUtil = new AesCryptoUtil();


        public DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<ReminderTime> ReminderTimes { get; set; }



        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);
        }


        //Local database connection string
      // public string ConnectionString { get; private set; } = @"Server=(localdb)\mssqllocaldb;Database=EtsClientData;Integrated Security=True;MultipleActiveResultSets=true";


        //Azure database connection string                     
       public string ConnectionString { get; private set; } = @"Data Source=etsclientservicetestdb.database.windows.net;Initial Catalog=EtsClientData;User ID=dbadmin;Password=VaIfHDseO6iPdM5FqoGO;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);


        }

        [Table("Users")]
        public class User
        {
            [Key]
            public int UserId { get; set; }

            [Column(TypeName = "nvarchar(250)")]
            [Required]
            public string EtsUserName { get; set; }


            private string _etsPassword;

            [Column(TypeName = "nvarchar(250)")]
            [Required]
            public string EtsPassword {

                get
                {
                    return AesCryptoUtil.Decrypt(_etsPassword);
                }
                set
                {
                   this._etsPassword = AesCryptoUtil.Encrypt(value);
                }
            
            }

            [Column(TypeName = "nvarchar(250)")]
            public string SlackChannelID { get; set; }

            [Column(TypeName = "nvarchar(250)")]
            public string UserType { get; set; }

            public List<Reminder> Reminders { get; set; }

            public static event EntityDataChanged userEntityChanged;


            static User()
            {

                Triggers<User>.Inserted += e =>
                {
                 
                    userEntityChanged(e);
                };


                Triggers<User>.Deleted += e =>
                {
                    userEntityChanged(e);
                };

                Triggers<User>.Updated += e =>
                {
                    userEntityChanged(e);
                };


            }

        }



        [Table("Reminders")]

        public class Reminder
        {
            [Key]
            public int ReminderId { get; set; }

            [Column(TypeName = "int")]
            [Required]
            public int UserId { get; set; }

            [Column(TypeName = "nvarchar(250)")]
            public string Type { get; set; }


            [Column(TypeName = "datetime2")]
            public DateTime Date { get; set; }

            public List<ReminderTime> ReminderTimes { get; set; }


            public static event EntityDataChanged reminderEntityChanged;

            static Reminder()
            {
                Triggers<Reminder>.Inserted += e =>
                {
                    reminderEntityChanged(e);
                };

                Triggers<Reminder>.Deleted += e =>
                {
                    reminderEntityChanged(e);
                };


                Triggers<Reminder>.Updated += e =>
                {
                    reminderEntityChanged(e);
                };

            }
        }

        [Table("ReminderTimes")]
        public class ReminderTime
        {
            [Key]
            public int ReminderTimeId { get; set; }

            [Column(TypeName = "int")]
            [Required]
            public int ReminderId { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime Time { get; set; }


            [Column(TypeName = "varchar(10)")]
            [DefaultValue("False")]
            public string Notified { get; set; } = "False";

            public static event EntityDataChanged reminderTimeEntityChanged;

            static ReminderTime()
            {

                Triggers<ReminderTime>.Inserted += e =>
                {
                    reminderTimeEntityChanged(e);

                };
                Triggers<ReminderTime>.Deleted += e =>
                {
                    reminderTimeEntityChanged(e);

                };
                Triggers<ReminderTime>.Updated += e =>
                {
                    reminderTimeEntityChanged(e);

                };

            }


        }




    }

}

