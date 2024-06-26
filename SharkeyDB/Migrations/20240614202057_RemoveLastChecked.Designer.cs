﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharkeyDB;

#nullable disable

namespace SharkeyDB.Migrations
{
    [DbContext(typeof(SharkeyContext))]
    [Migration("20240614202057_RemoveLastChecked")]
    partial class RemoveLastChecked
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SharkeyDB.Entities.MSUser", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("user_id");

                    b.Property<DateTime?>("CheckedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("checked_at");

                    b.Property<bool>("IsFlagged")
                        .HasColumnType("boolean")
                        .HasColumnName("is_flagged");

                    b.HasKey("UserId");

                    b.ToTable("ms_user");
                });

            modelBuilder.Entity("SharkeyDB.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("id");

                    b.Property<string>("Host")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("host");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("isDeleted");

                    b.Property<bool>("IsSuspended")
                        .HasColumnType("boolean")
                        .HasColumnName("isSuspended");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("username");

                    b.Property<string>("UsernameLower")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("usernameLower");

                    b.HasKey("Id");

                    b.ToTable("user", null, t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
