﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinimalApi_Test.Context;

#nullable disable

namespace MinimalApi_Test.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MinimalApi_Test.Entities.User.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("IsDelete")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2024, 12, 22, 6, 37, 23, 974, DateTimeKind.Utc).AddTicks(8895),
                            FirstName = "مدیر",
                            IsDelete = false,
                            LastName = "سیستم",
                            PasswordHash = "AQAAAAIAAYagAAAAELmh3PhOllrvnOiw7X5kGt+KhGnUi9w4WfLbWnUKJh1Xb50dog5SLeBdMLk9/B1ZNA==",
                            Role = "Admin",
                            Username = "admin@localhost.com"
                        },
                        new
                        {
                            Id = 2,
                            CreatedAt = new DateTime(2024, 12, 22, 6, 37, 24, 21, DateTimeKind.Utc).AddTicks(9402),
                            FirstName = "کاربر",
                            IsDelete = false,
                            LastName = "عادی",
                            PasswordHash = "AQAAAAIAAYagAAAAEFfVc18ehBkM8Nfi9fgqd2eKFjsRDcVx+YS7uq4QApk40SntOMDWi3n/FiPeQwzE7Q==",
                            Role = "User",
                            Username = "user@localhost.com"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
