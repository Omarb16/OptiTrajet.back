﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OptiTrajet.Persistence;

#nullable disable

namespace OptiTrajet.Migrations
{
    [DbContext(typeof(OptiTrajetContext))]
    partial class OptiTrajetContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("OptiTrajet.Domain.Entities.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CodePostal")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Coordianates")
                        .IsRequired()
                        .HasMaxLength(5000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Itinerary", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<Guid>("PlaceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StationId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("PlaceId");

                    b.HasIndex("StationId");

                    b.ToTable("Itineraries");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Line", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("nvarchar(7)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Lines");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Place", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Adresse")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Lat")
                        .HasColumnType("decimal(18,16)");

                    b.Property<decimal>("Lon")
                        .HasColumnType("decimal(18,16)");

                    b.HasKey("Id");

                    b.ToTable("Places");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Station", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Lat")
                        .HasColumnType("decimal(18,16)");

                    b.Property<Guid>("LineId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Lon")
                        .HasColumnType("decimal(18,16)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("CityId");

                    b.HasIndex("LineId");

                    b.ToTable("Stations");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Itinerary", b =>
                {
                    b.HasOne("OptiTrajet.Domain.Entities.Place", "Place")
                        .WithMany()
                        .HasForeignKey("PlaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OptiTrajet.Domain.Entities.Station", "Station")
                        .WithMany()
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Place");

                    b.Navigation("Station");
                });

            modelBuilder.Entity("OptiTrajet.Domain.Entities.Station", b =>
                {
                    b.HasOne("OptiTrajet.Domain.Entities.City", "City")
                        .WithMany()
                        .HasForeignKey("CityId");

                    b.HasOne("OptiTrajet.Domain.Entities.Line", "Line")
                        .WithMany()
                        .HasForeignKey("LineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("City");

                    b.Navigation("Line");
                });
#pragma warning restore 612, 618
        }
    }
}
