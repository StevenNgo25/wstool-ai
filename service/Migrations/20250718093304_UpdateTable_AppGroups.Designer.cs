﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WhatsAppCampaignManager.Data;

#nullable disable

namespace WhatsAppCampaignManager.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250718093304_UpdateTable_AppGroups")]
    partial class UpdateTable_AppGroups
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppAnalys", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastSeenAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("UserPhoneNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId", "UserPhoneNumber")
                        .IsUnique();

                    b.ToTable("AppAnalytics");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("GroupId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("InstanceId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastSyncAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("ParticipantCount")
                        .HasColumnType("int");

                    b.Property<string>("Participants")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId")
                        .IsUnique();

                    b.HasIndex("InstanceId");

                    b.ToTable("AppGroups");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppInstance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Status")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WhapiToken")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("WhapiUrl")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("WhatsAppNumber")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("AppInstances");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppJob", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<int>("InstanceId")
                        .HasColumnType("int");

                    b.Property<string>("JobType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("MessageId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ScheduledAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TargetData")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedByUserId");

                    b.HasIndex("InstanceId");

                    b.HasIndex("MessageId");

                    b.ToTable("AppJobs");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppJobLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Details")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<int>("JobId")
                        .HasColumnType("int");

                    b.Property<string>("LogLevel")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("AppJobLogs");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("InstanceId")
                        .HasColumnType("int");

                    b.Property<string>("TextContent")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CreatedByUserId");

                    b.HasIndex("InstanceId");

                    b.ToTable("AppMessages");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppMessageGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AssignedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("MessageId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("MessageId");

                    b.ToTable("AppMessageGroups");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppSentMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DeliveredAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("JobId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastValidatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RecipientId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("RecipientName")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("RecipientType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("WhapiMessageId")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("AppSentMessages");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("AppUsers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2025, 7, 18, 16, 33, 4, 58, DateTimeKind.Local).AddTicks(7177),
                            Email = "admin@whatsappcampaign.com",
                            IsActive = true,
                            PasswordHash = "$2a$11$YPpGg4hr7/pUYQO24mearOcWexXNDG.4kqFGCn4IT5Bo5OKw1ejoG",
                            Role = "Admin",
                            Username = "admin"
                        },
                        new
                        {
                            Id = 2,
                            CreatedAt = new DateTime(2025, 7, 18, 16, 33, 4, 180, DateTimeKind.Local).AddTicks(3006),
                            Email = "user1@whatsappcampaign.com",
                            IsActive = true,
                            PasswordHash = "$2a$11$KTlEehOGMGfF6Iz2uRDHw.6EX3mlGd0.9xTebVPWB5f4AlqYAKz56",
                            Role = "Member",
                            Username = "user1"
                        });
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppUserInstance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AssignedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("CanCreateJobs")
                        .HasColumnType("bit");

                    b.Property<bool>("CanSendMessages")
                        .HasColumnType("bit");

                    b.Property<int>("InstanceId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.HasIndex("UserId");

                    b.ToTable("AppUserInstances");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppAnalys", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppGroup", "Group")
                        .WithMany("Analytics")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppGroup", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppInstance", "Instance")
                        .WithMany("Groups")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Instance");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppJob", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppUser", "CreatedByUser")
                        .WithMany("Jobs")
                        .HasForeignKey("CreatedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WhatsAppCampaignManager.Models.AppInstance", "Instance")
                        .WithMany("Jobs")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WhatsAppCampaignManager.Models.AppMessage", "Message")
                        .WithMany("Jobs")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CreatedByUser");

                    b.Navigation("Instance");

                    b.Navigation("Message");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppJobLog", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppJob", "Job")
                        .WithMany("JobLogs")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppMessage", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppUser", "CreatedByUser")
                        .WithMany("Messages")
                        .HasForeignKey("CreatedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WhatsAppCampaignManager.Models.AppInstance", "Instance")
                        .WithMany()
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("CreatedByUser");

                    b.Navigation("Instance");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppMessageGroup", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppGroup", "Group")
                        .WithMany("MessageGroups")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatsAppCampaignManager.Models.AppMessage", "Message")
                        .WithMany("MessageGroups")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Message");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppSentMessage", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppJob", "Job")
                        .WithMany("SentMessages")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppUserInstance", b =>
                {
                    b.HasOne("WhatsAppCampaignManager.Models.AppInstance", "Instance")
                        .WithMany("UserInstances")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatsAppCampaignManager.Models.AppUser", "User")
                        .WithMany("UserInstances")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instance");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppGroup", b =>
                {
                    b.Navigation("Analytics");

                    b.Navigation("MessageGroups");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppInstance", b =>
                {
                    b.Navigation("Groups");

                    b.Navigation("Jobs");

                    b.Navigation("UserInstances");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppJob", b =>
                {
                    b.Navigation("JobLogs");

                    b.Navigation("SentMessages");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppMessage", b =>
                {
                    b.Navigation("Jobs");

                    b.Navigation("MessageGroups");
                });

            modelBuilder.Entity("WhatsAppCampaignManager.Models.AppUser", b =>
                {
                    b.Navigation("Jobs");

                    b.Navigation("Messages");

                    b.Navigation("UserInstances");
                });
#pragma warning restore 612, 618
        }
    }
}
