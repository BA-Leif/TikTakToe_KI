using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spielerei_TikTakToe_learning_AI.Model;


namespace Spielerei_TikTakToe_learning_AI.Data
{
    class SituationContext : DbContext
    {
        public DbSet<Game_Situation> Situations { get; set; }

        public SituationContext() : base("SituationContext")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
