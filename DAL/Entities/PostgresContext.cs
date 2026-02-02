using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aianalysis> Aianalyses { get; set; }

    public virtual DbSet<Aiextractedfield> Aiextractedfields { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Imageupload> Imageuploads { get; set; }

    public virtual DbSet<Leaderboard> Leaderboards { get; set; }

    public virtual DbSet<Leaderboardentry> Leaderboardentries { get; set; }

    public virtual DbSet<Player> Players { get; set; }
    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn;
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

            entity.HasIndex(e => e.Uploadid, "idx_analysis_upload");

            entity.Property(e => e.Analysisid).HasColumnName("analysisid");
            entity.Property(e => e.Aimodelversion)
                .HasMaxLength(100)
                .HasColumnName("aimodelversion");
            entity.Property(e => e.Confidencescore).HasColumnName("confidencescore");
            entity.Property(e => e.Processedtime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processedtime");
            entity.Property(e => e.Uploadid).HasColumnName("uploadid");

            entity.HasOne(d => d.Upload).WithMany(p => p.Aianalyses)
                .HasForeignKey(d => d.Uploadid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("aianalysis_uploadid_fkey");
        });

        modelBuilder.Entity<Aiextractedfield>(entity =>
        {
            entity.HasKey(e => e.Fieldid).HasName("aiextractedfield_pkey");

            entity.ToTable("aiextractedfield");

            entity.HasIndex(e => e.Analysisid, "idx_field_analysis");

            entity.Property(e => e.Fieldid).HasColumnName("fieldid");
            entity.Property(e => e.Analysisid).HasColumnName("analysisid");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
            entity.Property(e => e.Fieldtype)
                .HasMaxLength(100)
                .HasColumnName("fieldtype");
            entity.Property(e => e.Rawtext).HasColumnName("rawtext");

            entity.HasOne(d => d.Analysis).WithMany(p => p.Aiextractedfields)
                .HasForeignKey(d => d.Analysisid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("aiextractedfield_analysisid_fkey");
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

            entity.HasIndex(e => e.Gameid, "idx_event_game");

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
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("event_gameid_fkey");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Gameid).HasName("game_pkey");

            entity.ToTable("game");

            entity.HasIndex(e => e.Companyid, "idx_game_company");

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
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("game_companyid_fkey");
        });

        modelBuilder.Entity<Imageupload>(entity =>
        {
            entity.HasKey(e => e.Uploadid).HasName("imageupload_pkey");

            entity.ToTable("imageupload");

            entity.HasIndex(e => e.Eventid, "idx_upload_event");

            entity.HasIndex(e => e.Playerid, "idx_upload_player");

            entity.Property(e => e.Uploadid).HasColumnName("uploadid");
            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Imageurl).HasColumnName("imageurl");
            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Uploadtime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadtime");

            entity.HasOne(d => d.Event).WithMany(p => p.Imageuploads)
                .HasForeignKey(d => d.Eventid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("imageupload_eventid_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.Imageuploads)
                .HasForeignKey(d => d.Playerid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("imageupload_playerid_fkey");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.Leaderboardid).HasName("leaderboard_pkey");

            entity.ToTable("leaderboard");

            entity.HasIndex(e => e.Eventid, "idx_leaderboard_event");

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
                .HasConstraintName("leaderboard_createdfromanalysisid_fkey");

            entity.HasOne(d => d.Event).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.Eventid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboard_eventid_fkey");
        });

        modelBuilder.Entity<Leaderboardentry>(entity =>
        {
            entity.HasKey(e => e.Entryid).HasName("leaderboardentry_pkey");

            entity.ToTable("leaderboardentry");

            entity.HasIndex(e => e.Leaderboardid, "idx_entry_leaderboard");

            entity.HasIndex(e => e.Playerid, "idx_entry_player");

            entity.Property(e => e.Entryid).HasColumnName("entryid");
            entity.Property(e => e.Leaderboardid).HasColumnName("leaderboardid");
            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Leaderboard).WithMany(p => p.Leaderboardentries)
                .HasForeignKey(d => d.Leaderboardid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboardentry_leaderboardid_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.Leaderboardentries)
                .HasForeignKey(d => d.Playerid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboardentry_playerid_fkey");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Playerid).HasName("player_pkey");

            entity.ToTable("player");

            entity.HasIndex(e => e.Gameid, "idx_player_game");

            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Playername)
                .HasMaxLength(100)
                .HasColumnName("playername");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Player'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.Server)
                .HasMaxLength(100)
                .HasColumnName("server");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Active'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Game).WithMany(p => p.Players)
                .HasForeignKey(d => d.Gameid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("player_gameid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
