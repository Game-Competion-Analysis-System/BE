using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Swd392GameAiContext : DbContext
{
    public Swd392GameAiContext()
    {
    }

    public Swd392GameAiContext(DbContextOptions<Swd392GameAiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aianalysis> Aianalyses { get; set; }

    public virtual DbSet<Aiextractedfield> Aiextractedfields { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Guild> Guilds { get; set; }

    public virtual DbSet<Imageupload> Imageuploads { get; set; }

    public virtual DbSet<Leaderboard> Leaderboards { get; set; }

    public virtual DbSet<Leaderboardentry> Leaderboardentries { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<User> Users { get; set; }
    private static string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn ?? "";
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString());
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aianalysis>(entity =>
        {
            entity.HasKey(e => e.Analysisid).HasName("aianalysis_pkey");

            entity.ToTable("aianalysis");

            entity.Property(e => e.Analysisid).HasColumnName("analysisid");
            entity.Property(e => e.Aimodelversion)
                .HasMaxLength(100)
                .HasColumnName("aimodelversion");
            entity.Property(e => e.Confidencescore).HasColumnName("confidencescore");
            entity.Property(e => e.Processedtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processedtime");
            entity.Property(e => e.Uploadid).HasColumnName("uploadid");

            entity.HasOne(d => d.Upload).WithMany(p => p.Aianalyses)
                .HasForeignKey(d => d.Uploadid)
                .HasConstraintName("fk_analysis_upload");
        });

        modelBuilder.Entity<Aiextractedfield>(entity =>
        {
            entity.HasKey(e => e.Fieldid).HasName("aiextractedfield_pkey");

            entity.ToTable("aiextractedfield");

            entity.Property(e => e.Fieldid).HasColumnName("fieldid");
            entity.Property(e => e.Analysisid).HasColumnName("analysisid");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
            entity.Property(e => e.Fieldtype)
                .HasMaxLength(100)
                .HasColumnName("fieldtype");
            entity.Property(e => e.Rawtext)
                .HasMaxLength(500)
                .HasColumnName("rawtext");

            entity.HasOne(d => d.Analysis).WithMany(p => p.Aiextractedfields)
                .HasForeignKey(d => d.Analysisid)
                .HasConstraintName("fk_field_analysis");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Companyid).HasName("company_pkey");

            entity.ToTable("company");

            entity.Property(e => e.Companyid).HasColumnName("companyid");
            entity.Property(e => e.Companyname)
                .HasMaxLength(255)
                .HasColumnName("companyname");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .HasColumnName("website");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Eventid).HasName("event_pkey");

            entity.ToTable("event");

            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Enddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("enddate");
            entity.Property(e => e.Eventname)
                .HasMaxLength(255)
                .HasColumnName("eventname");
            entity.Property(e => e.Eventtype)
                .HasMaxLength(100)
                .HasColumnName("eventtype");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Startdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("startdate");

            entity.HasOne(d => d.Game).WithMany(p => p.Events)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("fk_event_game");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Gameid).HasName("game_pkey");

            entity.ToTable("game");

            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Companyid).HasColumnName("companyid");
            entity.Property(e => e.Gamename)
                .HasMaxLength(255)
                .HasColumnName("gamename");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .HasColumnName("genre");

            entity.HasOne(d => d.Company).WithMany(p => p.Games)
                .HasForeignKey(d => d.Companyid)
                .HasConstraintName("fk_game_company");
        });

        modelBuilder.Entity<Guild>(entity =>
        {
            entity.HasKey(e => e.Guildid).HasName("guild_pkey");

            entity.ToTable("guild");

            entity.Property(e => e.Guildid).HasColumnName("guildid");
            entity.Property(e => e.Guildname)
                .HasMaxLength(255)
                .HasColumnName("guildname");
            entity.Property(e => e.Leaderplayerid).HasColumnName("leaderplayerid");
            entity.Property(e => e.Serverid).HasColumnName("serverid");

            entity.HasOne(d => d.Server).WithMany(p => p.Guilds)
                .HasForeignKey(d => d.Serverid)
                .HasConstraintName("fk_guild_server");
        });

        modelBuilder.Entity<Imageupload>(entity =>
        {
            entity.HasKey(e => e.Uploadid).HasName("imageupload_pkey");

            entity.ToTable("imageupload");

            entity.Property(e => e.Uploadid).HasColumnName("uploadid");
            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Imageurl)
                .HasMaxLength(500)
                .HasColumnName("imageurl");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Uploadtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadtime");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Event).WithMany(p => p.Imageuploads)
                .HasForeignKey(d => d.Eventid)
                .HasConstraintName("fk_upload_event");

            entity.HasOne(d => d.User).WithMany(p => p.Imageuploads)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_upload_user");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.Leaderboardid).HasName("leaderboard_pkey");

            entity.ToTable("leaderboard");

            entity.Property(e => e.Leaderboardid).HasColumnName("leaderboardid");
            entity.Property(e => e.Createdfromanalysisid).HasColumnName("createdfromanalysisid");
            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Metrictype)
                .HasMaxLength(100)
                .HasColumnName("metrictype");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Createdfromanalysis).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.Createdfromanalysisid)
                .HasConstraintName("fk_leaderboard_analysis");

            entity.HasOne(d => d.Event).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.Eventid)
                .HasConstraintName("fk_leaderboard_event");
        });

        modelBuilder.Entity<Leaderboardentry>(entity =>
        {
            entity.HasKey(e => e.Entryid).HasName("leaderboardentry_pkey");

            entity.ToTable("leaderboardentry");

            entity.Property(e => e.Entryid).HasColumnName("entryid");
            entity.Property(e => e.Leaderboardid).HasColumnName("leaderboardid");
            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Leaderboard).WithMany(p => p.Leaderboardentries)
                .HasForeignKey(d => d.Leaderboardid)
                .HasConstraintName("fk_entry_leaderboard");

            entity.HasOne(d => d.Player).WithMany(p => p.Leaderboardentries)
                .HasForeignKey(d => d.Playerid)
                .HasConstraintName("fk_entry_player");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Playerid).HasName("player_pkey");

            entity.ToTable("player");

            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Guildid).HasColumnName("guildid");
            entity.Property(e => e.Playername)
                .HasMaxLength(255)
                .HasColumnName("playername");
            entity.Property(e => e.Serverid).HasColumnName("serverid");

            entity.HasOne(d => d.Game).WithMany(p => p.Players)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("fk_player_game");

            entity.HasOne(d => d.Guild).WithMany(p => p.Players)
                .HasForeignKey(d => d.Guildid)
                .HasConstraintName("fk_player_guild");

            entity.HasOne(d => d.Server).WithMany(p => p.Players)
                .HasForeignKey(d => d.Serverid)
                .HasConstraintName("fk_player_server");
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Serverid).HasName("server_pkey");

            entity.ToTable("server");

            entity.Property(e => e.Serverid).HasColumnName("serverid");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .HasColumnName("region");
            entity.Property(e => e.Servername)
                .HasMaxLength(255)
                .HasColumnName("servername");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Game).WithMany(p => p.Servers)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("fk_server_game");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "User_username_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
