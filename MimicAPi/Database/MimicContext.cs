﻿using Microsoft.EntityFrameworkCore;
using MimicAPi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPi.Database
{
    public class MimicContext : DbContext 
    {
        public MimicContext(DbContextOptions<MimicContext> options) : base(options)
        {

        }
        public DbSet<Palavra> Palavras { get; set; }
    }
}
