﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Backendify.Api.Repositories
{
    public partial class CompanyRepositoryModel
    {
        partial void Initialize()
        {
            var company = CompanyEntityType.Create(this);

            CompanyEntityType.CreateAnnotations(company);

            AddAnnotation("ProductVersion", "6.0.7");
        }
    }
}