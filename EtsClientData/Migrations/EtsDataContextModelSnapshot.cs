﻿// <auto-generated />
using System;
using EtsClientData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EtsClientData.Migrations
{
    [DbContext(typeof(EtsDataContext))]
    partial class EtsDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EtsClientData.EtsDataContext+Reminder", b =>
                {
                    b.Property<int>("ReminderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(250)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ReminderId");

                    b.HasIndex("UserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("EtsClientData.EtsDataContext+ReminderTime", b =>
                {
                    b.Property<int>("ReminderTimeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Notified")
                        .IsRequired()
                        .HasColumnType("varchar(10)");

                    b.Property<int>("ReminderId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.HasKey("ReminderTimeId");

                    b.HasIndex("ReminderId");

                    b.ToTable("ReminderTimes");
                });

            modelBuilder.Entity("EtsClientData.EtsDataContext+User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EtsPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("EtsUserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("SlackChanellID")
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("UserType")
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EtsClientData.EtsDataContext+Reminder", b =>
                {
                    b.HasOne("EtsClientData.EtsDataContext+User", null)
                        .WithMany("Reminders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EtsClientData.EtsDataContext+ReminderTime", b =>
                {
                    b.HasOne("EtsClientData.EtsDataContext+Reminder", null)
                        .WithMany("ReminderTimes")
                        .HasForeignKey("ReminderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
