﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Database;
using DemoModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemoBackend.Migrations
{
    [DbContext(typeof(SmDemoProductContext))]
    [Migration("20221221090923_proba")]
    partial class proba
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DemoModels.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool?>("Active")
                        .HasColumnType("boolean")
                        .HasColumnName("active");

                    b.Property<string>("Code")
                        .HasColumnType("text")
                        .HasColumnName("code");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<double?>("Price")
                        .HasColumnType("double precision")
                        .HasColumnName("price");

                    b.Property<int?>("Rating")
                        .HasColumnType("integer")
                        .HasColumnName("rating");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("releasedate");

                    b.Property<List<InventoryStock>>("Stocks")
                        .HasColumnType("jsonb")
                        .HasColumnName("stocks");

                    b.Property<int?>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_product");

                    b.ToTable("product", (string)null);
                });

            modelBuilder.Entity("DemoModels.ProductExt", b =>
                {
                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("productid");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<double?>("MinimumStock")
                        .HasColumnType("double precision")
                        .HasColumnName("minimumstock");

                    b.HasKey("ProductId")
                        .HasName("pk_productext");

                    b.ToTable("productext", (string)null);
                });

            modelBuilder.Entity("DemoModels.ProductExt", b =>
                {
                    b.HasOne("DemoModels.Product", null)
                        .WithOne("Ext")
                        .HasForeignKey("DemoModels.ProductExt", "ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_productext_product_productid");
                });

            modelBuilder.Entity("DemoModels.Product", b =>
                {
                    b.Navigation("Ext");
                });
#pragma warning restore 612, 618
        }
    }
}
