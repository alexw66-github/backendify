﻿// <auto-generated />
using System;
using System.Reflection;
using Backendify.Api.Entities;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Backendify.Api.Repositories
{
    internal partial class CompanyEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Backendify.Api.Entities.Company",
                typeof(Company),
                baseEntityType);

            var id = runtimeEntityType.AddProperty(
                "Id",
                typeof(string),
                propertyInfo: typeof(Company).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var countryCode = runtimeEntityType.AddProperty(
                "CountryCode",
                typeof(string),
                propertyInfo: typeof(Company).GetProperty("CountryCode", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<CountryCode>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var closed = runtimeEntityType.AddProperty(
                "Closed",
                typeof(DateTime?),
                propertyInfo: typeof(Company).GetProperty("Closed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<Closed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var companyName = runtimeEntityType.AddProperty(
                "CompanyName",
                typeof(string),
                propertyInfo: typeof(Company).GetProperty("CompanyName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<CompanyName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var isNullPlaceholder = runtimeEntityType.AddProperty(
                "IsNullPlaceholder",
                typeof(bool),
                propertyInfo: typeof(Company).GetProperty("IsNullPlaceholder", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<IsNullPlaceholder>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var opened = runtimeEntityType.AddProperty(
                "Opened",
                typeof(DateTime?),
                propertyInfo: typeof(Company).GetProperty("Opened", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<Opened>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var taxId = runtimeEntityType.AddProperty(
                "TaxId",
                typeof(string),
                propertyInfo: typeof(Company).GetProperty("TaxId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Company).GetField("<TaxId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var key = runtimeEntityType.AddKey(
                new[] { id, countryCode });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
