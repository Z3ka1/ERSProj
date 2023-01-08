using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

namespace EFDataBase
{
	public class EFContext : DbContext
	{
		public DbSet<InfoCentralHeater> Infos { get; set; }

        //TODO Proveriti na windowsu gde pravi db (Na macu je u home folderu)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=infosDB.db");
        }

        public static void addInfo(TimeSpan runTime, DateTime startTime, double resourcesSpent)
        {
            using (EFContext context = new EFContext())
            {
                context.Infos.Add(new InfoCentralHeater
                {
                    RunTime = runTime,
                    StartTime = startTime
                    ,
                    ResourcesSpent = resourcesSpent
                });

                context.SaveChanges();
            }
        }
    }
}

